using PXLib.Helpers;
using PXLib.Net.IOCP.Contract;
using PXLib.Net.Server;
using PXLib.Net.WebSocketHandler;
using PXLib.ObjectManage.ObjectManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PXLib.Net.IOCP
{
    public class SocketServer
    {
        private static LogHelper log = new LogHelper("IOCPServer");
        private Socket listenSocket;            // 服务端的Socket
        private Semaphore maxNumberAcceptedClients;//最大接受请求数信号量 //限制访问接收连接的线程数，用来控制最大并发数

        public ConcurrentStack<SocketAsyncEventArgs> recvStackPool = new ConcurrentStack<SocketAsyncEventArgs>();//线程安全的
        public ConcurrentStack<SocketAsyncEventArgs> sendStackPool = new ConcurrentStack<SocketAsyncEventArgs>();

        private int receiveBufferSize = 1024 * 8;
        public int ReceiveBufferSize { set { this.receiveBufferSize = value; } }//接收缓冲区的大小
        public int MaxConnectionCount { get; private set; }  //最大连接数量


        private long receiveTotalBytes;//服务器收到的总字节数
        public long ReceiveTotalBytes { get { return this.receiveTotalBytes; } }


        private long sendTotalBytes;//服务器发送的总字节数
        public long SendTotalBytes { get { return this.sendTotalBytes; } }


        private int connectedSocketsCount = 0;//已经连接到服务器的socket数量
        public int ConnectedSocketsCount { get { return this.connectedSocketsCount; } }//已连接数
        /// <summary>
        /// 通过哪个IP地址提供服务。如果设为null，则表示绑定本地所有IP。默认值为null。（如果要set该属性，则必须在调用Start方法之前设置才有效。）
        /// </summary>
        public string IPAddressBinding { get; set; }
        private int maxPacketLength = 1024 * 1024 * 5;
        /// <summary>
        /// 设置保护最大包的长度默认5M  在start方法之前设置生效
        /// </summary>
        public int MaxPacketLength { get { return maxPacketLength; } set { maxPacketLength = value; } }
        private int maxUserIdlength = 20;
        /// <summary>
        /// UserId长度，默认50 （如果要set该属性，则必须在调用Start方法之前设置才有效。）
        /// </summary>
        public int MaxUserIdLength { get { return this.maxUserIdlength; } set { this.maxUserIdlength = value; } }
        public ObjectManager<string, UserTokenEventArgs> UserDic { get; set; } // 接收数据事件对象集合
        private ICustomizeHandler customizeHander;
        private int heartBeatSpanInSecs = 60;
        //设置检查心跳包时间单位秒 默认60秒 <=0则不启动心跳检查
        public int HeartBeatSpanInSecs { get { return this.heartBeatSpanInSecs; } set { this.heartBeatSpanInSecs = value; } }
        Thread tCheckClientHeartbeat;
        public int Port { get; private set; }//监听端口
        #region 事件定义
        public event EventHandler<UserTokenEventArgs> OnClientConnect;        // 客户端已经连接事件

        public event EventHandler<UserTokenEventArgs> OnClientRead;           // 接收到数据事件

        public event EventHandler<UserTokenEventArgs> OnClientDisconnect;     // 客户端断开连接事件

        public event EventHandler<Exception> OnClientError;     // 客户端错误事件

        public event EventHandler<UserTokenEventArgs> OnDataSendCompleted; // 数据发送完成

        #endregion
        public SocketServer(int maxConnectionCount)
        {
            this.MaxConnectionCount = maxConnectionCount;
            this.UserDic = new ObjectManager<string, UserTokenEventArgs>();

            for (int i = 0; i <= maxConnectionCount; i++) //按照连接数建立读写对象
            {
                byte[] receiveBuffer = new byte[receiveBufferSize];
                UserTokenEventArgs userToken = new UserTokenEventArgs();

                userToken.RecvSocketAsyncEventArgs.SetBuffer(receiveBuffer, 0, receiveBufferSize);
                userToken.RecvSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                recvStackPool.Push(userToken.RecvSocketAsyncEventArgs);

                SocketAsyncEventArgs sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
                sendSocketAsyncEventArgs.SetBuffer(receiveBuffer, 0, receiveBufferSize);
                sendSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                sendStackPool.Push(sendSocketAsyncEventArgs);

            }
            maxNumberAcceptedClients = new Semaphore(maxConnectionCount, maxConnectionCount);      // 初始信号量

            tCheckClientHeartbeat = new Thread(CheckClientHeartbeat);//开启新线程检查心跳
            tCheckClientHeartbeat.IsBackground = true;
            tCheckClientHeartbeat.Start();
        }
        /// <summary>
        /// 启动
        /// </summary>
        public void Start(int listenPort, ICustomizeHandler customizeHander)
        {
            this.customizeHander = customizeHander;
            if (string.IsNullOrEmpty(this.IPAddressBinding))
            {
                IPAddressBinding = IPHelper.GetLocalIPAddress();
            }
            this.Port = listenPort;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(this.IPAddressBinding), this.Port);
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(this.MaxConnectionCount);//?表示能同时连接的最大数量
            StartAccept(null);

        }
        public void ShutDown() //关闭服务器
        {
            if (this.customizeHander != null)
            {
                customizeHander.SeverShutDown();
            }
            if (this.listenSocket != null)
            {
                this.listenSocket.Close();//停止侦听
            }
            foreach (UserTokenEventArgs userToken in this.UserDic.GetAll())
            {
                try
                {

                    EventHandler<UserTokenEventArgs> handler = OnClientDisconnect;
                    if ((handler != null) && (null != userToken))
                    {
                        handler(this, userToken);//抛出连接断开事件
                    }


                }
                catch { }
            }
            this.UserDic.Clear();
            maxNumberAcceptedClients.Release();
            recvStackPool.Clear();
            sendStackPool.Clear();
            tCheckClientHeartbeat.Abort();//终止线程
            tCheckClientHeartbeat.Join();
        }
        /// <summary>
        /// 客户端是否在线
        /// </summary>
        public bool IsOnline(string connectionId)
        {
            return this.UserDic.Contains(connectionId);
        }
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            }
            else
            {
                e.AcceptSocket = null;
            }
            maxNumberAcceptedClients.WaitOne();//开始 使用信号量时，可以多个线程同时访问受保护的资源
            bool willRaiseEvent = listenSocket.AcceptAsync(e);//异步开始接受连接请求
            if (!willRaiseEvent)
            {
                ProcessAccept(e);
            }
        }
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {

            switch (e.LastOperation)  // 确定刚刚完成的操作类型并调用关联的句柄
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    //throw new ArgumentException("最后一次在Socket上的操作不是接收或者发送操作");
                    log.WriteLog("________LastOperation:" + e.LastOperation);
                    break;
            }

        }
        #region 连接 接收数据 发送操作
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Accept)    //检查上一次操作是否是Accept，不是就返回
                return;
            if (e.SocketError == SocketError.Success)
            {
                Socket sock = e.AcceptSocket;
                if (sock != null && sock.Connected)
                {
                    SocketAsyncEventArgs recvSocketAsyncEventArgs = null;
                    if (this.recvStackPool.TryPop(out recvSocketAsyncEventArgs))
                    {
                        UserTokenEventArgs userToken = recvSocketAsyncEventArgs.UserToken as UserTokenEventArgs;
                        userToken.ConnectTime = DateTime.Now;
                        userToken.ActiveTime = DateTime.Now;
                        userToken.UserSocket = sock;
                        Interlocked.Increment(ref this.connectedSocketsCount);
                        // 获得一个新的Guid 32位 "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
                        userToken.ConnectedId = Guid.NewGuid().ToString("N");
                        try
                        {
                            this.UserDic.Add(userToken.ConnectedId, userToken);// 添加到集合中
                            EventHandler<UserTokenEventArgs> handler = OnClientConnect;
                            if (handler != null)
                            {
                                handler(this, userToken);//抛出连接断开事件
                            }
                            if (!userToken.UserSocket.ReceiveAsync(recvSocketAsyncEventArgs))
                            {
                                ProcessReceive(recvSocketAsyncEventArgs);
                            }

                        }
                        catch (Exception ex)
                        {
                            this.RaiseErrorEvent(userToken, ex);
                            this.RaiseDisconnectedEvent(userToken);
                        }
                    }
                    else
                    {
                        sock.Send(Encoding.Default.GetBytes("连接已经达到最大数!")); //已经达到最大客户连接数量，在这接受连接，发送“连接已经达到最大数”，然后断开连接
                        string outStr = String.Format("连接已满，拒绝 {0} 的连接。", sock.RemoteEndPoint);
                        RaiseErrorEvent(null, new Exception(outStr));
                        sock.Close();
                    }

                }
                StartAccept(e);//投递接收请求 
            }
        }
        /// <summary>
        /// 异步接收操作完成时调用.   如果远程主机关闭连接Socket将关闭    
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            UserTokenEventArgs userToken = e.UserToken as UserTokenEventArgs;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                //Console.WriteLine("收到长度" + e.BytesTransferred + "  是否接受完毕" + userToken.UserSocket.Available);
                Interlocked.Add(ref receiveTotalBytes, e.BytesTransferred); // 增加接收到的字节总数   
                userToken.ActiveTime = DateTime.Now;//重置活动时间   
                userToken.ResvBufferManager.WriteBuffer(e.Buffer, e.Offset, e.BytesTransferred);
                try
                {
                    EventHandler<UserTokenEventArgs> handler = OnClientRead;// 抛出接收到数据事件 
                    if (handler != null)
                    {
                        handler(this, userToken);
                    }
                    if (!this.ProcessPacket(userToken))//如果处理数据返回失败，则断开连接 使用内置的处理
                    {
                        RaiseDisconnectedEvent(userToken);
                    }
                    else
                    {
                        if (userToken.UserSocket != null && userToken.UserSocket.Connected)
                        {
                            bool willRaiseEvent = userToken.UserSocket.ReceiveAsync(e); //投递接收请求
                            if (!willRaiseEvent)
                            {
                                ProcessReceive(e);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(userToken, ex);
                    RaiseDisconnectedEvent(userToken);
                }
            }
            else
            {
                RaiseDisconnectedEvent(userToken);
            }
        }
        private bool ProcessPacket(UserTokenEventArgs userToken)
        {
            if (userToken.ClientType != ClientType.WebSocket)//第一次进来握手操作
            {
                byte[] recvMsg = userToken.ResvBufferManager.GetCopyBuffer();
                WebSocketHandshake handshake = new WebSocketHandshake(recvMsg);
                if (handshake.IsWebSocket)
                {
                    userToken.UserSocket.Send(Encoding.UTF8.GetBytes(handshake.Response));
                    userToken.ResvBufferManager.Reset();
                    userToken.ClientType = ClientType.WebSocket;
                }
                handshake = null;
                return true;
            }
            if (userToken.ClientType == ClientType.WebSocket)
            {

                #region 处理WebSocket
                try
                {
                    if (userToken.ResvBufferManager.Length % this.receiveBufferSize == 0) //客户端一次发送的数据大于服务端缓冲区
                    {
                        return true;
                    }
                    if (userToken.ResvBufferManager.Length >= 1024 * 120)
                    {
                        this.RaiseErrorEvent(userToken, new Exception("WebSocket暂不能解析大于120K的单个包"));
                        return false;
                    }
                    while (userToken.ResvBufferManager.Length != 0)
                    {
                        int clearCount = 0;
                        DataFrame dr = new DataFrame(userToken.ResvBufferManager.Buffer, ref clearCount);
                        if (dr.Header.OpCode == OpCode.Close)
                        {
                            this.RaiseErrorEvent(userToken, new Exception("33333333333用户主动断开"));
                            return false;
                        }
                        if (dr.Header.OpCode == OpCode.Binary)
                        {
                            #region 正常解析为二进制


                            MessageHeader header = new MessageHeader();
                            header.ProccessHeader(dr.BinaryContent);
                            if (header.MessageId > UInt16.MaxValue || header.MessageId < 0 || header.PacketLength > this.maxPacketLength | userToken.ResvBufferManager.Length > this.maxPacketLength)
                            {
                                this.RaiseErrorEvent(userToken, new Exception("解析消息类型错误|包长度超过最大长度"));
                                return false;
                            }
                            if (userToken.ResvBufferManager.Length < header.PacketLength + clearCount)//包不足够的情况
                            {
                                return true;
                            }
                            byte[] content = BytesHelper.CopyArrayData(dr.BinaryContent, header.Length, dr.BinaryContent.Length - header.Length);
                            NetMessage netMessage = new NetMessage(header, content);
                            userToken.NetMessage = netMessage;
                            //userToken.ResvBufferManager.SetOffset(header.Length-1);
                            if (this.customizeHander != null)
                            {
                                customizeHander.HandleInformation(userToken);
                                //customizeHander.HandleInformation(userToken, userToken.ConnectedId, netMessage.Header.MessageId, content);
                            }
                            userToken.ResvBufferManager.Clear(header.PacketLength + clearCount);
                            #endregion
                        }
                        else
                        {
                            this.RaiseErrorEvent(userToken, new Exception("WebScoket未能正确解析包 OP代码" + dr.Header.OpCode));
                            userToken.ResvBufferManager.Reset();
                            return true;
                        }
                    }

                }
                catch (Exception ex)
                {
                    this.RaiseErrorEvent(userToken, new Exception("处理WebSocket包出现异常:" + ex.ToString()));
                    return false;
                }
                #endregion
            }
            if (userToken.ClientType == ClientType.DotNET)//继续处理其他平台
            {
                //string userId = userToken.UserSocket.RemoteEndPoint.ToString();
            }
            return true;
        }
        /// <summary>
        /// 这个方法当一个异步发送操作完成时被调用.
        /// </summary>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            UserTokenEventArgs userToken = e.UserToken as UserTokenEventArgs;
            userToken.ActiveTime = DateTime.Now;
            Interlocked.Add(ref sendTotalBytes, e.BytesTransferred); // 增加发送的字节总数 
            // 回收SocketAsyncEventArgs以备再次被利用
            userToken.SendBufferManager.Reset();
            e.UserToken = null; // 清除UserToken对象引用 
            this.sendStackPool.Push(e);
            if (e.SocketError == SocketError.Success)
            {
                //发送完成后做清除，抛出事件等操作 这里暂时什么都不做
                EventHandler<UserTokenEventArgs> handler = OnDataSendCompleted;
                if (handler != null)
                {
                    handler(this, userToken);//抛出客户端发送完成事件
                }
            }
            else
            {
                RaiseDisconnectedEvent(userToken);
            }
        }
        #endregion
        #region 断开客服端 抛出错误事件
        public void RaiseDisconnectedEvent(UserTokenEventArgs userToken)//引发断开连接事件
        {
            if (null != userToken && userToken.UserSocket != null)
            {
                try
                {

                    EventHandler<UserTokenEventArgs> handler = OnClientDisconnect;//最先抛出事件
                    // 如果订户事件将为空(null)
                    if (handler != null)
                    {
                        handler(this, userToken);//抛出连接断开事件
                    }
                    if (this.customizeHander != null && !userToken.ConnectedId.IsNullEmpty())
                        customizeHander.ClientClose(userToken);
                    userToken.Reset();//方便下一次使用

                    if (userToken.UserSocket.Connected)
                    {
                        try
                        {
                            userToken.UserSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch { }

                    }

                    userToken.UserSocket.Close();
                    userToken.UserSocket = null;
                    Interlocked.Decrement(ref connectedSocketsCount);
                    maxNumberAcceptedClients.Release();
                    this.UserDic.Remove(userToken.ConnectedId);
                    recvStackPool.Push(userToken.RecvSocketAsyncEventArgs);// 释放以使它们可以被其他客户端重新利用,总数不变  
                }
                catch (ObjectDisposedException)
                {
                    // 抛出客户处理已经被关闭
                }
                catch (Exception ex)
                {

                    RaiseErrorEvent(userToken, ex);
                }
                finally
                {

                }
            }

        }
        //抛出错误事件
        public void RaiseErrorEvent(UserTokenEventArgs token, Exception ex)
        {
            if (OnClientError != null)
            {
                if (null != token)
                {
                    OnClientError(token, ex);//抛出客户端错误事件
                }
                else
                {
                    OnClientError(null, ex);
                }
            }
        }
        #endregion

        #region 检查心跳程序
        /// <summary>  
        /// 客户端心跳检测  
        /// </summary>  
        private void CheckClientHeartbeat()
        {
            if (this.heartBeatSpanInSecs <= 0)
            {
                tCheckClientHeartbeat.Abort();
                tCheckClientHeartbeat.Join();
                return;
            }
            while (tCheckClientHeartbeat.IsAlive)
            {
                try
                {
                    //Console.WriteLine(DateTime.Now);
                    int iCheckInterval = this.heartBeatSpanInSecs * 1000; //检测间隔  
                    Thread.Sleep(iCheckInterval);
                    List<UserTokenEventArgs> userTokenList = this.UserDic.GetAll();
                    if (userTokenList != null && userTokenList.Count > 0)
                    {
                        foreach (UserTokenEventArgs userToken in userTokenList)
                        {
                            if (userToken.ActiveTime.AddMilliseconds(iCheckInterval).CompareTo(DateTime.Now) < 0)//超时
                            {
                                //if (userToken.UserSocket != null)
                                //{
                                //    userToken.UserSocket.Close(); //服务端主动关闭心跳超时连接 
                                //}
                                RaiseDisconnectedEvent(userToken);//心跳断开
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLog("心跳进程关闭Socket错误" + ex.ToString());
                }
            }
        }
        #endregion
        #region 异步发送
        public void Send(string connectionId, byte[] buffer)
        {
            UserTokenEventArgs userToken = UserDic.Get(connectionId);
            if (userToken == null || userToken.UserSocket == null)
                return;
            if (userToken.ClientType == ClientType.WebSocket)
            {
                buffer = WebSocketUtils.PackServerData(buffer);//或者使用DataFram
            }
            SocketAsyncEventArgs sendEventArgs = null;
            if (this.sendStackPool.TryPop(out sendEventArgs))
            {

                try
                {
                    sendEventArgs.UserToken = userToken;
                    sendEventArgs.SetBuffer(buffer, 0, buffer.Length);//最后设置发送数据
                    //异步发送数据
                    //Array.Copy(buffer, 0, sendEventArgs.Buffer, 0, buffer.Length);//设置发送数据  
                    bool willRaiseEvent = userToken.UserSocket.SendAsync(sendEventArgs);//采用异步发送才能处理sendStackPool
                    if (!willRaiseEvent)
                    {
                        ProcessSend(sendEventArgs);
                    }
                }
                catch (ObjectDisposedException)//调用此处的异步发送可能socket被其他进程关闭异常
                {
                    RaiseDisconnectedEvent(userToken);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(userToken, ex);
                    RaiseDisconnectedEvent(userToken);
                }
            }
        }
        public void Send(string connectionId, int opType, byte[] buffer)
        {
            byte[] content = BytesHelper.MergeBytes(BitConverter.GetBytes((short)opType), buffer);
            Send(connectionId, content);
        }
        public void Send(UserTokenEventArgs userToken)
        {
            if (userToken == null || userToken.UserSocket == null)
                return;
            byte[] buffer = userToken.SendBufferManager.GetCopyBuffer();
            if (userToken.ClientType == ClientType.WebSocket)
            {
                buffer = WebSocketUtils.PackServerData(buffer);//或者使用DataFram
            }
            SocketAsyncEventArgs sendEventArgs = null;
            if (this.sendStackPool.TryPop(out sendEventArgs))
            {
                try
                {
                    sendEventArgs.UserToken = userToken;
                    sendEventArgs.SetBuffer(buffer, 0, buffer.Length);//最后设置发送数据
                    //异步发送数据
                    //Array.Copy(buffer, 0, sendEventArgs.Buffer, 0, buffer.Length);//设置发送数据  
                    bool willRaiseEvent = userToken.UserSocket.SendAsync(sendEventArgs);//采用异步发送才能处理sendStackPool
                    if (!willRaiseEvent)
                    {
                        ProcessSend(sendEventArgs);
                    }
                }
                catch (ObjectDisposedException)//调用此处的异步发送可能socket被其他进程关闭异常
                {
                    RaiseDisconnectedEvent(userToken);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(userToken, ex);
                    RaiseDisconnectedEvent(userToken);
                }
            }
        }
        #endregion

    }
}

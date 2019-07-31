
using PXLib.Helpers;
using PXLib.Net.Server;
using PXLib.Net.WebSocketHandler;
using PXLib.ObjectManage.ObjectManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PXLib.Net
{
    public class SocketServer
    {
        private Socket listenSocket;            // 服务端的Socket
        private Semaphore maxNumberAcceptedClients;//最大接受请求数信号量 //限制访问接收连接的线程数，用来控制最大并发数
        private SocketAsyncEventArgsStackPool recvStackPool;
        private SocketAsyncEventArgsStackPool sendStackPool;
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
        /// UserId长度，默认20 （如果要set该属性，则必须在调用Start方法之前设置才有效。）
        /// </summary>
        public int MaxUserIdLength { get; set; }
        //----------------------------------------------------
        public ObjectManager<string, UserTokenEventArgs> UserDic { get; set; } // 接收数据事件对象集合
        private ICustomizeHandler customizeHander;
        private IBasicHandler basicHandler;
        private int heartBeatSpanInSecs = 60;
        //设置检查心跳包时间单位秒 默认60秒 <=0则不启动心跳检查
        public int HeartBeatSpanInSecs { get { return this.heartBeatSpanInSecs; } set { this.heartBeatSpanInSecs = value; } }
        Thread tCheckClientHeartbeat;
        public int Port { get; private set; }//监听端口
        #region 事件定义
        public event EventHandler<UserTokenEventArgs> OnClientConnect;        // 客户端已经连接事件

        public event EventHandler<UserTokenEventArgs> OnClientRead;           // 接收到数据事件

        public event EventHandler<UserTokenEventArgs> OnClientDisconnect;     // 客户端断开连接事件

        public event EventHandler<SocketServerErrorEventArgs> OnClientError;     // 客户端错误事件

        // public event EventHandler<AsyncSocketUserToken> OnDataSendCompleted; // 数据发送完成

        #endregion
        public SocketServer(int maxConnectionCount)
        {
            this.MaxConnectionCount = maxConnectionCount;
            this.recvStackPool = new SocketAsyncEventArgsStackPool(maxConnectionCount);//初始化对象池
            this.sendStackPool = new SocketAsyncEventArgsStackPool(maxConnectionCount);
            this.UserDic = new ObjectManager<string, UserTokenEventArgs>();
            UserTokenEventArgs userToken;
            for (int i = 0; i <= maxConnectionCount; i++) //按照连接数建立读写对象
            {
                userToken = new UserTokenEventArgs(this.receiveBufferSize);
                userToken.ReceiveSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                recvStackPool.Push(userToken.ReceiveSocketAsyncEventArgs);//把userToken的SocketAsyncEventArgs装入对象池
                sendStackPool.Push(userToken.SendSocketAsyncEventArgs);
            }
            maxNumberAcceptedClients = new Semaphore(maxConnectionCount, maxConnectionCount);      // 初始信号量

            tCheckClientHeartbeat = new Thread(CheckClientHeartbeat);//开启新线程检查心跳
            tCheckClientHeartbeat.IsBackground = true;
            tCheckClientHeartbeat.Start();
        }
        /// <summary>
        /// 启动
        /// </summary>
        public void Start(int listenPort, ICustomizeHandler customizeHander, IBasicHandler basicHandler)
        {
            this.customizeHander = customizeHander;
            this.basicHandler = basicHandler;
            if (string.IsNullOrEmpty(this.IPAddressBinding))
            {
                IPAddressBinding = IPHelper.GetLocalIPAddress();
            }
            this.Port = listenPort;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(this.IPAddressBinding), this.Port);
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(this.MaxConnectionCount);//?表示能同时连接的最大数量
            StartAcceptRequset(null); // (第一次不采用可重用的SocketAsyncEventArgs对象来接受请求的Socket的Accept方式)  
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
            recvStackPool.Cleal();
            sendStackPool.Cleal();
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
        #region 处理连接
        /// <summary>
        /// 开始接受连接请求
        /// </summary>
        /// <param name="acceptEventArg"></param>
        private void StartAcceptRequset(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            try
            {
                if (socketAsyncEventArgs == null)//第一次
                {
                    socketAsyncEventArgs = new SocketAsyncEventArgs();
                    socketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptConnect_Completed);//注册连接事件
                }
                else
                {
                    socketAsyncEventArgs.AcceptSocket = null;  //第二次调用时释放之前的scoket
                }
                this.maxNumberAcceptedClients.WaitOne(); //获取信号量

                //这里注意的是，每个操作方法返回的是布尔值，这个布尔值的作用，是表明当前操作是否有等待I/O的情况，如果返回false则表示当前是同步操作，不需要等待，此时要要同步执行回调方法，一般写法是
                bool willRaiseEvent = listenSocket.AcceptAsync(socketAsyncEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessAcceptConnect(socketAsyncEventArgs);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("StartAcceptRequset调试" + ex.ToString());
            }

        }
        private void AcceptConnect_Completed(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {

            ProcessAcceptConnect(socketAsyncEventArgs);

        }
        private void ProcessAcceptConnect(SocketAsyncEventArgs e)//真正处理连接进来的socket
        {
            if (e.LastOperation != SocketAsyncOperation.Accept)    //检查上一次操作是否是Accept，不是就返回
                return;
            if (e.SocketError == SocketError.Success)
            {
                Socket sock = e.AcceptSocket;
                if (sock != null && sock.Connected)
                {
                    Interlocked.Increment(ref this.connectedSocketsCount);
                    SocketAsyncEventArgs tempAgrs = this.recvStackPool.Pop();
                    UserTokenEventArgs userToken = tempAgrs.UserToken as UserTokenEventArgs;
                    try
                    {

                        userToken.ConnectTime = DateTime.Now;
                        userToken.ActiveTime = DateTime.Now;
                        userToken.Socket = sock;

                        if (!userToken.Socket.ReceiveAsync(userToken.ReceiveSocketAsyncEventArgs))
                        {
                            this.ProcessReceive(userToken.ReceiveSocketAsyncEventArgs);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.RaiseErrorEvent(userToken, ex);
                    }
                    this.StartAcceptRequset(e);
                }
            }
        }
        #endregion
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {

            switch (e.LastOperation)  // 确定刚刚完成的操作类型并调用关联的句柄
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    //throw new ArgumentException("最后一次在Socket上的操作不是接收或者发送操作");
                    break;
            }

        }

        /// <summary>
        /// 异步接收操作完成时调用.   如果远程主机关闭连接Socket将关闭    
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            UserTokenEventArgs userToken = e.UserToken as UserTokenEventArgs;
            if (userToken == null || userToken.Socket == null)
                return;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success && userToken.Socket.Connected)  // 检查远程主机是否关闭连接
            {
                Interlocked.Add(ref receiveTotalBytes, e.BytesTransferred); // 增加接收到的字节总数              
                userToken.ActiveTime = DateTime.Now;//重置活动时间         
                if (userToken.Socket.Available == 0)    //判断所有需接收的数据是否已经完成  
                {
                    //从侦听者获取接收到的消息。   
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, data.Length);//从e.Buffer块中复制数据出来，保证它可重用  
                    userToken.bufferManager.WriteBuffer(data);
                }
                try
                {
                    if (!this.ProcessPacket(userToken))//如果处理数据返回失败，则断开连接
                    {
                        CloseClientSocket(userToken);
                    }
                    else
                    {
                        EventHandler<UserTokenEventArgs> handler = OnClientRead;// 抛出接收到数据事件 
                        if (handler != null)
                        {
                            handler(this, userToken);
                        }
                        // 继续接收数据
                        bool willRaiseEvent = userToken.Socket.ReceiveAsync(userToken.ReceiveSocketAsyncEventArgs); //投递接收请求
                        if (!willRaiseEvent)
                        {
                            ProcessReceive(userToken.ReceiveSocketAsyncEventArgs);
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    CloseClientSocket(userToken);
                }
                catch (SocketException socketException)
                {
                    if (socketException.ErrorCode == (int)SocketError.ConnectionReset)//10054一个建立的连接被远程主机强行关闭
                    {
                        CloseClientSocket(userToken);//引发断开连接事件
                    }
                    else
                    {
                        RaiseErrorEvent(userToken, new Exception("在SocketAsyncEventArgs对象上执行异步接收数据操作时发生SocketException异常", socketException));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ProcessReceive调试" + ex.ToString());
                }
            }
            else
            {
                CloseClientSocket(userToken);
            }
            Console.WriteLine("已连接数：" + this.connectedSocketsCount + "__LoginUserDic数：" + UserDic.Count + "--接收Stack数：" + this.recvStackPool.Count + "--发送Stack数：" + this.sendStackPool.Count);
        }
        private bool ProcessPacket(UserTokenEventArgs userToken)
        {
            if (userToken.ClientType != ClientType.WebSocket)//第一次进来握手操作
            {
                byte[] info = userToken.bufferManager.GetCopyBuffer();
                WebSocketHandshake handshake = new WebSocketHandshake(info);
                if (handshake.IsWebSocket)
                {
                    userToken.Socket.Send(Encoding.UTF8.GetBytes(handshake.Response));
                    userToken.bufferManager.Reset();
                    userToken.ClientType = ClientType.WebSocket;
                    handshake = null;
                    return true;
                }
            }
         
            if (userToken.ClientType == ClientType.WebSocket)
            {
                int bodyLen = 0;
                int informationType = -1;
                #region 处理WebSocket
                while (userToken.ResvBufferManager.Length > 6)
                {
                    if (userToken.ResvBufferManager.Length % this.receiveBufferSize == 0) //客户端一次发送的数据大于服务端缓冲区
                    {
                        return true;
                    }
                    if (userToken.ResvBufferManager.Length >= 1024 * 120)
                    {
                        this.RaiseErrorEvent(userToken, new Exception("单个包发送不能大于120K"));
                        return false;
                    }
                    int clearCount = 0;

                    DataFrame dr = new DataFrame(userToken.ResvBufferManager.Buffer, ref clearCount);
                    if (dr.Header.OpCode == OpCode.Close)
                    {

                        this.RaiseErrorEvent(userToken, new Exception("用户主动断开"));
                        return false;
                    }
                    if (dr.Header.OpCode == OpCode.Binary)
                    {
                        #region 正常解析为二进制

                        byte[] hasAnalyzeData = dr.BinaryContent;
                        bodyLen = BitConverter.ToInt32(hasAnalyzeData, 0);//包长
                        if (userToken.ResvBufferManager.Length >= bodyLen + clearCount)//包足够的情况
                        {
                            informationType = BitConverter.ToInt16(hasAnalyzeData, 4);//消息类型
                            if (informationType > Int16.MaxValue || informationType < 0 || bodyLen > this.maxPacketLength | userToken.ResvBufferManager.Length > this.maxPacketLength)
                            {
                                this.RaiseErrorEvent(userToken, new Exception("解析消息类型错误|包长度超过最大长度"));
                                return false;
                            }
                            if (informationType == 998)
                            {
                                int userIdLen = hasAnalyzeData[6];
                                if (userIdLen > this.maxUserIdlength)
                                {
                                    this.RaiseErrorEvent(userToken, new Exception("UserId长度过长"));
                                    return false;
                                }
                                string userId = Encoding.UTF8.GetString(BytesHelper.CopyArrayData(hasAnalyzeData, 7, this.maxUserIdlength)).Replace("\0", "");
                                string failCause = string.Empty;
                                if (this.customizeHander != null && !customizeHander.CheckLoginUser("ABC", userId, "", out failCause))
                                {
                                    this.RaiseErrorEvent(userToken, new Exception(failCause));
                                    return false;
                                }
                                this.UserDic.Add(userId, userToken);//添加登录用户才可以发送
                                userToken.ConnectedId = userId;
                                userToken.SendBufferManager.WriteInt16((short)informationType);
                                userToken.SendBufferManager.WriteString0(failCause);
                                this.Send(userId, userToken.SendBufferManager.GetCopyBuffer());
                                Console.WriteLine(string.Format("用户{0}登录成功---" + DateTime.Now, userId));
                                userToken.ResvBufferManager.Reset();
                                userToken.SendBufferManager.Reset();

                            }
                            else
                            {
                                if (string.IsNullOrEmpty(userToken.ConnectedId))
                                {
                                    this.RaiseErrorEvent(userToken, new Exception(string.Format("用户{0}未登录;InfomationType:{1}", userToken.UserSocket.RemoteEndPoint, informationType)));
                                    return false;
                                }
                                if (hasAnalyzeData.Length < bodyLen)
                                {
                                    Console.WriteLine("***********************" + bodyLen);
                                    userToken.ResvBufferManager.Clear(clearCount);
                                    //return true;
                                }

                                byte[] content = BytesHelper.CopyArrayData(hasAnalyzeData, 6, hasAnalyzeData.Length - 6);
                                if (this.customizeHander != null)
                                    customizeHander.HandleInformation(userToken, userToken.ConnectedId, informationType, content);

                                // Console.WriteLine(" FBL:" + userToken.bufferManager.Length + " BOdyLen:" + bodyLen + " HL:" + hasAnalyzeData.Length + " clearCount:" + clearCount);
                                userToken.ResvBufferManager.Clear(bodyLen + clearCount);
                            }

                        }
                        else
                        {
                            return true;
                        }

                        #endregion
                    }
                    else
                    {
                        Console.WriteLine("OP:" + dr.Header.OpCode + "____出现此错误可能包长度不对");
                        userToken.ResvBufferManager.Reset(true);
                        return true;
                    }
                }
                #endregion
            }
            if (userToken.ClientType == ClientType.DotNET)//继续处理其他平台
            {
              
            }
            return true;
        }


        /// <summary>
        /// 这个方法当一个异步发送操作完成时被调用.
        /// </summary>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            UserTokenEventArgs userToken = (UserTokenEventArgs)e.UserToken;
            // 回收SocketAsyncEventArgs以备再次被利用
            //Interlocked.Add(ref sendTotalBytes, e.BytesTransferred); // 增加发送的字节总数 
            lock (sendStackPool)
            {
                sendStackPool.Push(userToken.SendSocketAsyncEventArgs);
            }
            // 清除UserToken对象引用 
            e.UserToken = null;
            if (e.SocketError == SocketError.Success)
            {
                //发送完成后做清除，抛出事件等操作 这里暂时什么都不做
            }
            else
            {
                CloseClientSocket(userToken);
            }
        }
        #region 断开客服端 抛出错误事件
        //关闭连接进来的Socket
        public void CloseClientSocket(UserTokenEventArgs userToken)
        {

            if (userToken != null)
            {
                if (userToken.Socket == null)
                    return;
                try
                {

                    if (this.customizeHander != null && !userToken.CurrentUserID.IsNullEmpty())
                        customizeHander.ClientClose(userToken, userToken.CurrentUserID);

                    EventHandler<UserTokenEventArgs> handler = OnClientDisconnect;   // 如果订户事件将为空(null)               
                    if ((handler != null) && userToken.Socket != null)
                    {
                        handler(this, userToken);//抛出连接断开事件
                    }
                    if (userToken.Socket.Connected)
                    {
                        userToken.Socket.Shutdown(SocketShutdown.Both);
                    }
                    userToken.Socket.Close();
                    userToken.Socket = null;
                    userToken.SetInit();
                    this.maxNumberAcceptedClients.Release();
                    lock (recvStackPool)
                    {
                        recvStackPool.Push(userToken.ReceiveSocketAsyncEventArgs);  // 释放以使它们可以被其他客户端重新利用,总数不变
                    }
                }
                // 抛出客户处理已经被关闭
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("关闭对象已经被释放");
                }
                catch (SocketException)
                {

                }
                catch (Exception exception_debug)
                {

                    Console.WriteLine("关闭Socket调试:" + exception_debug.ToString());
                    //throw exception_debug;
                }
                finally
                {
                    if (connectedSocketsCount > 0) //减少连接到服务器的socket计数器
                    {
                        Interlocked.Decrement(ref connectedSocketsCount);
                    }
                    //this.UserDic.RemoveByValue(userToken);
                    if (!userToken.CurrentUserID.IsNullEmpty())
                        this.UserDic.Remove(userToken.CurrentUserID);
                }
            }
        }
        //抛出错误事件
        private void RaiseErrorEvent(UserTokenEventArgs token, Exception exception)
        {
            if (OnClientError != null)
            {
                if (null != token)
                {
                    OnClientError(token, new SocketServerErrorEventArgs(exception));//抛出客户端错误事件
                }
                else
                {
                    OnClientError(null, new SocketServerErrorEventArgs(exception));//抛出服务器错误事件
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

                            if (userToken.ActiveTime.AddMilliseconds(iCheckInterval).CompareTo(DateTime.Now) < 0)
                            {
                                if (userToken.Socket != null)
                                {
                                    try
                                    {
                                        string sClientIP = userToken.Socket.RemoteEndPoint.ToString();
                                        Console.WriteLine(sClientIP + " the heartbeat timeout ！");
                                    }
                                    catch { }
                                    CloseClientSocket(userToken); //服务端主动关闭心跳超时连接
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }
        #endregion
        public void Send(string connectionId, byte[] buffer)
        {
            UserTokenEventArgs userToken = UserDic.Get(connectionId);
            if (userToken == null || userToken.Socket == null)
                return;
            if (userToken.ClientType == ClientType.WebSocket)
            {
                buffer = WebSocketUtils.PackServerData(buffer);//或者使用DataFram
            }
            SocketAsyncEventArgs sendEventArgs = null;
            lock (sendStackPool)
            {
                sendEventArgs = sendStackPool.Pop();
            }
            if (sendEventArgs != null && sendEventArgs.SocketError == SocketError.Success)
            {
                sendEventArgs.UserToken = userToken;
                sendEventArgs.SetBuffer(buffer, 0, buffer.Length);//最后设置发送数据
                try
                {
                    //异步发送数据
                    //Array.Copy(buffer, 0, sendEventArgs.Buffer, 0, buffer.Length);//设置发送数据  
                    bool willRaiseEvent = userToken.Socket.SendAsync(sendEventArgs);//采用异步发送才能处理sendStackPool
                    if (!willRaiseEvent)
                    {
                        ProcessSend(sendEventArgs);
                    }

                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(userToken, ex);
                    Console.WriteLine("Send调试" + ex.ToString());
                }
            }
        }
        public void Send(string connectionId, int opType, byte[] buffer)
        {
            byte[] content = BytesHelper.MergeBytes(BitConverter.GetBytes((short)opType), buffer);
            Send(connectionId, content);
        }
    }
}

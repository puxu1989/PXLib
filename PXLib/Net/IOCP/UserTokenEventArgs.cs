using PXLib.Net.IOCP.Contract;
using PXLib.ObjectManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Net.IOCP
{
    public class UserTokenEventArgs : EventArgs
    {
        public ClientType ClientType { get; set; }
        /// <summary>
        /// 活动时间
        /// </summary>
        public DateTime ActiveTime { get; set; }
        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; }
        public Socket UserSocket { get; set; }//异步接收或发送的socket
        public string ConnectedId { get; set; }
        //public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs { get; private set; }//用于接收对象
        //public SocketAsyncEventArgs SendSocketAsyncEventArgs { get; private set; }//用于发送对象 （因为异步 不创建两个则会被同时使用异常）
        public BufferManager ResvBufferManager { get; set; }
        public BufferManager SendBufferManager { get; set; }
        public BufferManager BlobMsg { get; set; }
        /// <summary>
        /// 解析好的NetMessage
        /// </summary>
        public NetMessage NetMessage { get; set; }
        public bool IsLogin { get; set; }
        private SocketAsyncEventArgs recvSocketAsyncEventArgs;
        public SocketAsyncEventArgs RecvSocketAsyncEventArgs { get{return this.recvSocketAsyncEventArgs;}set{this.recvSocketAsyncEventArgs=value;} }
        public UserTokenEventArgs()
        {
            this.recvSocketAsyncEventArgs = new SocketAsyncEventArgs();        
            this.ClientType = ClientType.DotNET; 
            this.ResvBufferManager = new BufferManager();
            this.SendBufferManager = new BufferManager();
            this.BlobMsg = new BufferManager();
            this.recvSocketAsyncEventArgs.UserToken = this;
        }
        /// <summary>
        /// 重置BufferManager的_offset
        /// </summary>
        public void Reset()
        {
            this.ClientType = ClientType.DotNET;
            this.IsLogin = false;
            this.ResvBufferManager.Reset();
            this.SendBufferManager.Reset();
            this.BlobMsg.Reset();
        }
    }
}

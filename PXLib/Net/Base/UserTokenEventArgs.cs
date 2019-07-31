using PXLib.ObjectManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;
using PXLib.Net.WebSocketHandler;
using PXLib.Helpers;

namespace PXLib.Net
{
    public class UserTokenEventArgs : EventArgs
    {
        public DateTime ActiveTime { get; set; }
        public DateTime ConnectTime { get; set; }
        public Socket UserSocket { get; set; }//异步接收或发送的socket

        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs { get; private set; }//用于接收对象
        public SocketAsyncEventArgs SendSocketAsyncEventArgs { get; private set; }//用于发送对象 （因为异步 不创建两个则会被同时使用异常）
        public BufferManager bufferManager { get; set; }
        public BufferManager SendBufferManager { get; set; }
        public ClientType ClientType { get; set; }
        public BufferManager BlobMsg { get; set; }
        public string CurrentUserID { get; set; }
        public UserTokenEventArgs(int receiveBufferSize)
        {
            byte[] receiveBuffer= new byte[receiveBufferSize];
            this.ReceiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
            this.ReceiveSocketAsyncEventArgs.SetBuffer(receiveBuffer, 0, receiveBufferSize);
            this.ReceiveSocketAsyncEventArgs.UserToken = this;
            this.SendSocketAsyncEventArgs = new SocketAsyncEventArgs();
            this.SendSocketAsyncEventArgs.SetBuffer(receiveBuffer, 0, receiveBufferSize);
            this.SendSocketAsyncEventArgs.UserToken = this;
            SetInit();//初始化变量
        }
        public void SetInit()
        {
            this.ClientType = ClientType.DotNET;
            this.BlobMsg = new BufferManager();
            this.bufferManager = new BufferManager();
            this.SendBufferManager = new BufferManager();
        }
    }
   

   //自定义的错误事件参数
   public class SocketServerErrorEventArgs : EventArgs
   {
       public Exception ex;//自定义错误消息
       public SocketServerErrorEventArgs(Exception ex)
       {
           this.ex = ex;
       }
   }
}

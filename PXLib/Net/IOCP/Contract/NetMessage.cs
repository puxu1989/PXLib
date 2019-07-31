using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Net.IOCP.Contract
{
   public class NetMessage
    {
        private MessageHeader header;
        public MessageHeader Header
        {
            get { return header; }
            set { header = value; }
        }
        private byte[] body = null;
        /// <summary>
        /// Body 消息主体，可以经过变换
        /// </summary>
        public byte[] Body
        {
            get { return body; }
            set { body = value; }
        }
       private int length;
        public int Length 
        {
            get { return this.length; }
        }
        /// <summary>
        /// NetMessage 本Ctor说明如果body不为null，则BodyOffset为0，且this.Header.MessageBodyLength = this.Body.Length 
        /// </summary>	
        public NetMessage(MessageHeader header, byte[] body)
        {
            this.Header = header;
            this.Body = body;

            if (this.Body == null)
            {
                this.length = header.Length;
            }
            else
            {
                this.length = header.Length + body.Length;
            }

        }		

    }
}

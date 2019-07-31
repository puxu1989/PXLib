using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Net.IOCP.Contract
{
    public class MessageHeader
    {
        /// <summary>
        ///本消息主体的长度
        /// </summary>
        public int MessageBodyLength { get; private set; }
        public int PacketLength { get; set; }
        public UInt16 MessageId { get; set; }
        /// <summary>
        /// 包头固定长度 4+2
        /// </summary>
        public int Length { get { return sizeof(int) + sizeof(ushort); } }
        public MessageHeader()
        {
            
        }
        public void ProccessHeader(byte[] buffer) 
        {
            this.PacketLength = BitConverter.ToInt32(buffer, 0);//获取包长
            this.MessageId = BitConverter.ToUInt16(buffer, 4);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Net.WebSocketHandler
{
    /*
   4位操作码，定义有效负载数据，如果收到了一个未知的操作码，连接也必须断掉，以下是定义的操作码：
   *  %x0 表示连续消息片断
   *  %x1 表示文本消息片断
   *  %x2 表未二进制消息片断
   *  %x3-7 为将来的非控制消息片断保留的操作码
   *  %x8 表示连接关闭
   *  %x9 表示心跳检查的ping
   *  %xA 表示心跳检查的pong
   *  %xB-F 为将来的控制消息片断的保留操作码
 */
    public class OpCode
    {
        public const int Continue = 0;
        public const int Text = 1;
        public const int Binary = 2;
        public const int Close = 8;
        public const int Ping = 9;
        public const int Pong = 10;
    }
    /// <summary>
    /// 2字节数据头
    /// </summary>
    public class DataFrameHeader
    {
        private bool _fin;
        private bool _rsv1;
        private bool _rsv2;
        private bool _rsv3;
        private sbyte _opcode;
        private bool _maskcode;
        private sbyte _payloadlength;

        /// <summary>
        /// FIN
        /// </summary>
        public bool FIN { get { return _fin; } }

        /// <summary>
        /// RSV1
        /// </summary>
        public bool RSV1 { get { return _rsv1; } }

        /// <summary>
        /// RSV2
        /// </summary>
        public bool RSV2 { get { return _rsv2; } }

        /// <summary>
        /// RSV3
        /// </summary>
        public bool RSV3 { get { return _rsv3; } }

        /// <summary>
        /// OpCode
        /// </summary>
        public sbyte OpCode { get { return _opcode; } }

        /// <summary>
        /// 是否有掩码
        /// </summary>
        public bool HasMask { get { return _maskcode; } }

        /// <summary>
        /// Payload Length
        /// </summary>
        public sbyte Length { get { return _payloadlength; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <remarks>主要用于解析接收数据</remarks>
        public DataFrameHeader(byte[] buffer)
        {
            if (buffer.Length < 2)
                throw new Exception("无效的数据头.");
            //第一个字节
            _fin = (buffer[0] & 0x80) == 0x80;
            _rsv1 = (buffer[0] & 0x40) == 0x40;
            _rsv2 = (buffer[0] & 0x20) == 0x20;
            _rsv3 = (buffer[0] & 0x10) == 0x10;
            _opcode = (sbyte)(buffer[0] & 0x0f);
            //第二个字节
            _maskcode = (buffer[1] & 0x80) == 0x80;
            _payloadlength = (sbyte)(buffer[1] & 0x7f);

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <remarks>主要用于发送封装数据</remarks>
        public DataFrameHeader(bool fin, bool rsv1, bool rsv2, bool rsv3, sbyte opcode, bool hasmask, int length)
        {
            _fin = fin;
            _rsv1 = rsv1;
            _rsv2 = rsv2;
            _rsv3 = rsv3;
            _opcode = opcode;
            //第二个字节
            _maskcode = hasmask;
            _payloadlength = (sbyte)length;
        }

        /// <summary>
        /// 返回帧头字节
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[2] { 0, 0 };

            if (_fin) buffer[0] ^= 0x80;
            if (_rsv1) buffer[0] ^= 0x40;
            if (_rsv2) buffer[0] ^= 0x20;
            if (_rsv3) buffer[0] ^= 0x10;
            buffer[0] ^= (byte)_opcode;

            if (_maskcode) buffer[1] ^= 0x80;
            buffer[1] ^= (byte)_payloadlength;

            return buffer;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PXLib.ObjectManage
{
    //value = System.Net.IPAddress.HostToNetworkOrder(value);
    public class BufferManager
    {
        //最好编码也放成变量 默认UTF-8
        public Encoding EncodingConverter { private get; set; }
        private const int bufferSize = 2 * 1024;//初始化空间默认2K
        private byte[] _buffer;
        private int _offset;
        private int _dataCount;
        public BufferManager(int maxBufferSize)
        {
            this._buffer = new byte[maxBufferSize];//初始化空间
            this._offset = 0;
            this._dataCount = 0;
            EncodingConverter = Encoding.UTF8;
        }
        public BufferManager()
        {
            this._buffer = new byte[bufferSize];
            this._offset = 0;
            this._dataCount = 0;
            EncodingConverter = Encoding.UTF8;
        }
        /// <summary>
        /// 获取拷贝的缓冲区
        /// </summary>
        /// <returns></returns>
        public byte[] GetCopyBuffer()
        {
            byte[] lastBuffer = new byte[_dataCount];
            if (_dataCount > 0)
            {
                Array.Copy(this._buffer, 0, lastBuffer, 0, _dataCount);
            }
            return lastBuffer;
        }
       /// <summary>
       /// 获取有效的缓冲区
       /// </summary>
       /// <returns></returns>
        public byte[] Buffer
        {
            get { return this._buffer; }
        }
        public void SetBuffer(byte[] in_buffer)
        {       
            this._offset = 0;
            //this._buffer = in_buffer;
            //this._dataCount = in_buffer.Length;
            WriteBuffer(in_buffer);
        }
        public void SetOffset(int offset)
        {
            this._offset = offset <= 0 ? 0 : offset;
        }
        /// <summary>
        /// 获取缓冲区有效长度
        /// </summary>
        public int Length
        {
            get { return this._dataCount; }
        }
        public void Reset(bool clearBuffer=false)
        {
            this._offset = 0;
            this._dataCount = 0;
            if (clearBuffer)
                this._buffer = new byte[bufferSize];//重新分配空间
        }
        public void Clear(int count)
        {
            if (count >= _dataCount) //如果需要清理的数据大于现有数据大小，则全部清理
            {
                _dataCount = 0;
            }
            else
            {
                for (int i = 0; i < _dataCount - count; i++) //否则后面的数据往前移
                {
                    _buffer[i] = _buffer[count + i];//从第count处开始替换数据给第i个
                }
                _dataCount -= count;
            }
        }
        #region 写入操作
        private int GetReserveCount()//Buffer剩余的空间数(还没有写入数据的空间数)
        {
            return this._buffer.Length - this._dataCount;
        }
        public void WriteBuffer(byte[] buffer, int offset, int count)
        {
            if (GetReserveCount() >= count) //缓冲区空间够使用
            {
                Array.Copy(buffer, offset, _buffer, _dataCount, count);//直接从Buffer的DataCount处开始写入拷贝的buffer
                _dataCount += count;//DataCount增加count个数
            }
            else //缓冲区空间不够，需要申请更大的内存，并重写Buffer
            {
                int totalSize = _buffer.Length + count - GetReserveCount(); //最终数据总长度=Buffer原总大小+本次需要的count大小-空余大小
                byte[] tmpBuffer = new byte[totalSize];//开辟最终数据最终数据大小的空间
                Array.Copy(this._buffer, 0, tmpBuffer, 0, _dataCount); //复制以前的数据到tmpBuffer
                Array.Copy(buffer, offset, tmpBuffer, _dataCount, count); //从tmpBuffer的DataCount处写入新复制新的buffer数据
                _dataCount =_dataCount+ count;//重置大小
                this._buffer = tmpBuffer; //Buffer最终重置为tmpBuffer
            }
        }
        public void WriteBuffer(byte[] buffer)
        {
            if (buffer == null)
                return;
            WriteBuffer(buffer, 0, buffer.Length);
        }
        public void WriteLenBytes(byte[] blob)
        {
            UInt32 size = (UInt32)blob.Length;
            this.WriteUInt32(size);
            WriteBuffer(blob);
        }
        /// <summary>
        /// WriteInt8/WriteByte 写入一个字节 最大255
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt8(Byte value)
        {
            byte[] bytes = new byte[] { value };
            WriteBuffer(bytes);
        }
        /// <summary>
        /// 一个字节 8位 最大127
        /// </summary>
        public void WriteSByte(SByte value)
        {
            byte[] bytes = new byte[] { (Byte)value };
            WriteBuffer(bytes);
        }
        public void WriteInt16(short value) //Int16   大多数情况下短型需要强转
        {
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        public void WriteUInt16(ushort value)
        {
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        public void WriteBool(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            WriteBuffer(bytes);
        }
        public void WriteInt32(int value)
        {
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(tmpBuffer);
            //}
            WriteBuffer(tmpBuffer);
        }
        public void WriteUInt32(uint value)
        {
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        public void WriteLong(long value)
        {
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        public void WriteFloat(float value)
        {
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        public void WriteString(string value)
        {
            byte[] tmpBuffer = this.EncodingConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
        public void WriteLenString(string value)
        {
            byte[] tmpBuffer = EncodingConverter.GetBytes(value);
            WriteUInt32((UInt32)tmpBuffer.Length);
            WriteBuffer(tmpBuffer);
        }
        /// <summary>
        /// 文本全部转成UTF8，UTF8兼容性好 0标志写入结束
        /// </summary>
        public void WriteString0(string value)
        {
            if (value.IsNullEmpty())
                return;
            byte[] tmpBuffer = EncodingConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
            WriteSByte(0);//结束标识
        }
        #region 消息操作 未测试
        public void WriteMsgHead(ushort msgId, ushort msgLength) //写入消息 固定 
        {
            WriteUInt16(msgId);
            if (msgLength <= 0)//固定长度的
            {
                WriteUInt16(0);
            }
            else
            {
                //this.Buffer[2] = (Byte)(msg.MsgLength & 0xff);
                //this.Buffer[3] = (Byte)(msg.MsgLength >> 8 & 0xff);
                WriteUInt16(msgLength);
            }
        }
        public void WriteMsgLength()
        {
            this._buffer[2] = (Byte)(_dataCount & 0xff);
            this._buffer[3] = (Byte)(_dataCount >> 8 & 0xff);
        }
        #endregion
        #endregion

        #region 读取操作
        /// <summary>
        /// ReadInt8/ReadByte 读取一个字节 8位 最大255
        /// </summary>
        /// <returns></returns>
        public byte ReadInt8()
        {
            var value = _buffer[_offset];
            _offset = Interlocked.Increment(ref _offset);
            return value;
        }
        public sbyte ReadSByte()
        {
            var value = _buffer[_offset];
            _offset = Interlocked.Increment(ref _offset);
            return (sbyte)value;
        }
        public short ReadInt16() //Int16  
        {
            _offset = Interlocked.Add(ref _offset, 2);
            return BitConverter.ToInt16(_buffer, _offset - 2);//写法2
        }
        public ushort ReadUInt16() //UInt16  
        {
            _offset += 2;
            return BitConverter.ToUInt16(_buffer, _offset - 2);//写法1
        }

        public bool ReadBool()
        {
            var value = BitConverter.ToBoolean(_buffer, _offset);
            _offset = Interlocked.Increment(ref _offset);
            return value;
        }
        public int ReadInt32()
        {
            var value = BitConverter.ToInt32(_buffer, _offset);
            _offset = Interlocked.Add(ref _offset, 4);
            return value;
        }
        public uint ReadUInt32()
        {
            var value = BitConverter.ToUInt32(_buffer, _offset);
            _offset = Interlocked.Add(ref _offset, 4);
            return value;
        }

        public long ReadInt64()
        {
            var value = BitConverter.ToInt64(_buffer, _offset);
            _offset = Interlocked.Add(ref _offset, 8);
            return value;
        }
        public float ReadFloat()
        {
            var value = BitConverter.ToSingle(_buffer, _offset);
            _offset = Interlocked.Add(ref _offset, 4);
            return value;
        }
        public double ReadDouble()
        {
            var value = BitConverter.ToDouble(_buffer, _offset);
            _offset = Interlocked.Add(ref _offset, 8);
            return value;
        }
        public byte[] ReadToEnd()
        {
            int len = _dataCount - _offset;
            var value = new byte[len];
            System.Buffer.BlockCopy(_buffer, _offset, value, 0, len);//效率比Array.Copy高
            _offset = Interlocked.Add(ref _offset, len);
            return value;
        }
        /// <summary>
        /// 一次性读完包的字符串
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            int len = _dataCount - _offset;
            //string value = BitConverter.ToString(_buffer, _offset, len);//16进制标识的字符串
            string value = EncodingConverter.GetString(_buffer, _offset, len);
            _offset = Interlocked.Add(ref _offset, len);
            return value;
        }
        public string ReadLenString()
        {
            int offset = this._offset;
            int lenStr = ReadInt32();
            this._offset += lenStr;
            return EncodingConverter.GetString(_buffer, offset + 4, lenStr);
        }
        public string ReadString0()
        {
            int offset = this._offset;
            try
            {
                while (_buffer[_offset++] != 0)
                {

                }
            }
            catch {
                _offset = _buffer.Length;
            }
            return EncodingConverter.GetString(_buffer, offset, this._offset - offset - 1);
        }
        public string ReadString(int length)
        {
            int offset = _offset;
            this._offset += length;
            return EncodingConverter.GetString(_buffer, offset, length);
        }
        public byte[] ReadLenBytes()
        {
            Int32 size = ReadInt32();
            //if (size > 1024 * 1024 * 10)
            //    size = 1024 * 1024 * 10;
            byte[] tempBuffer = new byte[size];
            Array.Copy(_buffer, _offset, tempBuffer, 0, size);
            _offset += (int)size;
            return tempBuffer;
        }
        public byte[] ReadBuffer(int length)
        {
            byte[] tempBuffer = new byte[length];
            System.Buffer.BlockCopy(_buffer, _offset, tempBuffer, 0, length);
            _offset += (int)length;
            return tempBuffer;
        }

        #endregion

        //---------------------------------------------------------------------------------
        public string toString(byte[] buf)
        {
            string s = "";
            int ii = 0;


            for (int i = 0; i < buf.Length; i++)
            {
                ii += 1;
                if (ii >= 200)
                {
                    // MyDebug.Dbg.Log(s);
                    s = "";
                    ii = 0;
                }

                s += buf[i];
                s += " ";
            }

            // MyDebug.Dbg.Log(s);
            return s;
        }
        public string toString() {
            return toString(GetCopyBuffer());
        }
    }
}

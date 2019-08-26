using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PXLib.Helpers
{
    public class BytesHelper
    {
        /// <summary>
        /// CopyData 拷贝二进制数据
        /// </summary>      
        public static void CopyData(byte[] source, byte[] dest, int destOffset)
        {
            Buffer.BlockCopy(source, 0, dest, destOffset, source.Length);//效率比Array.Copy快
            //for (int i = 0; i < source.Length; i++)//相等
            //{
            //    dest[destOffset + i] = source[i];
            //}
        }
        /// <summary>
        /// 合并多个byte[]
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static byte[] MergeBytes(params byte[][] args)
        {
            Int32 length = 0;
            foreach (byte[] tempbyte in args)
            {
                length += (tempbyte != null ? tempbyte.Length : 0);  //计算数据包总长度
            }
            Byte[] bytes = new Byte[length]; //建立新的数据包
            Int32 tempLength = 0;
            foreach (byte[] tempByte in args)
            {
                if (tempByte == null) continue;
                tempByte.CopyTo(bytes, tempLength);
                tempLength += tempByte.Length;  //复制数据包到新数据包
            }
            return bytes;
        }
        /// <summary>
        /// 将字符串生成一个带头（长度）的字节数组
        /// </summary>
        public static byte[] AppendHeadBytes(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return MergeBytes(BitConverter.GetBytes(bytes.Length), bytes);
        }
        /// <summary>
        /// 将字节数组buffer生成一个带头（长度）的字节数组
        /// </summary>
        public static byte[] AppendHeadBytes(byte[] buffer)
        {
            return MergeBytes(BitConverter.GetBytes(buffer.Length), buffer);
        }
        public static byte[] GetBytesWithHead(byte[] buffer)
        {
            int length = BitConverter.ToInt32(buffer, 0);
            byte[] tempBuffer = new byte[length];
            Buffer.BlockCopy(buffer, buffer.Length - length, tempBuffer, 0, length);
            return tempBuffer;
        }
        /// <summary>
        /// 将一个数组中的连续部分复制到另一数组
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="startIndex">开始位置</param>
        /// <param name="length">长度</param>
        public static byte[] CopyArrayData(byte[] source, int startIndex, int length)
        {
            byte[] result = new byte[length];

            for (var i = 0; i < length; i++)
            {
                result[i] = source[startIndex + i];
            }

            return result;
        }
        public static byte[] GetAllBytes(Stream stream)
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                result = memoryStream.ToArray();
            }
            return result;
        }
    }
}

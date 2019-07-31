using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PXLib.Zip
{
    public class GZipHelper
    {
      
        /// <summary>
        /// 压缩字符串  这里是转成了Base64反而增大了33%
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }
            ms.Position = 0;
            byte[] compressed = ms.ToArray();
            ms.Read(compressed, 0, compressed.Length);
            byte[] gzBuffer = new byte[compressed.Length + 4];    // prepare final data with header that indicates length
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length); //copy compressed data 4 bytes from start of final header
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);// copy header to first 4 bytes
            ms.Close();
            return Convert.ToBase64String(gzBuffer);
        }

        /// <summary>
        /// 解压字符串
        /// </summary>
        /// <param name="compressedText"></param>
        /// <returns></returns>
        public static string UncompressString(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            MemoryStream ms = new MemoryStream();
            int msgLength = BitConverter.ToInt32(gzBuffer, 0);
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4);
            byte[] buffer = new byte[msgLength];
            ms.Position = 0;
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress)) // unzip the data through stream
            {
                zip.Read(buffer, 0, buffer.Length);
            }
            ms.Close();
            return Encoding.UTF8.GetString(buffer);
        }
        /// <summary>
        /// 压缩字节
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] bytData)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(bytData);
            var dest = new MemoryStream();
            GZipStream zipStream = new GZipStream(dest, CompressionMode.Compress, true);
            byte[] buf = new byte[4096];
            int len;
            ms.Seek(0, SeekOrigin.Begin);
            while ((len = ms.Read(buf, 0, buf.Length)) > 0)
            {
                zipStream.Write(buf, 0, len);
            }
            zipStream.Close();
            bw.Close();
            ms.Close();
            byte[] cb = dest.ToArray();
            return cb;
        }

        /// <summary>
        /// 解压字节
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] bytData)
        {
            var ms = new MemoryStream();
            ms.Write(bytData, 0, bytData.Length);
            ms.Seek(0, SeekOrigin.Begin);
            var dest2 = new System.IO.MemoryStream();
            GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress, true);
            byte[] buf2 = new byte[4096];
            int len2;
            while ((len2 = zipStream.Read(buf2, 0, buf2.Length)) > 0)
            {
                dest2.Write(buf2, 0, len2);
            }
            zipStream.Close();
            dest2.Seek(0, SeekOrigin.Begin);
            return dest2.ToArray();
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="destinationFile">目标文件</param>
        public static void CompressFile(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException();
            }
            byte[] buffer = null;
            FileStream stream = null;
            FileStream stream2 = null;
            try
            {
                stream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                buffer = new byte[stream.Length];
                if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    throw new ApplicationException();
                }
                stream2 = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write);
                new GZipStream(stream2, CompressionMode.Compress, true).Write(buffer, 0, buffer.Length);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (stream2 != null)
                {
                    stream2.Close();
                }
            }
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="destinationFile">目标文件</param>
        public static void DecompressFile(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException();
            }
            FileStream stream = null;
            FileStream stream2 = null;
            GZipStream stream3 = null;
            byte[] buffer = null;
            try
            {
                stream = new FileStream(sourceFile, FileMode.Open);
                stream3 = new GZipStream(stream, CompressionMode.Decompress, true);
                buffer = new byte[4];
                int num = ((int)stream.Length) - 4;
                stream.Position = num;
                stream.Read(buffer, 0, 4);
                stream.Position = 0L;
                byte[] buffer2 = new byte[BitConverter.ToInt32(buffer, 0) + 100];
                int offset = 0;
                int count = 0;
                while (true)
                {
                    int num5 = stream3.Read(buffer2, offset, 100);
                    if (num5 == 0)
                    {
                        break;
                    }
                    offset += num5;
                    count += num5;
                }
                stream2 = new FileStream(destinationFile, FileMode.Create);
                stream2.Write(buffer2, 0, count);
                stream2.Flush();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (stream3 != null)
                {
                    stream3.Close();
                }
                if (stream2 != null)
                {
                    stream2.Close();
                }
            }
        }
        #region 基于 ICSharpCode.SharpZipLib 源码已集成到本地
        /// <summary>
        /// 使用BZip2的Compress方法压缩二进制，如果是数据源是Stream流直接使用BZip类
        /// </summary>
        public static byte[] BZip2Compress(byte[] cdata)
        {
            return BZip2Compress(cdata, 0, cdata.Length);
        }
        public static byte[] BZip2Compress(byte[] data, int offset, int size)
        {
            MemoryStream inStream = new MemoryStream(data, offset, size);
            MemoryStream outStream = new MemoryStream();
            BZip2.BZip2.Compress(inStream, outStream, size);
            byte[] result = outStream.ToArray();
            inStream.Close();
            outStream.Close();
            return result;
        }
        /// <summary>
        /// 使用BZip2的Decompress方法解缩二进制，如果是数据源是Stream流直接使用BZip类
        /// </summary>
        public static byte[] BZip2Decompress(byte[] ddata) 
        {
            return BZip2Decompress(ddata, 0, ddata.Length);
        }
        public static byte[] BZip2Decompress(byte[] data, int offset, int size)
        {
            MemoryStream inStream = new MemoryStream(data, offset, size);
            MemoryStream outStream = new MemoryStream();
            BZip2.BZip2.Decompress(inStream, outStream);
            byte[] result = outStream.ToArray();
            inStream.Close();
            outStream.Close();
            return result;
        }
       
        #endregion
    }
}

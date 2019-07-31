using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Xml.Serialization;

namespace PXLib.Helpers
{
    /// <summary>
    /// 序列化和反序列化操作  一般类序列化需要标记[Serializable] 201706
    /// </summary>
    public class SerializeHelper
    {
        #region BinaryFormatter
        /// <summary>
        /// Obj（要标记能序列化的对象）序列化成byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] BinarySerializeObject(object obj) //obj 可以是数组
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();//此种情况下,mem_stream的缓冲区大小是可变的

            formatter.Serialize(memoryStream, obj);

            byte[] buff = memoryStream.ToArray();
            memoryStream.Close();

            return buff;
        }
        /// <summary>
        /// Obj序列化成byte[]
        /// </summary>
        public static void BinarySerializeObject(object obj, ref byte[] buff, int offset) //obj 可以是数组
        {
            byte[] rude_buff = SerializeHelper.BinarySerializeObject(obj);
            for (int i = 0; i < rude_buff.Length; i++)
            {
                buff[offset + i] = rude_buff[i];
            }
        }


        /// <summary>
        /// byte[]序列化成obj
        /// </summary>
        public static object BinaryDeserializeBytes(byte[] buff, int index, int count)//index：0，count：buff.Length
        {
            if (buff.Length <= 0)
            {
                throw new Exception("二进制数据不能为空");
            }
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(buff, index, count);
            object obj = formatter.Deserialize(stream);
            stream.Close();

            return obj;
        }
        /// <summary>
        /// byte[]序列化成obj
        /// </summary>
        public static object BinaryDeserializeBytes(byte[] bytes)
        {
            if (bytes == null) return null;
            MemoryStream stream = new MemoryStream(bytes);
            var result = new BinaryFormatter().Deserialize(stream);
            return result;
        }

        #region BinaryFormatter序列化反序列化
        /// <summary>
        /// BinaryFormatter序列化
        /// </summary>
        /// <param name="item">对象</param>
        public static string ModelToStringBinary<T>(T item)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, item);
                ms.Position = 0;
                byte[] bytes = ms.ToArray();
                StringBuilder sb = new StringBuilder();
                foreach (byte bt in bytes)
                {
                    sb.Append(string.Format("{0:X2}", bt));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// BinaryFormatter反序列化
        /// </summary>
        /// <param name="str">字符串序列</param>
        public static T StringToModelBinary<T>(string str)
        {
            int intLen = str.Length / 2;
            byte[] bytes = new byte[intLen];
            for (int i = 0; i < intLen; i++)
            {
                int ibyte = Convert.ToInt32(str.Substring(i * 2, 2), 16);
                bytes[i] = (byte)ibyte;
            }
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(ms);
            }
        }
        #endregion

        #endregion
        #region SoapFormatter
        /// <summary>
        /// SOAP 将对象序列化为 XML 格式。
        /// 如果要将对象转化为简洁的xml格式，请使用ESBasic.Persistence.SimpleXmlConverter类。
        /// </summary>        
        public static string SOAPSerializeObjectToString(object obj)
        {
            IFormatter formatter = new SoapFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string res = reader.ReadToEnd();
            stream.Close();
            return res;
        }
        /// <summary>
        /// SOAP字符串反序列化成简单对象 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static object SOAPDeserializeStringToObject(string str)
        {
            byte[] buff = System.Text.Encoding.Default.GetBytes(str);
            IFormatter formatter = new SoapFormatter();
            MemoryStream stream = new MemoryStream(buff, 0, buff.Length);
            object obj = formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }
        #endregion
        #region XmlSerializer
        public static string XmlObject(object obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();
            xmlSerializer.Serialize(stream, obj);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string res = reader.ReadToEnd();
            stream.Close();
            return res;
        }
        public static T ObjectXml<T>(string str)
        {
            return (T)SerializeHelper.ObjectXml(str, typeof(T));
        }

        public static object ObjectXml(string str, Type targetType)
        {
            byte[] buff = System.Text.Encoding.Default.GetBytes(str);
            XmlSerializer xmlSerializer = new XmlSerializer(targetType);
            MemoryStream stream = new MemoryStream(buff, 0, buff.Length);
            object obj = xmlSerializer.Deserialize(stream);
            stream.Close();
            return obj;
        }
        #endregion
        #region SaveOrReadFile
        /// <summary>
        /// SaveToFile 将对象的二进制序列化后保存到文件。
        /// </summary>       
        public static void SaveToFile(object obj, string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Flush();
            stream.Close();
        }
        /// <summary>
        /// ReadFromFile 从文件读取二进制反序列化为对象。
        /// </summary> 
        public static object ReadFromFile(string filePath)
        {
            byte[] buff = FileHelper.ReadFileToBytes(filePath);
            return SerializeHelper.BinaryDeserializeBytes(buff, 0, buff.Length);
        }
        #endregion
    }
}

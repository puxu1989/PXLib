using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PXLib.Helpers
{
    /// <summary>
    /// 安全帮助类 编码都是UTF8 一些常用的加密解密算法 对称密钥，非对称密钥，Hash(MD5,SHA)等
    /// </summary>
    public class SecurityHelper
    {
        protected static LogHelper log = new LogHelper("Security安全帮助类");
        private const string defDESEncryptKey = "PXLib";//DES默认长度可变的密钥
        private const string defFilesEncryptKey = "0123456789";//文件默认长度可变的密钥
        #region 第1个web引用过来的DES加密版本  对称
        /// <summary>
        /// DES数据加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string DESEncrypt(string Text)
        {
            return DESEncrypt(Text, defDESEncryptKey);
        }
        /// <summary> 
        /// DES数据加密 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string DESEncrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            //DES算法具有极高安全性，到目前为止，除了用穷举搜索法对DES算法进行攻击外，还没有发现更有效的办法。而56位长的密钥的穷举空间为256，这意味着如果一台计算机的速度是每一秒钟检测一百万个密钥，则它搜索完全部密钥就需要将近2285年的时间，可见，这是难以实现的。然而，这并不等于说DES是不可破解的。而实际上，随着硬件技术和Intemet的发展，其破解的可能性越来越大，而且，所需要的时间越来越少。
            byte[] inputByteArray;
            inputByteArray = Encoding.UTF8.GetBytes(Text);
            des.Key = ASCIIEncoding.UTF8.GetBytes(MD5String(sKey).Substring(0, 8));//B-S 请使用下列注释SecurityHelper.MD5String(sKey)System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5")//web中获取md5的另一种方法
            des.IV = ASCIIEncoding.UTF8.GetBytes(MD5String(sKey).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            cs.Close(); des.Dispose();
            return ret.ToString();
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string DESDecrypt(string Text)
        {
            return DESDecrypt(Text, defDESEncryptKey);
        }
        /// <summary> 
        /// DES解密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string DESDecrypt(string Text, string sKey)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                int len;
                len = Text.Length / 2;
                byte[] inputByteArray = new byte[len];
                int x, i;
                for (x = 0; x < len; x++)
                {
                    i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                    inputByteArray[x] = (byte)i;
                }
                des.Key = ASCIIEncoding.UTF8.GetBytes(MD5String(sKey).Substring(0, 8));
                des.IV = ASCIIEncoding.UTF8.GetBytes(MD5String(sKey).Substring(0, 8));
                using (MemoryStream ms = new MemoryStream())
                {
                    CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }

            }
        }
        #endregion

        #region 第2个网上找的3DES加密  对称
        //（3DES（即Triple DES）是DES向AES过渡的加密算法，它使用3条56位的密钥对数据进行三次加密。是DES的一个更安全的变形。它以DES为基本模块，通过组合分组方法设计出分组加密算法。比起最初的DES，3DES更为安全。）
        //构造一个对称算法
        private SymmetricAlgorithm mCSP = new TripleDESCryptoServiceProvider();
        private const string sIV3DESKey = "shuibiaoxie=";//12位长度的向量 测试了下好像可以随便定义最后等号不能变 不影响结果 ？
        /// <summary>
        /// 字符串的加密  密钥必须32位
        /// </summary>
        /// <param name="Value">要加密的字符串</param>
        /// <param name="sKey">密钥，必须32位</param>
        /// <returns>加密后的字符串</returns>
        public string Encrypt3DESString(string Value, string sKey)
        {
            try
            {
                ICryptoTransform ct;
                MemoryStream ms;
                CryptoStream cs;
                byte[] byt;
                mCSP.Key = Convert.FromBase64String(sKey);
                mCSP.IV = Convert.FromBase64String(sIV3DESKey);
                //指定加密的运算模式
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;
                //获取或设置加密算法的填充模式
                mCSP.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ct = mCSP.CreateEncryptor(mCSP.Key, mCSP.IV);//创建加密对象
                byt = Encoding.UTF8.GetBytes(Value);
                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                log.WriteLog(ex);
                return ("Error in Encrypting " + ex.Message);
            }
        }
        /// <summary>
        /// 解密字符串 密钥必须32位
        /// </summary>
        /// <param name="Value">加密后的字符串</param>
        /// <param name="sKey">密钥，必须32位</param>
        /// <returns>解密后的字符串</returns>
        public string Decrypt3DESString(string Value, string sKey)
        {
            try
            {
                ICryptoTransform ct;//加密转换运算
                MemoryStream ms;//内存流
                CryptoStream cs;//数据流连接到数据加密转换的流
                byte[] byt;
                //将3DES的密钥转换成byte
                mCSP.Key = Convert.FromBase64String(sKey);
                //将3DES的向量转换成byte
                mCSP.IV = Convert.FromBase64String(sIV3DESKey);
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;
                mCSP.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ct = mCSP.CreateDecryptor(mCSP.Key, mCSP.IV);//创建对称解密对象
                byt = Convert.FromBase64String(Value);
                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                log.WriteLog(ex);
                return ("Error in Decrypting " + ex.Message);
            }
        }
        #endregion
        #region AES 对称 21世纪标准 取代DES
        /// <summary>
        /// Aes加解密钥必须32位
        /// </summary>
        private static string AesDefaultKey = "PXLibAESKey";
        /// <summary>
        /// 获取Aes32位密钥
        /// </summary>
        /// <param name="key">Aes密钥字符串</param>
        /// <returns>Aes32位密钥</returns>
        static byte[] GetAesKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "Aes密钥不能为空");
            }
            if (key.Length < 32)
            {
                // 不足32补全
                key = key.PadRight(32, '0');
            }
            if (key.Length > 32)
            {
                key = key.Substring(0, 32);
            }
            return Encoding.UTF8.GetBytes(key);
        }
        public static string AESEncryptString(string content) 
        {
            return AESEncryptString(content, AesDefaultKey);
        }
        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="key">aes密钥，长度必须32位</param>
        /// <returns>加密后的字符串</returns>
        public static string AESEncryptString(string source, string key)
        {
            using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
            {
                aesProvider.Key = GetAesKey(key);
                aesProvider.Mode = CipherMode.ECB;
                aesProvider.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor())
                {
                    byte[] inputBuffers = Encoding.UTF8.GetBytes(source);
                    byte[] results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);
                    cryptoTransform.Dispose();
                    return Convert.ToBase64String(results, 0, results.Length);
                }
            }
        }
        public static string AESDecryptString(string content) 
        {
           return AESDecryptString(content, AesDefaultKey);
        }
        /// <summary>
        /// Aes解密
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="key">aes密钥，长度必须32位</param>
        /// <returns>解密后的字符串</returns>
        public static string AESDecryptString(string source, string key)
        {
            using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
            {
                aesProvider.Key = GetAesKey(key);
                aesProvider.Mode = CipherMode.ECB;
                aesProvider.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor())
                {
                    byte[] inputBuffers = Convert.FromBase64String(source);
                    byte[] results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);
                    cryptoTransform.Dispose();
                    return Encoding.UTF8.GetString(results);
                }
            }
        }
        /// <summary>
        /// AESDescrpt加密 AES-128-CBC，填充模式PKCS#7。
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public static string AESDecryptString(string encryptedData, string iv, string sessionKey)
        {
            //创建解密器生成工具实例  
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                //设置解密器参数  
                aes.Mode = CipherMode.CBC;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                //格式化待处理字符串  
                byte[] byte_encryptedData = Convert.FromBase64String(encryptedData);
                byte[] byte_iv = Convert.FromBase64String(iv);
                byte[] byte_sessionKey = Convert.FromBase64String(sessionKey);
                aes.IV = byte_iv;
                aes.Key = byte_sessionKey;
                //根据设置好的数据生成解密器实例  
                using (ICryptoTransform transform = aes.CreateDecryptor())
                {
                    //解密  
                    byte[] final = transform.TransformFinalBlock(byte_encryptedData, 0, byte_encryptedData.Length);

                    //生成结果  
                    string result = Encoding.UTF8.GetString(final);
                    return result;
                }
            }

        }
        #endregion
        #region RSA加密和解密  非对称
        /// <summary>
        /// 生成私钥和公钥 arr[0]私钥 arr[1]公钥
        /// </summary>
        /// <returns></returns>
        public static string[] RSACreateKeys()
        {
            string[] sKeys = new String[2];
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            sKeys[0] = rsa.ToXmlString(true);
            sKeys[1] = rsa.ToXmlString(false);
            return sKeys;
        }

        /// <summary>
        /// RSA加密 返回16进制表示的字符串 说明一点要加密的字符长度有限 
        /// </summary>
        /// <param name="sSource" >Source string</param>
        /// <param name="sPublicKey" >public key</param>
        /// <returns></returns>
        public static string RSAEncryptString(string encryptString, string xmlPublicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPublicKey);
            byte[] cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(encryptString), false);
            StringBuilder sb = new StringBuilder();
            foreach (byte bt in cipherbytes)
            {
                sb.Append(string.Format("{0:x2}", bt));
            }
            return sb.ToString();
        }
        /// <summary>
        /// RSA 解密  对应16进制标示的字符串
        /// </summary>
        /// <param name="decryptString">Source string</param>
        /// <param name="sPrivateKey">Private Key</param>
        /// <returns></returns>
        public static string RSADecryptString(string decryptString, string sPrivateKey)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(sPrivateKey);
                byte[] byteEn = new byte[128];//只能是128吗？
                for (int i = 0; i < decryptString.Length / 2; i++)
                {
                    byteEn[i] = byte.Parse(decryptString.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber);
                }
                byte[] plaintbytes = rsa.Decrypt(byteEn, false);
                return Encoding.UTF8.GetString(plaintbytes);
            }
            catch (Exception ex)
            {
                log.WriteLog(ex);
                return ex.Message;
            }
        }
        /// <summary>
        /// RSA的加密函数 返回Base64编码
        /// </summary>
        /// <param name="xmlPublicKey"></param>
        /// <param name="m_strEncryptString"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string xmlPublicKey, string m_strEncryptString)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPublicKey);
            byte[] CypherTextBArray = rsa.Encrypt(Encoding.UTF8.GetBytes(m_strEncryptString), false);
            return Convert.ToBase64String(CypherTextBArray);
        }
        /// <summary>
        /// RSA的解密函数 对应解密Base64编码的字符串
        /// </summary>
        /// <param name="xmlPrivateKey"></param>
        /// <param name="m_strDecryptString"></param>
        /// <returns></returns>
        public static string RSADecrypt(string xmlPrivateKey, string m_strDecryptString)
        {
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPrivateKey);
            byte[] PlainTextBArray = Convert.FromBase64String(m_strDecryptString);
            byte[] DypherTextBArray = rsa.Decrypt(PlainTextBArray, false);
            return Encoding.UTF8.GetString(DypherTextBArray);
        }
        #endregion
        #region 哈希算法 含有MD5 SHA1 SHA512(SHA-2)


        #region  MD5算法
        /// <summary>
        /// MD5加密算法 返回大写加密字符串 生成128位大整数 转换成16进制后是32位字符串
        /// </summary>
        public static string MD5String(string pwd)//默认32位字符串
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] bs = System.Text.Encoding.UTF8.GetBytes(pwd);
                bs = md5.ComputeHash(bs);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("X2"));//X2就是大写了
                }
                return sb.ToString();
            }
        }
        public static string MD5String16(string pwd) //麻痹原来16位的MD5中间的16位是相同的
        {
            return MD5String(pwd).Substring(8, 16);
        }

        /// <summary>
        /// 得到文件的md5值
        /// </summary>
        /// <param name="filePathName">传入的文件路径</param>
        /// <returns></returns>
        public static string MD5FileString(string filePathName)
        {
            MD5CryptoServiceProvider md = new MD5CryptoServiceProvider();
            byte[] bt;
            using (FileStream fs = new FileStream(filePathName, FileMode.Open, FileAccess.Read))
            {
                bt = md.ComputeHash(fs);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bt.Length; i++)
            {
                sb.Append(bt[i].ToString("X2"));
            }
            md.Dispose();
            return sb.ToString();
        }
        #endregion
        #region SHA1加密算法
        /// <summary>  
        /// SHA1 加密，返回大写字符串   速度慢 更安全
        /// </summary>  
        /// <param name="content">需要加密字符串</param>  
        /// <returns>返回40位UTF8 大写</returns>  
        public static string SHA1(string content)
        {
            return SHA1(content, Encoding.UTF8);
        }
        /// <summary>  
        /// SHA1 加密，返回大写字符串  
        /// </summary>  
        /// <param name="content">需要加密字符串</param>  
        /// <param name="encode">指定加密编码</param>  
        /// <returns>返回40位大写字符串</returns>  
        public static string SHA1(string content, Encoding encode)
        {
            try
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] bytes_in = encode.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                sha1.Dispose();
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("SHA1加密出错：" + ex.Message);
            }
        }
        #endregion
        #region SHA-2加密      //Net有SHA-256、SHA-384，和SHA-512并 都称为SHA-2
        /// <summary>
        ///  SHA256加密 哈希加密一个字符串 生成64位字符串
        /// </summary>
        /// <param name="Security"></param>
        /// <returns></returns>
        public static string SHA256String(string encodingString)
        {
            byte[] msg = Encoding.UTF8.GetBytes(encodingString);
            SHA256Managed Arithmetic = new SHA256Managed();
            byte[] Value = Arithmetic.ComputeHash(msg);
            StringBuilder sb = new StringBuilder();
            foreach (byte o in Value)
            {
                sb.Append(o.ToString("X2"));
            }
            Arithmetic.Dispose();
            return sb.ToString();
        }
        /// <summary>
        ///  SHA512加密 哈希加密一个字符串 生成128位字符串
        /// </summary>
        /// <param name="Security"></param>
        /// <returns></returns>
        public static string SHA512String(string encodingString)
        {
            byte[] msg = Encoding.UTF8.GetBytes(encodingString);
            SHA512Managed Arithmetic = new SHA512Managed();
            //.Net有SHA-256、SHA-384，和SHA-512并 都称为SHA-2
            byte[] Value = Arithmetic.ComputeHash(msg);
            StringBuilder sb = new StringBuilder();
            foreach (byte o in Value)
            {
                sb.Append(o.ToString("X2"));
            }
            Arithmetic.Dispose();
            return sb.ToString();
        }
        public static string HmacSHA256(string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                var sb = new StringBuilder();
                foreach (var i in hashmessage) sb.Append(i.ToString("X2"));
                return sb.ToString();
            }
        }
        /// <summary>
        ///  SHA512加密 哈希加密一个字符串 生成128位字符串
        /// </summary>
        /// <param name="Security"></param>
        /// <returns></returns>
        public static string SHA512FileString(string filePathName)
        {
            SHA512Managed sha512 = new SHA512Managed();
            byte[] bt;
            using (FileStream fs = new FileStream(filePathName, FileMode.Open, FileAccess.Read))
            {
                bt = sha512.ComputeHash(fs);
            }
            StringBuilder sb = new StringBuilder();
            //foreach (byte o in Value)
            //{
            //    Security += (int)o + "O";
            //}
            foreach (byte o in bt)
            {
                sb.Append(o.ToString("X2"));
            }
            sha512.Dispose();
            return sb.ToString();
        }
        #endregion
        #endregion
        #region Base64加密/解密
        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="text">要加密的字符串</param>
        /// <returns></returns>
        public static string Base64Encode(string text)
        {
            //如果字符串为空，则返回
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            try
            {
                char[] Base64Code = new char[]{'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T',
											'U','V','W','X','Y','Z','a','b','c','d','e','f','g','h','i','j','k','l','m','n',
											'o','p','q','r','s','t','u','v','w','x','y','z','0','1','2','3','4','5','6','7',
											'8','9','+','/','='};
                byte empty = (byte)0;
                ArrayList byteMessage = new ArrayList(Encoding.Default.GetBytes(text));
                StringBuilder outmessage;
                int messageLen = byteMessage.Count;
                int page = messageLen / 3;
                int use = 0;
                if ((use = messageLen % 3) > 0)
                {
                    for (int i = 0; i < 3 - use; i++)
                        byteMessage.Add(empty);
                    page++;
                }
                outmessage = new System.Text.StringBuilder(page * 4);
                for (int i = 0; i < page; i++)
                {
                    byte[] instr = new byte[3];
                    instr[0] = (byte)byteMessage[i * 3];
                    instr[1] = (byte)byteMessage[i * 3 + 1];
                    instr[2] = (byte)byteMessage[i * 3 + 2];
                    int[] outstr = new int[4];
                    outstr[0] = instr[0] >> 2;
                    outstr[1] = ((instr[0] & 0x03) << 4) ^ (instr[1] >> 4);
                    if (!instr[1].Equals(empty))
                        outstr[2] = ((instr[1] & 0x0f) << 2) ^ (instr[2] >> 6);
                    else
                        outstr[2] = 64;
                    if (!instr[2].Equals(empty))
                        outstr[3] = (instr[2] & 0x3f);
                    else
                        outstr[3] = 64;
                    outmessage.Append(Base64Code[outstr[0]]);
                    outmessage.Append(Base64Code[outstr[1]]);
                    outmessage.Append(Base64Code[outstr[2]]);
                    outmessage.Append(Base64Code[outstr[3]]);
                }
                return outmessage.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="text">要解密的字符串</param>
        public static string Base64Decode(string text)
        {
            //如果字符串为空，则返回
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            //将空格替换为加号
            text = text.Replace(" ", "+");

            try
            {
                if ((text.Length % 4) != 0)
                {
                    return "包含不正确的BASE64编码";
                }
                if (!Regex.IsMatch(text, "^[A-Z0-9/+=]*$", RegexOptions.IgnoreCase))
                {
                    return "包含不正确的BASE64编码";
                }
                string Base64Code = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
                int page = text.Length / 4;
                ArrayList outMessage = new ArrayList(page * 3);
                char[] message = text.ToCharArray();
                for (int i = 0; i < page; i++)
                {
                    byte[] instr = new byte[4];
                    instr[0] = (byte)Base64Code.IndexOf(message[i * 4]);
                    instr[1] = (byte)Base64Code.IndexOf(message[i * 4 + 1]);
                    instr[2] = (byte)Base64Code.IndexOf(message[i * 4 + 2]);
                    instr[3] = (byte)Base64Code.IndexOf(message[i * 4 + 3]);
                    byte[] outstr = new byte[3];
                    outstr[0] = (byte)((instr[0] << 2) ^ ((instr[1] & 0x30) >> 4));
                    if (instr[2] != 64)
                    {
                        outstr[1] = (byte)((instr[1] << 4) ^ ((instr[2] & 0x3c) >> 2));
                    }
                    else
                    {
                        outstr[2] = 0;
                    }
                    if (instr[3] != 64)
                    {
                        outstr[2] = (byte)((instr[2] << 6) ^ instr[3]);
                    }
                    else
                    {
                        outstr[2] = 0;
                    }
                    outMessage.Add(outstr[0]);
                    if (outstr[1] != 0)
                        outMessage.Add(outstr[1]);
                    if (outstr[2] != 0)
                        outMessage.Add(outstr[2]);
                }
                byte[] outbyte = (byte[])outMessage.ToArray(Type.GetType("System.Byte"));
                return Encoding.Default.GetString(outbyte);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region 加密文件/解密文件
        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="filePath">输入文件路径</param>
        /// <param name="savePath">加密后输出文件路径</param>
        /// <param name="keyStr">密码，可以为“”/NULL</param>
        /// <returns></returns>  
        public static bool FileEncrypt(string filePathName, string savePathName, string keyStr)
        {
            FileStream fs = null;
            CryptoStream cs = null;
            MemoryStream ms = null;
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                if (string.IsNullOrEmpty(keyStr))
                    keyStr = defFilesEncryptKey;
                fs = File.OpenRead(filePathName);
                byte[] inputByteArray = new byte[fs.Length];
                fs.Read(inputByteArray, 0, (int)fs.Length);
                fs.Close();
                byte[] keyByteArray = Encoding.Default.GetBytes(keyStr);
                SHA1 ha = new SHA1Managed();
                byte[] hb = ha.ComputeHash(keyByteArray);
                byte[] sKey = new byte[8];
                byte[] sIV = new byte[8];
                for (int i = 0; i < 8; i++)
                    sKey[i] = hb[i];
                for (int i = 8; i < 16; i++)
                    sIV[i - 8] = hb[i];
                des.Key = sKey;
                des.IV = sIV;
                ms = new MemoryStream();
                cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                fs = File.OpenWrite(savePathName);
                foreach (byte b in ms.ToArray())
                {
                    fs.WriteByte(b);
                }
                des.Dispose();
                ha.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                log.WriteLog(ex);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
                if (cs != null)
                    cs.Close();
                if (ms != null)
                    ms.Close();
            }
        }
        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="filePath">输入文件路径</param>
        /// <param name="savePath">解密后输出文件路径</param>
        /// <param name="keyStr">密码，可以为“”/NULL</param>
        /// <returns></returns>    
        public static bool FileDecrypt(string filePathName, string savePathName, string keyStr)
        {
            FileStream fs = null;
            MemoryStream ms = null;
            CryptoStream cs = null;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            if (string.IsNullOrEmpty(keyStr))
                keyStr = defFilesEncryptKey;
            try
            {
                fs = File.OpenRead(filePathName);
                byte[] inputByteArray = new byte[fs.Length];
                fs.Read(inputByteArray, 0, (int)fs.Length);
                fs.Close();
                byte[] keyByteArray = Encoding.Default.GetBytes(keyStr);
                SHA1 ha = new SHA1Managed();
                byte[] hb = ha.ComputeHash(keyByteArray);
                byte[] sKey = new byte[8];
                byte[] sIV = new byte[8];
                for (int i = 0; i < 8; i++)
                    sKey[i] = hb[i];
                for (int i = 8; i < 16; i++)
                    sIV[i - 8] = hb[i];
                des.Key = sKey;
                des.IV = sIV;
                ms = new MemoryStream();
                cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                fs = File.OpenWrite(savePathName);
                foreach (byte b in ms.ToArray())
                {
                    fs.WriteByte(b);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.WriteLog(ex);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
                if (cs != null)
                    cs.Close();
                if (ms != null)
                    ms.Close();
            }
        }

        #endregion
        #region 混淆与反混淆
        /// <summary>
        /// 用GUID简单混淆字符串 就是在字符串之间插入一些字符
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <returns>混淆后字符串</returns>
        public static string MixUp(string str)
        {
            var timestamp = Guid.NewGuid().ToString();
            var count = str.Length + 36;
            var sbd = new StringBuilder(count);
            int j = 0;
            int k = 0;
            for (int i = 0; i < count; i++)
            {
                if (j < 36 && k < str.Length)
                {
                    if (i % 2 == 0)
                    {
                        sbd.Append(str[k]);
                        k++;
                    }
                    else
                    {
                        sbd.Append(timestamp[j]);
                        j++;
                    }
                }
                else if (j >= 36)
                {
                    sbd.Append(str[k]);
                    k++;
                }
                else if (k >= str.Length)
                {
                    break;

                    // sbd.Append(timestamp[j]);
                    // j++;
                }
            }

            return sbd.ToString();
        }

        /// <summary>
        /// 简单反混淆
        /// </summary>
        /// <param name="str">混淆后字符串</param>
        /// <returns>原来字符串</returns>
        public static string MixClear(string str)
        {
            var sbd = new StringBuilder();
            int j = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (i % 2 == 0)
                {
                    sbd.Append(str[i]);
                }
                else
                {
                    j++;
                }

                if (j > 36)
                {
                    sbd.Append(str.Substring(i));
                    break;
                }
            }

            return sbd.ToString();
        }

        #endregion
    }
}

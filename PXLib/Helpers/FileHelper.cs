using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PXLib.Helpers
{
    #region 文件帮助类
    /// <summary>
    /// 文件帮助类 2016-9-23
    /// </summary>
    public class FileHelper
    {
        #region 获取文件副本名称全路径（加上重复副本的文件名）
        /// <summary>
        /// 检查获取要为复制的文件或者文件夹重命名的名称
        /// </summary>
        /// <param name="fileFullPathName">源文件全路径 如：D:\SVN\...\test.jpg</param>
        /// <param name="isDirectory">True是目录\False是文件</param>
        /// <returns>绝对路径新文件或者文件夹的名称</returns>
        public static string GetCopyPhysicalPathFullFileName(string fileFullPathName, bool isDirectory = false)
        {
            if (!File.Exists(fileFullPathName))
                return fileFullPathName;
            try
            {
                //要返回的对象名称
                string ReturnFileName = string.Empty;
                /*根据源文件名称复制出来的已经存在的副本文件名称的序号集合 
                 * eg. 源文件 ：Index.html 复制后会有  Index -副本.html、Index -副本 (2).html、Index -副本 (3).html...；
                 * 这里取序号 2、3填入集合
                */
                ArrayList FileName = new ArrayList();
                //同级目录下所有相同类型对象的名称
                string[] AllObjectName = isDirectory
                                      ? Directory.GetDirectories(Path.GetDirectoryName(fileFullPathName), "*", SearchOption.TopDirectoryOnly)   //目录
                                      : Directory.GetFiles(Path.GetDirectoryName(fileFullPathName), string.Format("*{0}", Path.GetExtension(fileFullPathName)), SearchOption.TopDirectoryOnly); //文件
                //复制源的对象名称
                string SourceFileName = string.Format("{0} - 副本", Path.GetFileNameWithoutExtension(fileFullPathName));//加空格和操作系统一样
                //同一目录中已经存在同种类型的对象的名称
                string ExistFileName = string.Empty;
                //遍历同种源复制出来的对角名称个数名，取其序号
                foreach (string var in AllObjectName)
                {
                    //已经存在的无后缀的文件名
                    ExistFileName = Path.GetFileNameWithoutExtension(var);
                    //判断是否存在源文件的副本文件，存在则把所有副本的序号填充到集合
                    if (ExistFileName.Contains(SourceFileName))
                    {
                        //抽取新建文件名中的序号
                        string tmpstr = ExistFileName.Replace(SourceFileName, string.Empty).ToString().Replace("(", string.Empty).Replace(")", string.Empty).Trim();
                        if (tmpstr.Equals(string.Empty))
                        {
                            FileName.Add(1);
                            continue;
                        }
                        int novalue;
                        //把序号转换成数值型
                        if (int.TryParse(tmpstr, out novalue))
                        {
                            FileName.Add(novalue);
                            continue;
                        }
                    }
                }
                //给集合按数字序号从小到大排序
                FileName.Sort();

                string usevalue = string.Empty;
                for (int i = 0; i < FileName.Count; i++)
                {
                    int objectNo = i + 1; //索引处对象名称序号应该对应的序号值

                    //查找数字序列中是否不完整，
                    if (!FileName[i].ToString().Equals(objectNo.ToString()))
                    { //如果不存在则选择这个值
                        usevalue = objectNo.Equals(1) ? string.Empty : objectNo.ToString();
                        break;
                    }
                    else
                    { //如果存在，则加1后，继续循环
                        usevalue = (objectNo + 1).ToString();
                        continue;
                    }
                }
                if (!usevalue.Equals(string.Empty)) //如果不为空给序号加上括号
                    usevalue = "(" + usevalue + ")";
                //组合文件名称
                ReturnFileName = Path.Combine(Path.GetDirectoryName(fileFullPathName), string.Format("{0} {1}", SourceFileName, usevalue)).Trim();
                if (!isDirectory) //如果不是目录，加上对象的后缀名
                    ReturnFileName += Path.GetExtension(fileFullPathName);
                //返回文件名称
                return ReturnFileName.Trim();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
        /// <summary>
        ///获取路径如 C:\Program Files\IIS Express
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// GetFileDirectory 获取文件所在的目录路径
        /// </summary>       
        public static string GetFileDirectory(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }
        //文件是否正在被使用
        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
            }
            catch
            {
                inUse = true;
            }

            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return inUse;
        }
        #region 获取带单位的文件大小
        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>
        public static string FileCountSize(long Size)
        {
            string m_strSize = "";
            long FactSize = 0;
            FactSize = Size;
            if (FactSize < 1024.00)
                m_strSize = FactSize.ToString("F2") + " Byte";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " KB";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " MB";
            else if (FactSize >= 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " GB";
            return m_strSize;
        }
        /// <summary>
        /// 获取文件的长度 如果文件不存在就返回0
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static long GetFileLength(string fileName)
        {
            if (File.Exists(fileName) == true)
            {
                FileInfo fileinfo = new FileInfo(fileName);
                long fl = fileinfo.Length;
                return fl;
            }
            else
                return 0;
        }
        /// <summary>
        /// 根据文件名，得到文件的大小，单位分别是GB/MB/KB
        /// </summary>
        /// <param name="FileFullPath">文件名</param>
        /// <returns>返回文件大小</returns>
        public static string GetFileSize(string fileName)
        {
            if (System.IO.File.Exists(fileName) == true)
            {
                FileInfo fileinfo = new FileInfo(fileName);
                long fl = fileinfo.Length;
                if (fl > 1024 * 1024 * 1024)
                {
                    return Convert.ToString(Math.Round((fl + 0.00) / (1024 * 1024 * 1024), 2)) + " GB";
                }
                else if (fl > 1024 * 1024)
                {
                    return Convert.ToString(Math.Round((fl + 0.00) / (1024 * 1024), 2)) + " MB";
                }
                else
                {
                    return Convert.ToString(Math.Round((fl + 0.00) / 1024, 2)) + " KB";
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取文件夹大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDictSize(string path)
        {
            if (!System.IO.Directory.Exists(path))
                return "0";
            string[] fs = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            //获取该文件夹中所有的文件名	
            long ll = 0;
            foreach (string f in fs)
            {
                dynamic fa = System.IO.File.GetAttributes(f);
                System.IO.FileInfo fi = new System.IO.FileInfo(f);
                ll += fi.Length;
            }
            return FileCountSize(ll);
        }
        #endregion

        #region 二进制写入文件
        /// <summary>
        /// WriteBuffToFile 将二进制数据写入文件中
        /// </summary>    
        public static void WriteBuffToFile(byte[] buff, int offset, int len, string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(buff, offset, len);
            bw.Flush();
            bw.Close();
            fs.Close();
        }
        /// <summary>
        /// WriteBuffToFile 将二进制数据写入文件中
        /// </summary>   
        public static void WriteBuffToFile(byte[] buff, string filePath)
        {
            WriteBuffToFile(buff, 0, buff.Length, filePath);
        }
        #endregion

        #region 将字符串写入文件/从文件读取字符串内容
        /// <summary>
        /// WriteTextToFile 将字符串写成文件
        /// </summary>       
        public static void WriteStringToFile(string filePath, string text)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            // FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(filePath, true);
            streamWriter.WriteLine(text);
            streamWriter.Flush();
            streamWriter.Close();
        }
        /// <summary>
        /// GetFileContent 读取文本文件的内容
        /// </summary>       
        public static string GetContentFile(string file_path)
        {
            string result;
            if (!File.Exists(file_path))
            {
                result = null;
            }
            else
            {
                StreamReader streamReader = new StreamReader(file_path, Encoding.UTF8);
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                result = text;
            }
            return result;
        }
        #endregion

        #region 清空并写入

        public static void ClearFileContentWriteIn(string filePath, string context)
        {
            //判断文件的存
            if (!File.Exists(filePath))
            {
                //没做完
            }
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
            //清空文件
            fs.SetLength(0);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            try
            {
                sw.WriteLine(context);
                sw.Close();
                fs.Close();
            }
            catch
            {
                sw.Close();
                fs.Close();
            }
        }

        #endregion

        #region 读取文件到二进制
        //根据文件路径读取到二进制
        public static byte[] ReadFileToBytes(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buff = new byte[fs.Length];
            fs.Read(buff, 0, buff.Length);
            //注释BinaryReader 二选一BinaryReader和FileStream的区别
            //BinaryReader可以指定 Encoding，从而实现读取字符串。
            //FileStream 可读可写，并且支持异步操作，还能封装非托管IO句柄，只支持文件流。
            //BinaryReader只能读，不支持异步操作，但支持所有继承至 Stream 的任何流，比如 NetworkStream，MemoryStream.
            //BinaryReader br = new BinaryReader(fs);
            //byte[] buff = br.ReadBytes((int)fs.Length);
            //br.Close();
            fs.Close();
            return buff;

        }
        #endregion

        #region 创建文件夹
        /// <summary>
        /// //如果不存在就创建文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        #endregion

        #region 删除文件 删除目录
        /// <summary>
        /// 删除文件
        /// </summary>
        public static void DeleteFile(string filefullPathName)
        {
            if (File.Exists(filefullPathName))
                File.Delete(filefullPathName);
        }
        /// <summary>
        /// 删除非空文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                DirectoryInfo[] childs = dir.GetDirectories();
                foreach (DirectoryInfo child in childs)
                {
                    child.Delete(true);
                }
                dir.Delete(true);
            }
        }
        #endregion

        #region  获得指定目录下的文件列表
        /// <summary>
        /// 获得指定目录下的文件列表
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static string[] GetDirectoryFileListName(string path, string searchPattern)
        {
            if (!Directory.Exists(path))
                return new string[0];
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] fileInfos = dirInfo.GetFiles(searchPattern);
            string[] result = new string[fileInfos.Length];
            for (int i = 0; i < fileInfos.Length; i++)
            {
                result[i] = fileInfos[i].Name;
            }
            return result;
        }
        /// <summary>
        /// 获取指定目录的所有文件列表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetDirectoryFileListName(string path)
        {
            return GetDirectoryFileListName(path, "*.*");
        }
        /// <summary>
        /// 获取自定义文件信息  searchPattern为null则获取该路径下的全部文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static List<FileDesc> GetDirectoryFileDesc(string path, string searchPattern)
        {
            if (!Directory.Exists(path))
                return null;
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*.*";
            }
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] fileInfos = dirInfo.GetFiles(searchPattern);
            List<FileDesc> list = new List<FileDesc>();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                FileDesc filedesc = new FileDesc();
                filedesc.FileName = fileInfos[i].Name;
                filedesc.FileLength = fileInfos[i].Length;
                filedesc.FileCreateDate = fileInfos[i].CreationTime;
                list.Add(filedesc);
            }
            return list;

        }

        #endregion

        #region 拷贝文件
        /// <summary>
        /// 拷贝指定文件夹下的所有文件（文件夹）到指定文件夹
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            try
            {
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                    File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));

                }

                if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                    destDirName = destDirName + Path.DirectorySeparatorChar;

                string[] files = Directory.GetFiles(sourceDirName);
                foreach (string file in files)
                {
                    if (File.Exists(destDirName + Path.GetFileName(file)))
                        continue;
                    File.Copy(file, destDirName + Path.GetFileName(file), true);
                    File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
                }

                string[] dirs = Directory.GetDirectories(sourceDirName);
                foreach (string dir in dirs)
                {
                    CopyDirectory(dir, destDirName + Path.GetFileName(dir));
                }
            }
            catch (Exception ex)
            {
                new LogHelper("FileHelper帮助类").WriteLog(ex);
            }
        }
        public static void CopyFile(string filePathName, string destDirName)
        {
            if (File.Exists(filePathName))
                File.Copy(filePathName, destDirName + "\\" + Path.GetFileName(filePathName), true);
        }
        public static void CopyAndRenameFile(string filePathName, string destDirName, string newPathName)
        {
            if (File.Exists(filePathName))
            {
                File.Copy(filePathName, destDirName + "\\" + Path.GetFileName(filePathName), true);
                File.Move(destDirName + "\\" + Path.GetFileName(filePathName), newPathName);
                File.Delete(destDirName + "\\" + Path.GetFileName(filePathName));
            }

        }

        #endregion

        #region 断点续传读取和写入 使用FileStream避免每次都打开
        /// <summary>
        /// 断点续传-根据第几个包和包的大小读取文件到二进制
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="packetIndex">读取第几个包</param>
        /// <param name="packetSize">每个包的大下</param>
        /// <returns></returns>
        public static byte[] ReadFileToByteWithPacketIndexAndSize(FileStream stream, int packetIndex, int packetSize)
        {
            byte[] result = null;
            long length = (long)packetSize * (packetIndex + 1);
            if (length - packetSize > stream.Length)
                return null;//多余最后一个包直接返回null
            if (length > stream.Length)
                result = new byte[stream.Length - packetIndex * packetSize];
            else
                result = new byte[packetSize];
            stream.Seek(packetIndex * packetSize, SeekOrigin.Begin);
            stream.Read(result, 0, result.Length);
            return result;
        }
        /// <summary>
        /// 断点续传-根据第几个包和包的的大小写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="packetIndex"></param>
        /// <param name="packetSize"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="receiveSize"></param>
        public static void FileWriteWithPacketIndexAndSize(FileStream stream, int packetIndex, int packetSize, byte[] data, int offset, int receiveSize)
        {
            stream.Seek((long)packetIndex * (long)packetSize, SeekOrigin.Begin);
            stream.Write(data, offset, receiveSize);
            stream.Flush();
        }
        #endregion

        #region 根据文件全路径获取文件名 获取文件夹
        /// <summary>
        /// 获取文件名称 
        /// </summary>
        /// <param name="filePathName"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutPath(string filePathName)
        {
            return Path.GetFileName(filePathName);
            //return filePathName.Substring(filePathName.LastIndexOf("\\") + 1, filePathName.Length - 1 - filePathName.LastIndexOf("\\"));
        }
        public static string GetDirPathWithoutFileName(string filePathName)
        {
            return Path.GetDirectoryName(filePathName);
        }
        #endregion
    }
    #endregion

    #region 文件操作扩展类
    public class BreakPointPost//断点续传类
    {
        public string FilePathName { get; set; }
        public long FileSize { get; set; }
        public int PackageSize { get; set; }
        public int PackageCount { get; set; }
        public int PackIndex { get; set; }

        public FileStream BreakPointPostStream { get; set; }

        public BreakPointPost(string filePathName, int packIndex, int packageSize)
        {
            FilePathName = filePathName;
            FileSize = new FileInfo(FilePathName).Length;
            PackageSize = packageSize;
            PackageCount = (int)Math.Ceiling((double)FileSize / PackageSize);
            PackIndex = packIndex;
            BreakPointPostStream = File.OpenRead(filePathName);
        }
    }
    /// <summary>
    /// 自定义文件描述类
    /// </summary>
    public class FileDesc
    {
        public string FileName { get; set; }
        public long FileLength { get; set; }
        public DateTime FileCreateDate { get; set; }
    }
    #endregion
}

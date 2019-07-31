using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace PXLib.Helpers
{
   public class FileDownHelper
    {
        /// <summary>
        /// 普通下载
        /// </summary>
        /// <param name="filePathName">文件虚拟路径</param>
        /// <param name="clientFileName">自定义返回带扩展名客户端名称</param>
        public static void DownLoadFile(string filePathName, string clientFileName="")
        {
            string destFileName = HttpContext.Current.Server.MapPath(filePathName);
            if (string.IsNullOrEmpty(clientFileName)) 
            {
                clientFileName = Path.GetFileName(destFileName);
            }
            if (File.Exists(destFileName))
            {
                FileInfo fi = new FileInfo(destFileName);
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.Buffer = false;
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(clientFileName, Encoding.UTF8));
                HttpContext.Current.Response.AppendHeader("Content-Length", fi.Length.ToString());
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.WriteFile(destFileName);
                HttpContext.Current.Response.Flush();
            }
            else
            {
                HttpContext.Current.Response.Write("文件不存在");
            }
            HttpContext.Current.Response.End();
        }
        /// <summary>
        /// 分块下载 默认200k
        /// </summary>
        /// <param name="FileName">文件虚拟路径</param>
        public static void DownLoadBlob(string fileName)
        {
            string filePath = HttpContext.Current.Server.MapPath(fileName);
            long chunkSize = 204800;             //指定块大小200k 
            byte[] buffer = new byte[chunkSize]; //建立一个200K的缓冲区 
            long dataToRead = 0;                 //已读的字节数   
            FileStream stream = null;
            try
            {
                //打开文件   
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                dataToRead = stream.Length;
                HttpContext.Current.Response.Clear();//先清除一次 如果是aspx页面response的信息会被写进文件
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachement;filename=" + HttpUtility.UrlEncode(Path.GetFileName(filePath)));
                HttpContext.Current.Response.AddHeader("Content-Length", dataToRead.ToString());

                while (dataToRead > 0)
                {
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        int length = stream.Read(buffer, 0, Convert.ToInt32(chunkSize));
                        HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);
                        HttpContext.Current.Response.Flush();
                        HttpContext.Current.Response.Clear();
                        dataToRead -= length;
                    }
                    else
                    {
                        dataToRead = -1; //防止client失去连接 
                    }
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("Error:" + ex.Message);
            }
            finally
            {
                if (stream != null) stream.Close();
                HttpContext.Current.Response.Close();
            }
        }
      
        //调用：
        // string FullPath=Server.MapPath("count.txt");
        // DownLoadFile(FullPath,500);
        //---------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fullPath"></param>
        /// <param name="_speed"></param>
        /// <returns></returns>
        public static bool DownLoadFile(string _fullPath, long _speed)
        {
            HttpRequest _Request = HttpContext.Current.Request;
            HttpResponse _Response = HttpContext.Current.Response;
            try
            {
                FileStream myFile = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(myFile);
                try
                {               
                    _Response.Clear();
                    _Response.AddHeader("Accept-Ranges", "bytes");
                    _Response.Buffer = false;

                    long fileLength = myFile.Length;
                    long startBytes = 0;
                    int pack = 1024*100;  //10K bytes
                    int sleep = (int)Math.Floor((double)( pack / _speed)) + 1;

                    if (_Request.Headers["Range"] != null)
                    {
                        _Response.StatusCode = 206;
                        string[] range = _Request.Headers["Range"].Split(new char[] { '=', '-' });
                        startBytes = Convert.ToInt64(range[1]);
                    }
                    _Response.AddHeader("Content-Length", (fileLength - startBytes).ToString());
                    if (startBytes != 0)
                    {
                        _Response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                    }

                    _Response.AddHeader("Connection", "Keep-Alive");
                    _Response.ContentType = "application/octet-stream";
                    _Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(_fullPath), System.Text.Encoding.UTF8));

                    br.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                    int maxCount = (int)Math.Floor((double)((fileLength - startBytes) / pack)) + 1;

                    for (int i = 0; i < maxCount; i++)
                    {
                        if (_Response.IsClientConnected)
                        {
                            _Response.BinaryWrite(br.ReadBytes(pack));
                            Thread.Sleep(sleep);
                        }
                        else
                        {
                            i = maxCount;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    br.Close();
                    myFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// DownLoadFileFromUrl 将url处的文件下载到本地
        /// </summary>       
        public static void DownLoadFileFromUrl(string url, string saveFilePath)
        {
            FileStream fstream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
            WebRequest wRequest = WebRequest.Create(url);

            try
            {
                WebResponse wResponse = wRequest.GetResponse();
                int contentLength = (int)wResponse.ContentLength;

                byte[] buffer = new byte[1024];
                int read_count = 0;
                int total_read_count = 0;
                bool complete = false;

                while (!complete)
                {
                    read_count = wResponse.GetResponseStream().Read(buffer, 0, buffer.Length);

                    if (read_count > 0)
                    {
                        fstream.Write(buffer, 0, read_count);
                        total_read_count += read_count;
                    }
                    else
                    {
                        complete = true;
                    }
                }

                fstream.Flush();
            }
            finally
            {
                fstream.Close();
                wRequest = null;
            }
        }
    }
}

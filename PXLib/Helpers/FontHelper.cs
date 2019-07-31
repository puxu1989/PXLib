using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PXLib.Helpers
{
    public class FontHelper
    {
        /// <summary>
        /// 通过字体文件获取字体  
        /// </summary>
        /// <param name="fontPath">路径和执行路径相同</param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static Font GetFontFromFile(string fontPath, float fontSize, FontStyle fontStyle)
        {
            try
            {
                //校验
                if (!File.Exists(fontPath) || fontSize <= 0)
                {
                    return null;
                }
                //获取字体对象
                PrivateFontCollection fontCollection = new PrivateFontCollection();
                fontCollection.AddFontFile(fontPath);
                var font = new Font(fontCollection.Families[0], fontSize, fontStyle);
                return font;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 通过资源流获取字体
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static Font GetFontFromStream(string fontName, float fontSize, FontStyle fontStyle)
        {
            try
            {
                //获取程序集
                //Assembly assembly = Assembly.GetExecutingAssembly();
                ////获取字体文件流
                //Stream stream = assembly.GetManifestResourceStream(fontName);//读不到
                ////读取字体到字节数组
                //byte[] fontData = new byte[stream.Length];
                //stream.Read(fontData, 0, (int)stream.Length);
                //stream.Close();
                byte[] fontData = FileHelper.ReadFileToBytes(fontName);
                //获取字体对象
                PrivateFontCollection pfc = new PrivateFontCollection();
                GCHandle hObject = GCHandle.Alloc(fontData, GCHandleType.Pinned);//不安全代码更改
                IntPtr pObject = hObject.AddrOfPinnedObject();
                pfc.AddMemoryFont(pObject, fontData.Length);
                return new Font(pfc.Families[0], fontSize, fontStyle);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //字体安装
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);
        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        [DllImport("gdi32")]
        private static extern int AddFontResource(string lpFileName);
        public static bool InstallFont(string sFontFileName)
        {
            string _sTargetFontPath = string.Format(@"{0}/Fonts/{1}", System.Environment.GetEnvironmentVariable("WINDIR"), sFontFileName);
            //系统FONT目录           
            string _sResourceFontPath = string.Format(@"{0}/Font/{1}", System.AppDomain.CurrentDomain.BaseDirectory, sFontFileName);
            //需要安装的FONT目录            
            try
            {
                if (!File.Exists(_sTargetFontPath) && File.Exists(_sResourceFontPath))
                {
                    int _nRet;
                    File.Copy(_sResourceFontPath, _sTargetFontPath);
                    _nRet = AddFontResource(_sTargetFontPath);
                    _nRet = WriteProfileString("Fonts", sFontFileName.Split('.')[0] + "(TrueType)", sFontFileName);
                }
            }
            catch { return false; } return true;
        }
    }
}

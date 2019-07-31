using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace PXLib.Helpers
{
    /// <summary>
    /// 生成验证码类
    /// </summary>
    public class ValidationCode
    {
        public static Bitmap CreateImageCode(int codeLength, out string validationCode)
        {
            validationCode = GenCode(codeLength);
            return GenImg(validationCode);
        }

        private static string GenCode(int num)
        {
            string[] array = new string[]
			{
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9"
			};
            string text = "";
            Random random = new Random();
            for (int i = 0; i < num; i++)
            {
                text += array[random.Next(0, array.Length)];
            }
            return text;
        }

        private static Bitmap GenImg(string code)
        {
            return GenImg(code, Color.DimGray, Color.White);
        }

        private static Bitmap GenImg(string code, Color foreColor, Color backColor)
        {
            return GenImg(code, foreColor, backColor, new Font("Courier New", 18f, FontStyle.Bold));
        }

        private static Bitmap GenImg(string code, Color foreColor, Color backColor, Font font)
        {
            int width = code.Length * 18;
            Bitmap bitmap = new Bitmap(width, 28);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rectangle = new Rectangle(0, 0, width, 28);
            graphics.FillRectangle(new SolidBrush(backColor), rectangle);
            graphics.DrawString(code, font, new SolidBrush(foreColor), rectangle);
            graphics.Dispose();
            return bitmap;
        }
        /// <summary>
        /// Web程序使用  调用处理程序调用该方法 一般处理程序来承载且继承IRequiresSessionState
        /// </summary>
        /// <param name="codeLength"></param>
        public static void GetWebVerifyCode(int codeLength = 4,bool isSimple=false)
        {
            HttpContext context = HttpContext.Current;
            context.Response.Buffer = true;
            context.Response.ExpiresAbsolute = DateTime.Now.AddMilliseconds(0.0);
            context.Response.Expires = 0;
            context.Response.CacheControl = "no-cache";
            context.Response.AppendHeader("Pragma", "No-Cache");
            context.Response.ClearContent();
            context.Response.ContentType = "image/Png";
            context.Response.BinaryWrite(GetVerifyCode(codeLength, isSimple));//
        }
        /// <summary>
        /// 生成验证码 mvc File使用 键session_verifycode
        /// </summary>
        /// <returns></returns>
        public static byte[] GetVerifyCode(int codeLength=4,bool isSimple=false)
        {
            int codeW = 20*codeLength;
            int codeH = 30;
            int fontSize = 16;
            string chkCode = string.Empty;
            //颜色列表，用于验证码、噪线、噪点 
            Color[] color = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.Brown, Color.DarkBlue };
            //字体列表，用于验证码 
            string[] font = { "Times New Roman" };
            //验证码的字符集，去掉了一些容易混淆的字符 
            const string characterA="2345689abde";
            const string characterB = "fhkmnrxyABCDEFGHJKLMNPRSTWXY";
            char[] character;
            if (isSimple)
               character= characterA.ToCharArray();
            else
               character= characterB.ToCharArray();
            Random rnd = new Random();
            //生成验证码字符串 
            for (int i = 0; i < codeLength; i++)
            {
                chkCode += character[rnd.Next(character.Length)];
            }          
            //写入Session、验证码加密
            WebHelper.WriteSession("session_verifycode", SecurityHelper.MD5String16(chkCode.ToUpper()));//一般处理程序实现IRequiresSessionState
            //创建画布
            Bitmap bmp = new Bitmap(codeW, codeH);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            //画噪线 
            for (int i = 0; i < 1; i++)
            {
                int x1 = rnd.Next(codeW);
                int y1 = rnd.Next(codeH);
                int x2 = rnd.Next(codeW);
                int y2 = rnd.Next(codeH);
                Color clr = color[rnd.Next(color.Length)];
                g.DrawLine(new Pen(clr), x1, y1, x2, y2);
            }
            //画验证码字符串 
            for (int i = 0; i < chkCode.Length; i++)
            {
                string fnt = font[rnd.Next(font.Length)];
                Font ft = new Font(fnt, fontSize);
                Color clr = color[rnd.Next(color.Length)];
                g.DrawString(chkCode[i].ToString(), ft, new SolidBrush(clr), (float)i * 18, (float)0);
            }
            //将验证码图片写入内存流，并将其以 "image/Png" 格式输出 
            MemoryStream ms = new MemoryStream();
            try
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                g.Dispose();
                bmp.Dispose();
            }
        }
        /// <summary>
        /// 检查验证码是否正确
        /// </summary>
        /// <param name="inputVerifyCode"></param>
        /// <returns></returns>
        public static bool IsCheckVerifyCode(string inputVerifyCode) 
        {
            HttpContext context = HttpContext.Current;
            if (inputVerifyCode.IsNullEmpty()||context.Session["session_verifycode"].ToString().IsNullEmpty() || SecurityHelper.MD5String16(inputVerifyCode.ToUpper()) != context.Session["session_verifycode"].ToString()) 
            {
                return false;
            }
            return true;
        }
    }
}

using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.ObjectManage.ImageManager
{
    public class WaterImageManager
    {

        public int Padding { get; set; } = 0;
        public ImageFormat PicFormat { get; set; } = ImageFormat.Jpeg;//默认生成图片的格式
        public string SavePath { get; set; }
        public string FileName { get; set; }
        /// <summary>
        /// 生成一个新的水印图片制作实例(默认)
        /// </summary>
        public WaterImageManager()
        {

        }
        public WaterImageManager(int padding)
        {

        }
        /// <summary>
        /// 生成一个新的水印图片制作实例(有参) 
        /// </summary>
        /// <param name="tragetPicName">生成合成图片的文件名称</param>
        /// <param name="tragetPicPath">生成合成图片的文件路径</param>
        /// <param name="padding">指定水印距离父容器边距</param>
        /// <param name="picFormat">指定生成合成图片的图片格式</param>
        public WaterImageManager(string savePath, string fileName, int padding, ImageFormat picFormat = null)
        {
            this.Padding = padding;
            if (picFormat != null)
                this.PicFormat = picFormat;
            this.SavePath = savePath.EndsWith(@"\")? savePath: savePath+@"\";
            this.FileName = fileName;
        }
        private bool IsEnableImageExtension(string extension)
        {
            if (extension == ".gif" || extension == ".jpg" || extension == ".png")
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 合成图片 sourcePicPathName和创建对象的文件名一致则覆盖源文件
        /// </summary>
        /// <param name="sourcePicPathName"></param>
        /// <param name="waterPicPathName"></param>
        /// <param name="alpha">透明度(0.1-1.0数值越小透明度越高)</param>
        /// <param name="position">位置</param>
        /// <returns></returns>
        public string DrawImage(string sourcePicPathName, string waterPicPathName, float alpha, ImagePosition position)
         {
            //检查参数
            if (alpha > 1.0f || alpha <= 0.0f)
            {
                throw new Exception("alpha透明度值只能在0.1-1.0(包含1.0)之间");
            }
            if (!File.Exists(sourcePicPathName))
                throw new FileNotFoundException(sourcePicPathName + " file not found!");
            if (!File.Exists(waterPicPathName))
                throw new FileNotFoundException(waterPicPathName + " file not found!");

            string fileSourceExtension = Path.GetExtension(sourcePicPathName).ToLower();
            string fileWaterExtension = Path.GetExtension(waterPicPathName).ToLower();

            if (!IsEnableImageExtension(fileSourceExtension) || !IsEnableImageExtension(fileWaterExtension))
            {
                throw new Exception("水印图片只能是png|gif|jpg");
            }
            // 将需要加上水印的图片装载到Image对象中   并获取宽高
            Image imgSourcePic = ImageHelper.ReadImageFile(sourcePicPathName);
            int sImgWidth = imgSourcePic.Width;
            int sImgHeight = imgSourcePic.Height;
            //封装 GDI+位图，此位图由图形图像及其属性的像素数据组成。
            Bitmap sourcebmPhoto = new Bitmap(sImgWidth, sImgHeight, imgSourcePic.PixelFormat);//sImgWidth, sImgHeight, PixelFormat.Format24bppRgb  imgSourcePic
            // 设定分辨率
            sourcebmPhoto.SetResolution(imgSourcePic.HorizontalResolution, imgSourcePic.VerticalResolution);
            // 定义一个绘图画面用来装载位图
            Graphics grPhoto = Graphics.FromImage(sourcebmPhoto);
            //同样，由于水印是图片，我们也需要定义一个Image来装载它 获取水印图片的高度和宽度
            Image imgWatermark = ImageHelper.ReadImageFile(waterPicPathName);
            int wmWidth = imgWatermark.Width;
            int wmHeight = imgWatermark.Height;

            //SmoothingMode：指定是否将平滑处理（消除锯齿）应用于直线、曲线和已填充区域的边缘。
            // 成员名称  说明 
            // AntiAlias   指定消除锯齿的呈现。 
            // Default    指定不消除锯齿。
            // HighQuality 指定高质量、低速度呈现。 
            // HighSpeed  指定高速度、低质量呈现。 
            // Invalid    指定一个无效模式。 
            // None     指定不消除锯齿。 
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
            // 开始描绘，将我们的底图描绘在绘图画面上
            grPhoto.DrawImage(imgSourcePic, new Rectangle(0, 0, sImgWidth, sImgHeight), 0, 0, sImgWidth, sImgHeight, GraphicsUnit.Pixel);
            // 与底图一样，我们需要一个位图来装载水印图片。并设定其分辨率
            Bitmap bmWatermark = new Bitmap(sourcebmPhoto);
            bmWatermark.SetResolution(imgWatermark.HorizontalResolution, imgWatermark.VerticalResolution);
            // 继续，将水印图片装载到一个绘图画面grWatermark
            Graphics grWatermark = Graphics.FromImage(bmWatermark);
            //ImageAttributes 对象包含有关在呈现时如何操作位图和图元文件颜色的信息。
            ImageAttributes imageAttributes = new ImageAttributes();
            //Colormap: 定义转换颜色的映射
            ColorMap colorMap = new ColorMap();
            //我的水印图被定义成拥有绿色背景色的图片被替换成透明
            colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
            colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
            ColorMap[] remapTable = { colorMap };
            imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);
            float[][] colorMatrixElements ={
                new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f}, //red红色
                new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f}, //green绿色
                new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f}, //blue蓝色    
                new float[] {0.0f, 0.0f, 0.0f, alpha, 0.0f},//透明度   
                new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}};
            // ColorMatrix:定义包含 RGBA 空间坐标的 5 x 5 矩阵。
            // ImageAttributes 类的若干方法通过使用颜色矩阵调整图像颜色。
            ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);

            imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            //上面设置完颜色，下面开始设置位置
            int xPosOfWm;
            int yPosOfWm;

            switch (position)
            {
                case ImagePosition.BottomMiddle:
                    xPosOfWm = (sImgWidth - wmWidth) / 2;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
                case ImagePosition.Center:
                    xPosOfWm = (sImgWidth - wmWidth) <= 0 ? 0 : (sImgWidth - wmWidth) / 2;
                    yPosOfWm = (sImgHeight - wmHeight) / 2;
                    break;
                case ImagePosition.LeftBottom:
                    xPosOfWm = Padding;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
                case ImagePosition.LeftTop:
                    xPosOfWm = Padding;
                    yPosOfWm = Padding;
                    break;
                case ImagePosition.RightTop:
                    xPosOfWm = sImgWidth - wmWidth - Padding;
                    yPosOfWm = Padding;
                    break;
                case ImagePosition.RigthBottom:
                    xPosOfWm = sImgWidth - wmWidth - Padding;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
                case ImagePosition.TopMiddle:
                    xPosOfWm = (sImgWidth - wmWidth) / 2;
                    yPosOfWm = Padding;
                    break;
                default:
                    xPosOfWm = Padding;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
            }
            imgSourcePic.Dispose();//释放底图，解决图片保存时 “GDI+ 中发生一般性错误。”
            // 第二次绘图，把水印印上去
            grWatermark.DrawImage(imgWatermark, new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes);
            Image  newImgSourcePic = new Bitmap(bmWatermark);
            grPhoto.Dispose();
            grWatermark.Dispose();
            bmWatermark.Dispose();
            imgWatermark.Dispose();
            // 保存文件到服务器的文件夹里面
            if (this.SavePath.IsNullEmpty())
            {
                this.SavePath = Path.GetDirectoryName(sourcePicPathName) + @"\";
                this.FileName =  Path.GetFileName(sourcePicPathName);
            }             
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            string filePathName = SavePath + FileName;
            try
            {
                // newImgSourcePic.Save(filePathName, PicFormat);//原
                using (MemoryStream ms = new MemoryStream())
                {
                    newImgSourcePic.Save(ms, ImageFormat.Bmp);//用内存流保存 要不远程客户端会报错 “GDI+ 中发生一般性错误。”                       
                    //重新生成Image对象 
                    Image img2 = Image.FromStream(ms);
                    img2.Save(filePathName, PicFormat);                
                    return filePathName;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                newImgSourcePic.Dispose();             
            }
        }

        /// <summary>
        /// 在图片上添加水印文字
        /// </summary>
        /// <param name="sourcePicture">源图片文件名(包含后缀)</param>
        /// <param name="waterWords">需要添加到图片上的文字</param>
        /// <param name="alpha">透明度（取值区间(0.0,1.0]）</param>
        /// <param name="position">位置</param>
        /// <param name="PicturePath">文件路径</param>
        /// <returns></returns>
        public string DrawWords(string sourcePicPathName,string waterWords, float alpha, FontFamilys fontFamily, FontStyle style, ImagePosition position)
        {
            if (string.IsNullOrEmpty(waterWords)) {
                throw new Exception("水印文字不能为空");
            }
            if (!File.Exists(sourcePicPathName))
                throw new FileNotFoundException(sourcePicPathName + " file not found!");
            string fileExtension = Path.GetExtension(sourcePicPathName).ToLower();
            if (!IsEnableImageExtension(fileExtension))
            {
                throw new Exception("水印背景图片只能是png|gif|jpg");
            }
            //创建一个图片对象用来装载要被添加水印的图片
            Image imgSourcePic = Image.FromFile(sourcePicPathName);

            //获取图片的宽和高
            int sImgWidth = imgSourcePic.Width;
            int sImgHeight = imgSourcePic.Height;

            //
            //建立一个bitmap，和我们需要加水印的图片一样大小
            Bitmap bmPhoto = new Bitmap(sImgWidth, sImgHeight, imgSourcePic.PixelFormat);

            //SetResolution：设置此 Bitmap 的分辨率
            //这里直接将我们需要添加水印的图片的分辨率赋给了bitmap
            bmPhoto.SetResolution(imgSourcePic.HorizontalResolution, imgSourcePic.VerticalResolution);

            //Graphics：封装一个 GDI+ 绘图图面。
            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            //设置图形的品质
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

            //将我们要添加水印的图片按照原始大小描绘（复制）到图形中
            grPhoto.DrawImage(
                imgSourcePic,                    //  要添加水印的图片
                new Rectangle(0, 0, sImgWidth, sImgHeight), // 根据要添加的水印图片的宽和高
                0,                           // X方向从0点开始描绘
                0,                           // Y方向

                sImgWidth,                     // X方向描绘长度
                sImgHeight,                    // Y方向描绘长度
                GraphicsUnit.Pixel);         // 描绘的单位，这里用的是像素

            //根据图片的大小我们来确定添加上去的文字的大小
            //在这里我们定义一个数组来确定
            int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4, 2 };

            //字体
            Font crFont = null;
            //矩形的宽度和高度，SizeF有三个属性，分别为Height高，width宽，IsEmpty是否为空
            SizeF crSize = new SizeF();

            //利用一个循环语句来选择我们要添加文字的型号
            //直到它的长度比图片的宽度小
            for (int i = 0; i < 8; i++)
            {
                crFont = new Font(fontFamily.ToString(), sizes[i], style);

                //测量用指定的 Font 对象绘制并用指定的 StringFormat 对象格式化的指定字符串。
                crSize = grPhoto.MeasureString(waterWords, crFont);

                // ushort 关键字表示一种整数数据类型
                if ((ushort)crSize.Width < (ushort)sImgWidth)
                    break;
            }

            //截边5%的距离，定义文字显示(由于不同的图片显示的高和宽不同，所以按百分比截取)
            int yPixlesFromBottom = (int)(sImgHeight * .05);

            //定义在图片上文字的位置
            float wmHeight = crSize.Height;
            float wmWidth = crSize.Width;

            float xPosOfWm;
            float yPosOfWm;

            switch (position)
            {
                case ImagePosition.BottomMiddle:
                    xPosOfWm = sImgWidth / 2;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
                case ImagePosition.Center:
                    xPosOfWm = sImgWidth / 2;
                    yPosOfWm = sImgHeight / 2;
                    break;
                case ImagePosition.RigthBottom:
                    xPosOfWm = sImgWidth - wmWidth - Padding;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
                case ImagePosition.RightTop:
                    xPosOfWm = sImgWidth / 2 + wmWidth / 2;
                    yPosOfWm = wmHeight / 2 + Padding;
                    break;
                case ImagePosition.LeftTop:
                    xPosOfWm = wmWidth / 2 + Padding;
                    yPosOfWm = wmHeight / 2 + Padding;
                    break;
                case ImagePosition.LeftBottom:
                    xPosOfWm = wmWidth / 2 + Padding;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
                case ImagePosition.TopMiddle:
                    xPosOfWm = sImgWidth / 2;
                    yPosOfWm = wmHeight / 2 + Padding;
                    break;
                default:
                    xPosOfWm = wmWidth;
                    yPosOfWm = sImgHeight - wmHeight - Padding;
                    break;
            }

            imgSourcePic.Dispose();//释放底图，解决图片保存时 “GDI+ 中发生一般性错误。”

            //封装文本布局信息（如对齐、文字方向和 Tab 停靠位），显示操作（如省略号插入和国家标准 (National) 数字替换）和 OpenType 功能。
            StringFormat StrFormat = new StringFormat();

            //定义需要印的文字居中对齐
            StrFormat.Alignment = StringAlignment.Center;

            //SolidBrush:定义单色画笔。画笔用于填充图形形状，如矩形、椭圆、扇形、多边形和封闭路径。
            //这个画笔为描绘阴影的画笔，呈灰色
            int m_alpha = Convert.ToInt32(255 * alpha);
            SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(m_alpha, 0, 0, 0));

            //描绘文字信息，这个图层向右和向下偏移一个像素，表示阴影效果
            //DrawString 在指定矩形并且用指定的 Brush 和 Font 对象绘制指定的文本字符串。
            grPhoto.DrawString(waterWords,                        //string of text
                          crFont,                                 //font
                          semiTransBrush2,                        //Brush
                          new PointF(xPosOfWm + 1, yPosOfWm + 1), //Position
                          StrFormat);

            //从四个 ARGB 分量（alpha、红色、绿色和蓝色）值创建 Color 结构，这里设置透明度为153
            //这个画笔为描绘正式文字的笔刷，呈白色
            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));

            //第二次绘制这个图形，建立在第一次描绘的基础上
            grPhoto.DrawString(waterWords,                //string of text
                          crFont,                         //font
                          semiTransBrush,                 //Brush
                          new PointF(xPosOfWm, yPosOfWm), //Position
                          StrFormat);

            //imgSourcePic是我们建立的用来装载最终图形的Image对象
            //bmPhoto是我们用来制作图形的容器，为Bitmap对象
            imgSourcePic = bmPhoto;
            //释放资源，将定义的Graphics实例grPhoto释放，grPhoto功德圆满
            grPhoto.Dispose();

            //将grPhoto保存
            if (Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            string filePathName = SavePath + FileName;
            try
            {
                imgSourcePic.Save(filePathName, PicFormat);
            }
            catch (Exception e)
            {
                throw e;
            }
            imgSourcePic.Dispose();
            return filePathName;
        }
    }
}

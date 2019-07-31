using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PXLib.Helpers;
using PXLib.Offices.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PXLib.Offices
{
    public class ExcelHelper
    {
        #region Excel导出方法 ExcelDownload
        /// <summary>
        /// Excel导出下载
        /// </summary>
        /// <param name="dtSource">DataTable数据源</param>
        /// <param name="excelConfig">导出设置包含文件名、标题、列设置</param>
        public static void ExcelDownload(DataTable dtSource, ExcelConfig excelConfig)
        {
            HttpContext curContext = HttpContext.Current;
            // 设置编码和附件格式
            curContext.Response.ContentType = "application/ms-excel";
            curContext.Response.ContentEncoding = Encoding.UTF8;
            curContext.Response.Charset = "";
            curContext.Response.AppendHeader("Content-Disposition",
                "attachment;filename=" + HttpUtility.UrlEncode(excelConfig.FileName, Encoding.UTF8));
            //调用导出具体方法Export()
            curContext.Response.BinaryWrite(ExportMemoryStream(dtSource, excelConfig).GetBuffer());
            curContext.Response.End();
        }
        /// <summary>
        /// Excel导出下载
        /// </summary>
        /// <param name="list">数据源</param>
        /// <param name="templdateName">模板文件名</param>
        /// <param name="newFileName">文件名</param>
        //public static void ExcelDownload(List<TemplateMode> list, string templdateName, string newFileName)
        //{
        //    HttpResponse response = System.Web.HttpContext.Current.Response;
        //    response.Clear();
        //    response.Charset = "UTF-8";
        //    response.ContentType = "application/vnd-excel";//"application/vnd.ms-excel";
        //    System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment; filename=" + newFileName));
        //    System.Web.HttpContext.Current.Response.BinaryWrite(ExportListByTempale(list, templdateName).ToArray());
        //}
        #endregion
        #region NPOI导出表格
        #region DataTable导出到Excel的MemoryStream
        /// <summary>
        /// DataTable导出到Excel的MemoryStream Export()
        /// </summary>
        /// <param name="dtSource">DataTable数据源</param>
        /// <param name="excelConfig">导出设置包含文件名、标题、列设置</param>
        public static MemoryStream ExportMemoryStream(DataTable dtSource, ExcelConfig excelConfig)
        {
            int colint = 0;
            for (int i = 0; i < dtSource.Columns.Count; )
            {
                DataColumn column = dtSource.Columns[i];
                if (excelConfig.ColumnEntity[colint].Column != column.ColumnName)
                {
                    dtSource.Columns.Remove(column.ColumnName);
                }
                else
                {
                    i++;
                    colint++;
                }
            }

            HSSFWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet();
            #region 右击文件 属性信息
            {
                DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
                dsi.Company = "NPOI";
                workbook.DocumentSummaryInformation = dsi;

                SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
                si.Author = "PX"; //填加xls文件作者信息
                si.ApplicationName = "PXLib"; //填加xls文件创建程序信息
                si.LastAuthor = "PX"; //填加xls文件最后保存者信息
                si.Comments = "PX"; //填加xls文件作者信息
                si.Title = "标题信息"; //填加xls文件标题信息
                si.Subject = "主题信息";//填加文件主题信息
                si.CreateDateTime = System.DateTime.Now;
                workbook.SummaryInformation = si;
            }
            #endregion

            #region 设置标题样式
            ICellStyle headStyle = workbook.CreateCellStyle();
            int[] arrColWidth = new int[dtSource.Columns.Count];
            string[] arrColName = new string[dtSource.Columns.Count];//列名
            ICellStyle[] arryColumStyle = new ICellStyle[dtSource.Columns.Count];//样式表
            headStyle.Alignment = HorizontalAlignment.Center; // ------------------
            if (excelConfig.Background != new Color())
            {
                if (excelConfig.Background != new Color())
                {
                    headStyle.FillPattern = FillPattern.SolidForeground;
                    headStyle.FillForegroundColor = GetXLColour(workbook, excelConfig.Background);
                }
            }
            IFont font = workbook.CreateFont();
            font.FontHeightInPoints = excelConfig.TitlePoint;
            if (excelConfig.ForeColor != new Color())
            {
                font.Color = GetXLColour(workbook, excelConfig.ForeColor);
            }
            font.Boldweight = 700;
            headStyle.SetFont(font);
            #endregion

            #region 列头及样式
            ICellStyle cHeadStyle = workbook.CreateCellStyle();
            cHeadStyle.Alignment = HorizontalAlignment.Center; // ------------------
            IFont cfont = workbook.CreateFont();
            cfont.FontHeightInPoints = excelConfig.HeadPoint;
            cHeadStyle.SetFont(cfont);
            #endregion

            #region 设置内容单元格样式
            foreach (DataColumn item in dtSource.Columns)
            {
                ICellStyle columnStyle = workbook.CreateCellStyle();
                columnStyle.Alignment = HorizontalAlignment.Center;
                arrColWidth[item.Ordinal] = Encoding.GetEncoding(936).GetBytes(item.ColumnName.ToString()).Length;
                arrColName[item.Ordinal] = item.ColumnName.ToString();
                if (excelConfig.ColumnEntity != null)
                {
                    ColumnEntity columnentity = excelConfig.ColumnEntity.Find(t => t.Column == item.ColumnName);
                    if (columnentity != null)
                    {
                        arrColName[item.Ordinal] = columnentity.ExcelColumn;
                        if (columnentity.Width != 0)
                        {
                            arrColWidth[item.Ordinal] = columnentity.Width;
                        }
                        if (columnentity.Background != new Color())
                        {
                            if (columnentity.Background != new Color())
                            {
                                columnStyle.FillPattern = FillPattern.SolidForeground;
                                columnStyle.FillForegroundColor = GetXLColour(workbook, columnentity.Background);
                            }
                        }
                        if (columnentity.Font != null || columnentity.Point != 0 || columnentity.ForeColor != new Color())
                        {
                            IFont columnFont = workbook.CreateFont();
                            columnFont.FontHeightInPoints = 10;
                            if (columnentity.Font != null)
                            {
                                columnFont.FontName = columnentity.Font;
                            }
                            if (columnentity.Point != 0)
                            {
                                columnFont.FontHeightInPoints = columnentity.Point;
                            }
                            if (columnentity.ForeColor != new Color())
                            {
                                columnFont.Color = GetXLColour(workbook, columnentity.ForeColor);
                            }
                            columnStyle.SetFont(font);
                        }
                        columnStyle.Alignment = getAlignment(columnentity.Alignment);
                    }
                }
                arryColumStyle[item.Ordinal] = columnStyle;
            }
            if (excelConfig.IsAllSizeColumn)
            {
                #region 根据列中最长列的长度取得列宽
                for (int i = 0; i < dtSource.Rows.Count; i++)
                {
                    for (int j = 0; j < dtSource.Columns.Count; j++)
                    {
                        if (arrColWidth[j] != 0)
                        {
                            int intTemp = Encoding.GetEncoding(936).GetBytes(dtSource.Rows[i][j].ToString()).Length;
                            if (intTemp > arrColWidth[j])
                            {
                                arrColWidth[j] = intTemp;
                            }
                        }

                    }
                }
                #endregion
            }
            #endregion

            #region 填充数据

            #endregion
            ICellStyle dateStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");
            int rowIndex = 0;
            foreach (DataRow row in dtSource.Rows)
            {
                #region 新建表，填充表头，填充列头，样式
                if (rowIndex == 65535 || rowIndex == 0)
                {
                    if (rowIndex != 0)
                    {
                        sheet = workbook.CreateSheet();
                    }

                    #region 表头及样式
                    {
                        if (excelConfig.Title != null)
                        {
                            IRow headerRow = sheet.CreateRow(0);
                            if (excelConfig.TitleHeight != 0)
                            {
                                headerRow.Height = (short)(excelConfig.TitleHeight * 20);
                            }
                            headerRow.HeightInPoints = 25;
                            headerRow.CreateCell(0).SetCellValue(excelConfig.Title);
                            headerRow.GetCell(0).CellStyle = headStyle;
                            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, dtSource.Columns.Count - 1)); // ------------------
                        }

                    }
                    #endregion

                    #region 列头及样式
                    {
                        IRow headerRow = sheet.CreateRow(1);
                        #region 如果设置了列标题就按列标题定义列头，没定义直接按字段名输出
                        foreach (DataColumn column in dtSource.Columns)
                        {
                            headerRow.CreateCell(column.Ordinal).SetCellValue(arrColName[column.Ordinal]);
                            headerRow.GetCell(column.Ordinal).CellStyle = cHeadStyle;
                            //设置列宽
                            sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
                        }
                        #endregion
                    }
                    #endregion

                    rowIndex = 2;
                }
                #endregion

                #region 填充内容
                IRow dataRow = sheet.CreateRow(rowIndex);
                foreach (DataColumn column in dtSource.Columns)
                {
                    ICell newCell = dataRow.CreateCell(column.Ordinal);
                    newCell.CellStyle = arryColumStyle[column.Ordinal];
                    string drValue = row[column].ToString();
                    SetCell(newCell, dateStyle, column.DataType, drValue);
                }
                #endregion
                rowIndex++;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                return ms;
            }
        }
        #endregion
        #region 设置表格内容
        private static void SetCell(ICell newCell, ICellStyle dateStyle, Type dataType, string drValue)
        {
            switch (dataType.ToString())
            {
                case "System.String"://字符串类型
                    newCell.SetCellValue(drValue);
                    break;
                case "System.DateTime"://日期类型
                    System.DateTime dateV;
                    if (System.DateTime.TryParse(drValue, out dateV))
                    {
                        newCell.SetCellValue(dateV);
                    }
                    else
                    {
                        newCell.SetCellValue("");
                    }
                    newCell.CellStyle = dateStyle;//格式化显示
                    break;
                case "System.Boolean"://布尔型
                    bool boolV = false;
                    bool.TryParse(drValue, out boolV);
                    newCell.SetCellValue(boolV);
                    break;
                case "System.Int16"://整型
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                    int intV = 0;
                    int.TryParse(drValue, out intV);
                    newCell.SetCellValue(intV);
                    break;
                case "System.Decimal"://浮点型
                case "System.Double":
                    double doubV = 0;
                    double.TryParse(drValue, out doubV);
                    newCell.SetCellValue(doubV);
                    break;
                case "System.DBNull"://空值处理
                    newCell.SetCellValue("");
                    break;
                default:
                    newCell.SetCellValue("");
                    break;
            }
        }
        #endregion
        #region RGB颜色转NPOI颜色
        private static short GetXLColour(HSSFWorkbook workbook, Color SystemColour)
        {
            short s = 0;
            HSSFPalette XlPalette = workbook.GetCustomPalette();
            NPOI.HSSF.Util.HSSFColor XlColour = XlPalette.FindColor(SystemColour.R, SystemColour.G, SystemColour.B);
            if (XlColour == null)
            {
                if (NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE < 255)
                {
                    XlColour = XlPalette.FindSimilarColor(SystemColour.R, SystemColour.G, SystemColour.B);
                    s = XlColour.Indexed;
                }

            }
            else
                s = XlColour.Indexed;
            return s;
        }
        #endregion

        #region 设置列的对齐方式
        /// <summary>
        /// 设置对齐方式
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        private static HorizontalAlignment getAlignment(string style)
        {
            switch (style)
            {
                case "center":
                    return HorizontalAlignment.Center;
                case "left":
                    return HorizontalAlignment.Left;
                case "right":
                    return HorizontalAlignment.Right;
                case "fill":
                    return HorizontalAlignment.Fill;
                case "justify":
                    return HorizontalAlignment.Justify;
                case "centerselection":
                    return HorizontalAlignment.CenterSelection;
                case "distributed":
                    return HorizontalAlignment.Distributed;
            }
            return NPOI.SS.UserModel.HorizontalAlignment.General;


        }

        #endregion
        #endregion
        #region 从Excel导入
        /// <summary>
        /// 读取excel ,默认第一行为标头
        /// </summary>
        /// <param name="strFileName">excel文档路径</param>
        /// <returns></returns>
        //public static DataTable ExcelFileToDataTable(string strFilePathName)
        //{
        //    DataTable dt = new DataTable();

        //    ISheet sheet = null;
        //    using (FileStream file = new FileStream(strFilePathName, FileMode.Open, FileAccess.Read))
        //    {
        //        if (strFilePathName.IndexOf(".xlsx") == -1)//2003
        //        {
        //            HSSFWorkbook hssfworkbook = new HSSFWorkbook(file);
        //            sheet = hssfworkbook.GetSheetAt(0);
        //        }
        //        else//2007
        //        {
        //            XSSFWorkbook xssfworkbook = new XSSFWorkbook(file);
        //            sheet = xssfworkbook.GetSheetAt(0);
        //        }
        //    }

        //    System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

        //    IRow headerRow = sheet.GetRow(0);
        //    int cellCount = headerRow.LastCellNum;

        //    for (int j = 0; j < cellCount; j++)
        //    {
        //        ICell cell = headerRow.GetCell(j);
        //        dt.Columns.Add(cell.ToString());
        //    }

        //    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
        //    {
        //        IRow row = sheet.GetRow(i);
        //        DataRow dataRow = dt.NewRow();

        //        for (int j = row.FirstCellNum; j < cellCount; j++)
        //        {
        //            if (row.GetCell(j) != null)
        //                dataRow[j] = row.GetCell(j).ToString();
        //        }

        //        dt.Rows.Add(dataRow);
        //    }
        //    return dt;
        //}
        #endregion
        #region Excel导出数据 原
        /// <summary>
        /// Excel导出数据
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="fileName">文件名</param>
        public static void ExportExcel(DataTable data, string fileName)
        {
            try
            {
                if (data != null && data.Rows.Count > 0)
                {
                    HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                    HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                    HttpContext.Current.Response.Charset = "Utf-8";
                    HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName + ".xls", Encoding.UTF8));

                    StringBuilder sbHtml = new StringBuilder();
                    sbHtml.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
                    sbHtml.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");

                    //写出列名
                    sbHtml.AppendLine("<tr style=\"background-color: #FFE88C;font-weight: bold; white-space: nowrap;\">");

                    foreach (DataColumn column in data.Columns)
                    {
                        sbHtml.AppendLine("<td>" + column.ColumnName + "</td>");
                    }
                    sbHtml.AppendLine("</tr>");
                    //写数据

                    foreach (System.Data.DataRow row in data.Rows)
                    {
                        sbHtml.Append("<tr>");

                        foreach (System.Data.DataColumn column in data.Columns)
                        {
                            //sbHtml.Append("<td>").Append(row[column].ToString()).Append("</td>");
                            sbHtml.Append(string.Format("<td style='mso-number-format:general;mso-number-format:\"{0}\"'>{1}</td>", @"\@", row[column].ToString()));//处理数字到文本  
                        }
                        sbHtml.AppendLine("</tr>");
                    }
                    sbHtml.AppendLine("</table>");
                    HttpContext.Current.Response.Write(sbHtml.ToString());
                    HttpContext.Current.Response.End();
                }
            }
            catch (Exception ex)
            {
                new LogHelper("Excel导出数据异常").WriteLog(ex);
            }
        }
        /// <summary>
        /// 导出Excel文件，并自定义文件名
        /// </summary>
        public static void DataTableToExcel(System.Data.DataTable dtData, String FileName)
        {
            GridView dgExport = null;
            HttpContext curContext = HttpContext.Current;
            StringWriter strWriter = null;
            HtmlTextWriter htmlWriter = null;

            if (dtData != null)
            {
                HttpUtility.UrlEncode(FileName, System.Text.Encoding.UTF8);
                curContext.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(FileName, System.Text.Encoding.UTF8) + ".xls");
                curContext.Response.ContentType = "application nd.ms-excel";
                curContext.Response.ContentEncoding = System.Text.Encoding.UTF8;
                curContext.Response.Charset = "GB2312";
                strWriter = new StringWriter();
                htmlWriter = new HtmlTextWriter(strWriter);
                dgExport = new GridView();
                dgExport.DataSource = dtData.DefaultView;
                dgExport.AllowPaging = false;
                dgExport.DataBind();
                dgExport.RenderControl(htmlWriter);
                curContext.Response.Write(strWriter.ToString());
                curContext.Response.End();
            }
        }
        #endregion
    }
}

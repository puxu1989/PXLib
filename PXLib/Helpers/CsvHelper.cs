using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace PXLib.Helpers
{
    public class CsvHelper
    {
        /// <summary>
        /// 导出报表为Csv
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="strFilePath">物理路径</param>
        /// <param name="tableheader">表头</param>
        /// <param name="columname">字段标题,逗号分隔</param>
        public static bool DataTableToCsv(DataTable dt, string strFilePath, string tableheader, string columname)
        {
            try
            {
                if (string.IsNullOrEmpty(strFilePath))
                {
                    return false;
                }
                StreamWriter strmWriterObj = new StreamWriter(strFilePath, false, System.Text.Encoding.UTF8);
                if (!string.IsNullOrEmpty(tableheader))
                {
                    strmWriterObj.WriteLine(tableheader);
                }
                if (!string.IsNullOrEmpty(columname))
                {
                    strmWriterObj.WriteLine(columname);
                }

                string strBufferLine = string.Empty;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    strBufferLine = string.Empty;
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j > 0)
                            strBufferLine += ",";
                        strBufferLine += dt.Rows[i][j].ToString();
                    }
                    strmWriterObj.WriteLine(strBufferLine);
                }
                strmWriterObj.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将Csv读入DataTable
        /// </summary>
        /// <param name="filePath">csv文件路径</param>
        /// <param name="n">表示第n行是字段title,第n+1行是记录开始</param>
        public static DataTable CsvToDataTable(string filePathName, int n)
        {
            DataTable dt = new DataTable();
            StreamReader reader = new StreamReader(filePathName, System.Text.Encoding.UTF8, false);
            int i = 0;
            int m = 0;
            reader.Peek();
            while (reader.Peek() > 0)
            {
                m = m + 1;
                string str = reader.ReadLine();
                if (m >= n + 1)
                {
                    string[] split = str.Split(',');
                    System.Data.DataRow dr = dt.NewRow();
                    for (i = 0; i < split.Length; i++)
                    {
                        dr[i] = split[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
        /// <summary>
        /// DataTable 生成 CSV 返回CSV字符串
        /// </summary>
        /// <param name="dt">DataTable</param>
        public static string DataTableToCSV(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "NO";
            StringBuilder csvText = new StringBuilder();
            StringBuilder csvrowText = new StringBuilder();
            foreach (DataColumn dc in dt.Columns)
            {
                csvrowText.Append(",");
                csvrowText.Append(dc.ColumnName);
            }
            csvText.AppendLine(csvrowText.ToString().Substring(1));

            foreach (DataRow dr in dt.Rows)
            {
                csvrowText = new StringBuilder();
                foreach (DataColumn dc in dt.Columns)
                {
                    csvrowText.Append(",");
                    csvrowText.Append(dr[dc.ColumnName].ToString().Replace(',', ' '));
                }
                csvText.AppendLine(csvrowText.ToString().Substring(1));
            }
            return csvText.ToString();
        }
        /// <summary>
        /// DataTable 生成 CSV
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="csvPath">csv文件路径</param>
        public static void DataTableToCSV(DataTable dt, string csvPath)
        {
            if (null == dt)
                return;

            StringBuilder csvText = new StringBuilder();
            StringBuilder csvrowText = new StringBuilder();
            foreach (DataColumn dc in dt.Columns)
            {
                csvrowText.Append(",");
                csvrowText.Append(dc.ColumnName);
            }
            csvText.AppendLine(csvrowText.ToString().Substring(1));

            foreach (DataRow dr in dt.Rows)
            {
                csvrowText = new StringBuilder();
                foreach (DataColumn dc in dt.Columns)
                {
                    csvrowText.Append(",");
                    csvrowText.Append(dr[dc.ColumnName].ToString().Replace(',', ' '));
                }
                csvText.AppendLine(csvrowText.ToString().Substring(1));
            }

            File.WriteAllText(csvPath, csvText.ToString(), Encoding.Default);
        }
    }
}

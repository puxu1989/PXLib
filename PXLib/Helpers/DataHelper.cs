using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Helpers
{
   public class DataHelper
    {
        #region DataTable根据条件过滤表的内容
        /// <summary>
        /// 根据条件过滤表的内容
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static DataTable DataFilter(DataTable dt, string condition)
        {
            if (DataHelper.IsExistRows(dt))
            {
                if (condition.Trim() == "")
                {
                    return dt;
                }
                else
                {
                    DataTable newdt = new DataTable();
                    newdt = dt.Clone();
                    DataRow[] dr = dt.Select(condition);
                    for (int i = 0; i < dr.Length; i++)
                    {
                        newdt.ImportRow((DataRow)dr[i]);
                    }
                    return newdt;
                }
            }
            else
            {
                return null;
            }
        }
        public static DataTable DataFilter(DataTable dt, string condition, string sort)
        {
            if (DataHelper.IsExistRows(dt))
            {
                DataTable newdt = new DataTable();
                newdt = dt.Clone();
                DataRow[] dr = dt.Select(condition, sort);
                for (int i = 0; i < dr.Length; i++)
                {
                    newdt.ImportRow((DataRow)dr[i]);
                }
                return newdt;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 检查DataTable 是否有数据行
        /// <summary>
        /// 检查DataTable 是否有数据行
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public static bool IsExistRows(DataTable dt)
        {
            if (dt != null && dt.Rows.Count > 0)
                return true;

            return false;
        }
        #endregion
        #region DataTable/DataSet 转 XML
        /// <summary>
        /// DataTable 转 XML
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DataTableToXML(DataTable dt)
        {
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    System.IO.StringWriter writer = new System.IO.StringWriter();
                    dt.WriteXml(writer);
                    return writer.ToString();
                }
            }
            return String.Empty;
        }
        /// <summary>
        /// DataSet 转 XML
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string DataSetToXML(DataSet ds)
        {
            if (ds != null)
            {
                System.IO.StringWriter writer = new System.IO.StringWriter();
                ds.WriteXml(writer);
                return writer.ToString();
            }
            return String.Empty;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.WebControl
{
    /// <summary>
    /// 分页类 字段名匹配前端js框架
    /// </summary>
    public class Pagination
    {

        /// <summary>
        /// 每页行数 pageSize
        /// </summary>
        public int rows { get; set; }
        /// <summary>
        /// 当前页 pageIndex
        /// </summary>
        public int page { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public string sidx { get; set; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public string sord { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int records { get; set; }
        /// <summary>
        /// 总分页数
        /// </summary>
        public int total
        {
            get
            {
                if (records > 0)
                {
                    return records % this.rows == 0 ? records / this.rows : records / this.rows + 1;//如果能除尽就总条数除以每页数 否则+1页
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 查询条件Json
        /// </summary>
        public string queryJson { get; set; }

        /// <summary>
        /// 查询条件Json
        /// </summary>
        public string conditionJson { get; set; }
        /// <summary>
        /// 构造设置初始值
        /// </summary>
        public Pagination(int pageIndex, int pageSize, string sortCol, string sortType) 
        {
            this.rows = pageSize;
            this.page = pageIndex;
            this.sord = sortType;
            this.sidx = sortCol;
        }
        public Pagination()
        {
        }
        public static PagingBase ConvertToBasePager(Pagination pager)
        {
            PagingBase pbase = new PagingBase();
            pbase.PageIndex = pager.page;
            pbase.PageSize = pager.rows;
            if (!pager.sord.IsNullEmpty())
            {
                pbase.SortType = pager.sord;

            }
            pbase.SortField = pager.sidx;
            pbase.QueryJson = pager.queryJson;
            return pbase;
        }
    }
}

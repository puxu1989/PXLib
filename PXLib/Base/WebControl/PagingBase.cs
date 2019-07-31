using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.WebControl
{
    /// <summary>
    /// 分页基础类
    /// </summary>
    public class PagingBase
    {
        private int _pageindex = 1;//默认值为1
        public int PageIndex
        {
            get
            {
                if (_pageindex <=1)
                    return 1;
                else return _pageindex;
            }
            set { this._pageindex = value; }
        }

        private int _pagesize = 1;//默认值为1
        public int PageSize
        {
            get
            {
                if (_pagesize <=1)
                    return 1;
                else return _pagesize;
            }
            set { this._pagesize = value; }
        }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortField { get; set; }

        private string _sorttype = "asc";
        /// <summary>
        /// 排序类型
        /// </summary>
        public string SortType { get { return _sorttype; } set { this._sorttype = value; } }
        /// <summary>
        /// 记录总条数
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// 查询条件
        /// </summary>
        public string QueryJson { get; set; }
        /// <summary>
        /// 分页总是
        /// </summary>
        public int PageCount
        {
            get
            {
                if (RecordCount > 0)
                {
                    return RecordCount % this._pagesize == 0 ? RecordCount / this._pagesize : RecordCount / this._pagesize + 1;//如果能除尽就总条数除以每页数 否则+1页
                }
                else
                {
                    return 0;
                }
            }
        }
        public PagingBase() { }
        public PagingBase(int pageIndex, int pageSize, string sortField, string sortType="Asc") 
        {
            this.PageIndex =pageIndex;
            this.PageSize =pageSize;
            this.SortField = sortField;
            this.SortType = sortType;
        }
        /// <summary>
        /// 添加单条件查询参数
        /// </summary>
        /// <param name="condition">条件字段</param>
        /// <param name="keyword">条件参数</param>
        public void AddQueryJson(string condition, object keyword) 
        {
            if (!condition.IsNullEmpty() && !keyword.IsNullEmpty())
            {
                var queryJsonObj = new { condition=condition,keyword=keyword };
                this.QueryJson= queryJsonObj.ToJson();
            }
        }
    }
}

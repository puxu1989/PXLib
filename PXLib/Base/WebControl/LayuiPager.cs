using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.WebControl
{
   public class LayuiPager
    {
        public int limit { get; set; }
        /// <summary>
        /// 当前页 pageIndex
        /// </summary>
        public int page { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public string field { get; set; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public string order { get; set; }
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
                    return records % this.limit == 0 ? records / this.limit : records / this.limit + 1;//如果能除尽就总条数除以每页数 否则+1页
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

        public LayuiPager() { }
        /// <summary>
        /// 构造设置初始值
        /// </summary>
        public LayuiPager(int pageIndex, int pageSize, string sortCol, string sortType) 
        {
            this.limit = pageSize;
            this.page = pageIndex;
            this.order= sortType;
            this.field = sortCol;
        }
        public static PagingBase ConvertToPaging(LayuiPager pager)
        {
            PXLib.Helpers.ValidationHelper.ArgumentNotNull(pager,"分页数据");
            PagingBase pbase = new PagingBase();
            pbase.PageIndex = pager.page;
            pbase.PageSize = pager.limit;
            if (!pager.order.IsNullEmpty())
            {
                pbase.SortType = pager.order;
               
            }
            pbase.SortField = pager.field;
            pbase.QueryJson = pager.queryJson;
            return pbase;
        }
        public static object GetLayuiPageData(object data, PagingBase basepage) 
        {
            var LayuiData = new
            {
                code = 0,
                msg = "",
                page = basepage.PageIndex,
                count = basepage.RecordCount,
                data = data,
            };
            return LayuiData;
        }
        public static object GetLayuiPageData(object data) 
        {
            var LayuiData = new
            {
                code = 0,
                msg = "",
                data = data,
            };
            return LayuiData;
        } 
    }
}

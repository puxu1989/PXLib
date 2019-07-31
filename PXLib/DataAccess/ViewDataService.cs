using Newtonsoft.Json.Linq;
using PXLib.Helpers;
using PXLib.WebControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PXLib;
using PXLib.DataAccess.DbFactory;
using System.Linq.Expressions;
using System.Data.Common;

namespace PXLib.DataAccess
{
    public class ViewDataService<T> : RepositoryFactory<T> where T : class, new()
    {
        public  IEnumerable<T> GetPageList(PagingBase pagination, Expression<Func<T, bool>> expression)
        {
            return this.BaseRepository().FindList(expression, pagination);
        }
        public IEnumerable<T> GetPageList(string sql, PagingBase pagination)
        {
            return this.BaseRepository().FindList(sql, pagination);
        }
        public IEnumerable<T> GetList(Expression<Func<T, bool>> condition)
        {
            return this.BaseRepository().FindList(condition);
        }
        public IEnumerable<T> GetList(string sqlStr, params DbParameter[] parameter)
        {
            return this.BaseRepository().FindList(sqlStr, parameter);
        }
        public T FindEntity(string sqlStr, params DbParameter[] parameter)
        {
            return this.BaseRepository().FindList(sqlStr, parameter).FirstOrDefault();
        }
        public T FindEntity(Expression<Func<T, bool>> condition)
        {
            return this.BaseRepository().FindEntity(condition);
        }
    }
}
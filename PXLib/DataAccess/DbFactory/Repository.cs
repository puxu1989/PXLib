using PXLib.DataAccess.Database;
using PXLib.WebControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.DataAccess.DbFactory
{
    /// <summary>
    /// 数据存储仓库接口
    /// </summary>
    public interface IRepository
    {
        IRepository BeginTrans();

        void Commit();

        void Rollback();

        int Insert<T>(T entity) where T : class;

        int Insert<T>(List<T> entitys) where T : class;


        int Delete<T>(T entity) where T : class;

        int Delete<T>(List<T> entity) where T : class;

        int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new();

        int Delete<T>(object keyValue) where T : class;

        int Delete<T>(object[] keyValue) where T : class;

        bool Delete<T>(string propertyName, object propertyValue) where T : class;
        bool Delete(string tableName, string propertyName, object propertyValue);
        int Update<T>(T entity) where T : class;
        int Update<T>(T entity, string pkName) where T : class;
        int Update<T>(List<T> entities) where T : class;

        int Update<T>(Hashtable ht, Expression<Func<T, bool>> condition) where T : class, new();

        T FindEntity<T>(object keyValue) where T : class;

        T FindEntity<T>(Expression<Func<T, bool>> condition) where T : class, new();

        IQueryable<T> IQueryable<T>() where T : class, new();

        IQueryable<T> IQueryable<T>(Expression<Func<T, bool>> condition) where T : class, new();

        IEnumerable<T> FindList<T>(string where, string colNameSort) where T : class, new();
        IEnumerable<T> FindList<T>(string strSql, DbParameter[] dbParameter) where T : class, new();
        IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition) where T : class, new();
        IEnumerable<T> FindList<T>(PagingBase pagination) where T : class, new();
        IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition, PagingBase pagination) where T : class, new();
        IEnumerable<T> FindList<T>(string strSql, PagingBase pagination) where T : class;
        IEnumerable<T> FindList<T>(string strSql, DbParameter[] dbParameter, PagingBase pagination) where T : class;

        List<T> FindListTop<T>(int top) where T : new();
        List<T> FindListTop<T>(int top, string propertyName, object propertyValue) where T : new();
        List<T> FindListTop<T>(int top, string WhereOrderBySql, params DbParameter[] parameters) where T : new();

        int FindCount<T>() where T : new();

        int FindCount<T>(Expression<Func<T, bool>> condition) where T : new();

       DataTable FindTable(string strSql);
        DataTable FindTable(string strSql, DbParameter[] dbParameter);
        DataTable FindTable(string strSql, PagingBase pagination);
        DataTable FindTable(string strSql, DbParameter[] dbParameter, PagingBase pagination);

        object FindObject(string strSql);
        object FindObject(string strSql, DbParameter[] dbParameter);

        bool Exists<T>(object keyValue) where T : class;
        bool Exists<T>(Expression<Func<T, bool>> condition) where T : class;
        object GetMax<T>(string pkName) where T : class;
        int GetMaxCode<T>(string pkName) where T : class;
        DataTable GetDataTablePageList(string tableName, string colNames, int pageIndex, int pageSize, string where, string colFieldAndSortType, ref int recordCount);
        int ExecuteNonQuery(string sql);
    }
    public class Repository : IRepository
    {
        public IDatabase db;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="idatabase">数据库</param>
        public Repository(IDatabase idatabase)
        {
            this.db = idatabase;
            
        }

        public IRepository BeginTrans()
        {
            this.db.BeginTrans();
            return this;
        }

        public void Commit()
        {
            this.db.Commit();
        }

        public void Rollback()
        {
            this.db.Rollback();
        }

        public int Insert<T>(T entity) where T : class
        {
            return this.db.Insert<T>(entity);
        }

        public int Insert<T>(List<T> entitys) where T : class
        {
            return this.db.Insert<T>(entitys);
        }
        public int Delete<T>(T entity) where T : class
        {
            return this.db.Delete<T>(entity);
        }

        public int Delete<T>(List<T> entity) where T : class
        {
            return this.db.Delete<T>(entity);
        }

        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            return this.db.Delete<T>(condition);
        }

        public int Delete<T>(object keyValue) where T : class
        {
            return this.db.Delete<T>(keyValue);
        }

        public int Delete<T>(object[] keyValue) where T : class
        {
            return this.db.Delete<T>(keyValue);
        }

        public bool Delete<T>(string propertyName,object propertyValue) where T : class
        {
            return this.db.Delete<T>(propertyName,propertyValue);
        }
        public bool Delete(string tableName, string propertyName, object propertyValue)
        {
            return this.db.Delete(tableName,propertyName, propertyValue);
        }
        public int Update<T>(T entity) where T : class
        {
            return this.db.Update<T>(entity);
        }

        public int Update<T>(T entity, string pkName) where T : class
        {
            return this.db.Update<T>(entity,pkName);
        }

        public int Update<T>(List<T> entities) where T : class
        {
            return this.db.Update<T>(entities);
        }
      

        public int Update<T>(Hashtable ht,Expression<Func<T, bool>> condition) where T : class, new()
        {
            return this.db.Update<T>(ht,condition);
        }

        public T FindEntity<T>(object keyValue) where T : class
        {
            return this.db.FindEntity<T>(keyValue);
        }

        public T FindEntity<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            return this.db.FindEntity<T>(condition);
        }

        public IQueryable<T> IQueryable<T>() where T : class, new()
        {
            return this.db.IQueryable<T>();
        }

        public IQueryable<T> IQueryable<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            return this.db.IQueryable<T>(condition);
        }


        //???????????????????????????????
        public IEnumerable<T> FindList<T>(string where, string colNameSort) where T : class, new()
        {
            return this.db.FindList<T>(where,colNameSort);
        }
        public IEnumerable<T> FindList<T>(string strSql, DbParameter[] dbParameter) where T : class, new() 
        {
            return this.db.FindList<T>(strSql, dbParameter);
        }
        public IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            return this.db.FindList<T>(condition);
        }
        public IEnumerable<T> FindList<T>(PagingBase pagination) where T : class, new()
        {
            int records = pagination.RecordCount;
            IEnumerable<T> result = this.db.FindList<T>(pagination.PageIndex, pagination.PageSize, pagination.SortField, pagination.SortType.ToLower() == "asc",out records);
            pagination.RecordCount = records;
            return result;
        }

        public IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition, PagingBase pagination) where T : class, new()
        {
            int records = pagination.RecordCount;
            IEnumerable<T> result = this.db.FindList<T>(condition, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }

        public IEnumerable<T> FindList<T>(string strSql, PagingBase pagination) where T : class
        {
            int records = pagination.RecordCount;
            IEnumerable<T> result = this.db.FindList<T>(strSql, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }

        public IEnumerable<T> FindList<T>(string strSql, DbParameter[] dbParameter, PagingBase pagination) where T : class
        {
            int records = pagination.RecordCount;
            IEnumerable<T> result = this.db.FindList<T>(strSql, dbParameter, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }

        public List<T> FindListTop<T>(int top) where T : new()
        {
           return this.db.FindListTop<T>(top);
        }

        public List<T> FindListTop<T>(int top, string propertyName, object propertyValue) where T : new()
        {
            return this.db.FindListTop<T>(top,propertyName, propertyValue);
        }
        public List<T> FindListTop<T>(int top, string WhereOrderBySql, params DbParameter[] parameters) where T : new()
        {
            return this.db.FindListTop<T>(top, WhereOrderBySql, parameters);
        }


        public int FindCount<T>() where T : new()
        {
           return this.db.FindCount<T>();
        }
        public int FindCount<T>(Expression<Func<T, bool>> condition) where T : new()
        {
           return  this.db.FindCount<T>(condition);
        }

        public DataTable FindTable(string strSql)
        {
            return this.db.FindTable(strSql);
        }

        public DataTable FindTable(string strSql, DbParameter[] dbParameter)
        {
            return this.db.FindTable(strSql, dbParameter);
        }

        public DataTable FindTable(string strSql, PagingBase pagination)
        {
            int records = 0;
            DataTable result = this.db.FindTable(strSql, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }

        public DataTable FindTable(string strSql, DbParameter[] dbParameter, PagingBase pagination)
        {
            int records = 0;
            DataTable result = this.db.FindTable(strSql, dbParameter, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }
  

        public object FindObject(string strSql)//调用ExecuteScalar
        {
            return this.db.FindObject(strSql);
        }

        public object FindObject(string strSql, DbParameter[] dbParameter)
        {
            return this.db.FindObject(strSql, dbParameter);
        }

        public bool Exists<T>(object pkValue) where T : class
        {
            return this.db.Exists<T>(pkValue);
        }
        public bool Exists<T>(Expression<Func<T, bool>> condition) where T : class 
        {
            return this.db.Exists<T>(condition);
        }
        public object GetMax<T>(string pkName) where T : class
        {
            return this.db.GetMax<T>(pkName);
        }

        public int GetMaxCode<T>(string pkName) where T : class
        {
            return this.db.GetMaxCode<T>(pkName);
        }
        //数据分页 新加 20180428 
        public DataTable GetDataTablePageList(string tableName, string colNames, int pageIndex, int pageSize, string where, string colFieldAndSortType, ref int recordCount)// //按名称查询分页
        {
            return this.db.GetDataTablePageList(tableName, colNames, pageIndex, pageSize, where, colFieldAndSortType, ref recordCount);
        }
        public int ExecuteNonQuery(string sql)
        {
            return this.db.ExecuteNonQuery(sql);
        }
    }
}

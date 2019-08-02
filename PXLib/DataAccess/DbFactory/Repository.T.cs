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
    /// Repository泛型接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : class, new()
    {
        IRepository<T> BeginTrans();

        void Commit();

        void Rollback();

        #region 插入2个
        int Insert(T entity);

        int Insert(List<T> entity);
        #endregion 
        #region 删除6个
        int Delete(T entity);

        int Delete(List<T> entity);

        int Delete(Expression<Func<T, bool>> condition);

        int Delete(object keyValue);

        int Delete(object[] keyValue);

        bool Delete(string propertyName, object propertyValue);
        bool Delete(string tableName, string propertyName, object propertyValue);
        #endregion
        #region 更新4个
        int Update(T entity);

        int Update(List<T> entity);
        /// <summary>
        /// 更新指定条件列 entity必须包含此列
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pkName"></param>
        /// <returns></returns>
        int Update(T entity, string pkName);
        int Update(Hashtable ht, Expression<Func<T, bool>> condition);
        #endregion
        T FindEntity(object keyValue);

        T FindEntity(Expression<Func<T, bool>> condition);


        IEnumerable<T> FindList(string where, string colNameSort);
        IEnumerable<T> FindList(string strSql, params DbParameter[] dbParameter);
        IEnumerable<T> FindList(PagingBase pagination);
        IEnumerable<T> FindList(Expression<Func<T, bool>> condition);
        IEnumerable<T> FindList(Expression<Func<T, bool>> condition, PagingBase pagination);

        IEnumerable<T> FindList(string strSql, PagingBase pagination);

        IEnumerable<T> FindList(string strSql, DbParameter[] dbParameter, PagingBase pagination);


        List<T> FindListTop(int top);
        List<T> FindListTop(int top, string propertyName, object propertyValue);
        List<T> FindListTop(int top, string WhereOrderBySql, params DbParameter[] parameters);
        int FindCount();
        int FindCount(Expression<Func<T, bool>> condition);

        IQueryable<T> IQueryable();

        IQueryable<T> IQueryable(Expression<Func<T, bool>> condition);
        DataTable FindTable(string strSql);

        DataTable FindTable(string strSql, DbParameter[] dbParameter);

        DataTable FindTable(string strSql, PagingBase pagination);

        DataTable FindTable(string strSql, DbParameter[] dbParameter, PagingBase pagination);
        DataTable FindTableByProc(string procName, params DbParameter[] dbParamete);
        object FindObject(string strSql);

        object FindObject(string strSql, params DbParameter[] dbParameter);

        bool Exists(object keyValue);
        bool Exists(Expression<Func<T, bool>> condition);
        /// <summary>
        /// 获取列的最大值
        /// </summary>
        /// <param name="pkName"></param>
        /// <returns></returns>
        object GetMax(string pkName);
        /// <summary>
        /// 获取列的最编号（已经+1了）
        /// </summary>
        /// <param name="pkName"></param>
        /// <returns></returns>
        int GetMaxCode(string pkName);

        int ExecuteNonQuery(string sql);
        /// <summary>
        /// 执行存储过程 此过程必须有OUT类型的Out_Res参数
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="dbParameter"></param>
        /// <returns></returns>
        object ExecuteByProc(string procName, params DbParameter[] dbParameter);
    }
    /// <summary>
    /// Factory T 调用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        public IDatabase db;

        public Repository(IDatabase idatabase)
        {
            this.db = idatabase;
        }

        public IRepository<T> BeginTrans()
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

        public int Insert(T entity)
        {
            return this.db.Insert<T>(entity);
        }

        public int Insert(List<T> listEntitys)
        {
            return this.db.Insert<T>(listEntitys);
        }
        public int Delete(T entity)
        {
            return this.db.Delete<T>(entity);
        }

        public int Delete(List<T> entity)
        {
            return this.db.Delete<T>(entity);
        }

        public int Delete(Expression<Func<T, bool>> condition)
        {
            return this.db.Delete<T>(condition);
        }

        public int Delete(object keyValue)
        {
            return this.db.Delete<T>(keyValue);
        }

        public int Delete(object[] keyValue)
        {
            return this.db.Delete<T>(keyValue);
        }

        public bool Delete(string propertyName, object propertyValue)
        {
            return this.db.Delete<T>(propertyName, propertyValue);
        }
        public bool Delete(string tableName, string propertyName, object propertyValue)
        {
            return this.db.Delete(tableName, propertyName, propertyValue);
        }

        public int Update(T entity)
        {
            return this.db.Update<T>(entity);
        }

        public int Update(List<T> entity)
        {
            return this.db.Update<T>(entity);
        }
        public int Update(T entity, string pkName)
        {
            return this.db.Update<T>(entity, pkName);
        }
        public int Update(Hashtable ht, Expression<Func<T, bool>> condition)
        {
            return this.db.Update<T>(ht, condition);
        }

        public T FindEntity(object keyValue)
        {
            return this.db.FindEntity<T>(keyValue);
        }

        public T FindEntity(Expression<Func<T, bool>> condition)
        {
            return this.db.FindEntity<T>(condition);
        }

        public IQueryable<T> IQueryable()
        {
            return this.db.IQueryable<T>();
        }

        public IQueryable<T> IQueryable(Expression<Func<T, bool>> condition)
        {
            return this.db.IQueryable<T>(condition);
        }
        public IEnumerable<T> FindList(string where, string colNameSort)
        {
            return this.db.FindList<T>(where, colNameSort);
        }
        public IEnumerable<T> FindList(string strSql, params DbParameter[] dbParameter)
        {
            return this.db.FindList<T>(strSql, dbParameter);
        }
        public IEnumerable<T> FindList(Expression<Func<T, bool>> condition)
        {
            return this.db.FindList<T>(condition);
        }
        public IEnumerable<T> FindList(PagingBase pagination)
        {
            int records = 0;
            IEnumerable<T> result = this.db.FindList<T>(pagination.PageIndex, pagination.PageSize, pagination.SortField, pagination.SortType.ToLower() == "asc", out records);
            pagination.RecordCount = records;
            return result;
        }

        public IEnumerable<T> FindList(Expression<Func<T, bool>> condition, PagingBase pagination)
        {
            int records = 0;
            IEnumerable<T> result = this.db.FindList<T>(condition, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }

        public IEnumerable<T> FindList(string strSql, PagingBase pagination)
        {
            int records = 0;
            IEnumerable<T> result = this.db.FindList<T>(strSql, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }

        public IEnumerable<T> FindList(string strSql, DbParameter[] dbParameter, PagingBase pagination)
        {
            int records = 0; ;
            IEnumerable<T> result = this.db.FindList<T>(strSql, dbParameter, pagination.SortField, pagination.SortType.ToLower() == "asc", pagination.PageSize, pagination.PageIndex, out records);
            pagination.RecordCount = records;
            return result;
        }

        public List<T> FindListTop(int top)
        {
            return this.db.FindListTop<T>(top);
        }

        public List<T> FindListTop(int top, string propertyName, object propertyValue)
        {
            return this.db.FindListTop<T>(top, propertyName, propertyValue);
        }

        public List<T> FindListTop(int top, string WhereOrderBySql, params DbParameter[] parameters)
        {
            return this.db.FindListTop<T>(top, WhereOrderBySql, parameters);
        }

        public int FindCount()
        {
            return this.db.FindCount<T>();
        }
        public int FindCount(Expression<Func<T, bool>> condition)
        {
            return this.db.FindCount(condition);
        }

        public DataTable FindTable(string strSql)
        {
            return this.db.FindTable(strSql);
        }

        public DataTable FindTable(string strSql, DbParameter[] dbParameter)
        {
            return this.db.FindTable(strSql, dbParameter);
        }
        public DataTable FindTableByProc(string prcoName,params DbParameter[] dbParameter)
        {
            return this.db.FindTableByProc(prcoName, dbParameter);
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

        public object FindObject(string strSql)
        {
            return this.db.FindObject(strSql);
        }

        public object FindObject(string strSql, params DbParameter[] dbParameter)
        {
            return this.db.FindObject(strSql, dbParameter);
        }

        public bool Exists(object keyValue)
        {
            return this.db.Exists<T>(keyValue);
        }

        public bool Exists(Expression<Func<T, bool>> condition)
        {
            return this.db.Exists<T>(condition);
        }
        public object GetMax(string pkName)
        {
            return this.db.GetMax<T>(pkName);
        }
        public int GetMaxCode(string pkName)
        {
            return this.db.GetMaxCode<T>(pkName);
        }
     
        public int ExecuteNonQuery(string sql)
        {
            return this.db.ExecuteNonQuery(sql);
        }
        public object ExecuteByProc(string procName, params DbParameter[] dbParameter)
        {
            return this.db.ExecuteByProc(procName, dbParameter);
        }
    }
}

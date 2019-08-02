using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PXLib.DataAccess.Database
{
    /// <summary>
    /// 数据库操作接口 MySql SqlServer Oracle各自实现该接口 2017.6.30
    /// </summary>
    public interface IDatabase:IDisposable
    {
        IDatabase BeginTrans();
        int Commit();
        void Rollback();
        void Close();
        int ExecuteNonQuery(string cmdText, params DbParameter[] dbParameter);
        object ExecuteScalar(string cmdText, params DbParameter[] parameters);
        object ExecuteByProc(string procName, DbParameter[] dbParameter);
        int Insert<T>(T entity) where T : class;
        int Insert<T>(IEnumerable<T> entities) where T : class;
        int Delete<T>(T entity) where T : class;
        int Delete<T>(IEnumerable<T> entities) where T : class;
        int Delete<T>(Expression<Func<T, bool>> condition) where T : class,new();
        int Delete<T>(object KeyValue) where T : class;
        int Delete<T>(object[] KeyValue) where T : class;
        bool Delete<T>(string propertyName, object propertyValue) where T : class;
        bool Delete(string tableName, string propertyName, object propertyValue);
        int Update<T>(T entity) where T : class;
        int Update<T>(T entity,string pkName) where T : class;
        int Update<T>(IEnumerable<T> entities) where T : class;
        int Update<T>(Hashtable ht,Expression<Func<T, bool>> condition) where T : class,new();

        IQueryable<T> IQueryable<T>() where T : class,new();
        IQueryable<T> IQueryable<T>(Expression<Func<T, bool>> condition) where T : class,new();

        T FindEntity<T>(object KeyValue) where T : class;
        T FindEntity<T>(Expression<Func<T, bool>> condition) where T : class,new();


        IEnumerable<T> FindList<T>(string where, string colNameSort) where T : class,new();
        IEnumerable<T> FindList<T>(string strSql,params DbParameter[] dbParameter) where T : class;


        IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition) where T : class,new();
        IEnumerable<T> FindList<T>(int pageIndex,int pageSize,string orderField, bool isAsc,  out int total) where T : class,new();
        IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class,new();
        IEnumerable<T> FindList<T>(string strSql, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class;
        IEnumerable<T> FindList<T>(string strSql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class;


        List<T> FindListTop<T>(int top) where T : new();
        List<T> FindListTop<T>(int top, string propertyName, object propertyValue) where T : new();
        List<T> FindListTop<T>(int top, string WhereOrderBySql, params DbParameter[] parameters) where T : new();

        int FindCount<T>() where T : new();
        int FindCount<T>(Expression<Func<T, bool>> condition) where T : new();
        DataTable FindTable(string strSql);
        DataTable FindTable(string strSql, DbParameter[] dbParameter);
        DataTable FindTable(string strSql, string orderField, bool isAsc, int pageSize, int pageIndex, out int total);
        DataTable FindTable(string strSql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex, out int total);
        DataTable FindTableByProc(string procName, params DbParameter[] parameters);
        object FindObject(string strSql);
        object FindObject(string strSql, DbParameter[] dbParameter);


        bool Exists<T>(object keyValue) where T : class;
        bool Exists<T>(Expression<Func<T, bool>> condition) where T : class;
        int GetMaxCode<T>(string pkName) where T : class;
        object GetMax<T>(string pkName) where T : class;
        DataTable GetDataTablePageList(string tableName, string colNames, int pageIndex, int pageSize, string where, string colFieldAndSortType, ref int count);
    }
}

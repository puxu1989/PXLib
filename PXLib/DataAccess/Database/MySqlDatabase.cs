using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using PXLib.Exceptions;
using PXLib.DataAccess.Database.Extension;
using PXLib.Attributes;
using System.Collections;
using PXLib.Helpers;

namespace PXLib.DataAccess.Database
{
    public class MySqlDatabase : IDatabase
    {
        #region 属性
        /// <summary>
        /// 获取 数据库连接串
        /// </summary>
        private static string connectionStringName;

        /// <summary>
        /// 连接对象
        /// </summary>
        //private static IDbConnection _connection; 静态有莫名其妙的问题,不能并发操作？
        protected IDbConnection dbConnection
        {
            get
            {
                //if (_connection == null)
                //{       
                //    IDbConnection dbconnection = DatabaseCommon.CreateDbConnection(connectionStringName);
                //    _connection = dbconnection;
                //}
                //return _connection;
                return DatabaseCommon.CreateDbConnection(connectionStringName);
            }
        }
        /// <summary>
        /// 事务对象
        /// </summary>
        public IDbTransaction dbTransaction { get; set; }
        #endregion
        /// <summary>
        /// 构造方法
        /// </summary>
        public MySqlDatabase(string connStringName)
        {
            connectionStringName = connStringName;
            IDbConnection connection = this.dbConnection;//赋值一次 获取连接类型 dbChar 数据库类型等
        }
        #region 事物提交
        /// <summary>
        /// 事务开始 Mysql请使用支持事务的Innodb引擎
        /// </summary>
        /// <returns></returns>
        public IDatabase BeginTrans()
        {
            IDbConnection connection = this.dbConnection;//赋值一次
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            dbTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
            return this;
        }
        /// <summary>
        /// 提交当前操作的结果
        /// </summary>
        public int Commit()
        {
            int result;
            try
            {
                if (this.dbTransaction != null)
                {
                    this.dbTransaction.Commit();
                    this.Close();
                }
                result = 1;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.InnerException is SqlException)
                {
                    SqlException ex2 = ex.InnerException.InnerException as SqlException;
                    string sqlExceptionMessage = ExceptionMessage.GetSqlExceptionMessage(ex2.Number);
                    throw DataAccessException.ThrowDataAccessException(ex2, sqlExceptionMessage);
                }
                throw;
            }
            finally
            {
                if (this.dbTransaction == null)
                {
                    this.Close();
                }
            }
            return result;
        }
        /// <summary>
        /// 把当前操作回滚成未提交状态
        /// </summary>
        public void Rollback()
        {
            this.dbTransaction.Rollback();
            this.dbTransaction.Dispose();
            this.Close();
        }
        /// <summary>
        /// 关闭连接 内存回收
        /// </summary>
        public void Close()
        {
            IDbConnection connection = this.dbConnection;
            if (connection != null && connection.State != ConnectionState.Closed)
            {
                connection.Close();
                connection.Dispose();
            }
            this.dbTransaction = null;

        }
        /// <summary>
        /// 内存回收
        /// </summary>
        public void Dispose()
        {
            if (this.dbConnection != null)
            {
                this.dbConnection.Dispose();
            }
            if (this.dbTransaction != null)
            {
                this.dbTransaction.Dispose();
            }
        }

        #endregion

        #region 基本操作
        /// <summary>
        /// 返回影响的行数
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText, params DbParameter[] parameters)
        {
            if (dbTransaction == null)
            {
                return DatabaseCommon.ExecuteNonQuery(this.dbConnection, cmdText, parameters);
            }
            else
            {
                return DatabaseCommon.ExecuteNonQueryTran(this.dbTransaction, cmdText, parameters);
            }
        }

        public object ExecuteScalar(string cmdText, params DbParameter[] parameters)
        {
            return DatabaseCommon.ExecuteScalar(this.dbConnection, cmdText, parameters);
        }
        #region 存储过程
        /// <summary>
        /// 执行存储过程 返回Output_Value为Out参数的执行结果
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteByProc(string procName, params DbParameter[] parameters) 
        {
            return DatabaseCommon.ExecuteByProc(this.dbConnection, procName, parameters,out int s);
        }
        #endregion
        #endregion

        #region 添加 2 个
        public int Insert<T>(T entity) where T : class
        {
            return ExecuteNonQuery(DatabaseCommon.InsertEntitySql<T>(entity).ToString(), DatabaseCommon.GetParameter<T>(entity));
        }
        public int Insert<T>(IEnumerable<T> entities) where T : class//批量插入 应使用事务 
        {

            if (dbTransaction == null)
            {
                BeginTrans();
                foreach (var item in entities)
                {
                    Insert<T>(item);
                }
                return Commit();
            }
            else
            {
                int i = 0;
                foreach (var item in entities)
                {
                    i += Insert<T>(item);
                }
                return i;
            }
        }
        #endregion
        #region 删除 5个
        public int Delete<T>(T entity) where T : class
        {
            return ExecuteNonQuery(DatabaseCommon.DeleteEntitySql<T>(entity).ToString(), DatabaseCommon.GetParameter<T>(entity));
        }
        public int Delete<T>(IEnumerable<T> entities) where T : class
        {
            if (dbTransaction == null)
            {
                BeginTrans();
                foreach (var item in entities)
                {
                    Delete<T>(item);
                }
                return Commit();
            }
            else
            {
                int i = 0;
                foreach (var item in entities)
                {
                    i += Delete<T>(item);
                }
                return i;
            }

        }
        public int Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            string sql = $"Delete  From {typeof(T).GetMappingAttributeName()} Where {ExpressionToSql.GetWhereByLambda<T>(condition)}";
            return ExecuteNonQuery(sql);
        }
        public int Delete<T>(object pkValue) where T : class
        {
            ValidationHelper.ArgumentNotNull(pkValue, "删除主键");
            string pkName = AttributeHelper.GetEntityPrimaryKey<T>();
            StringBuilder sb = new StringBuilder($"Delete From {typeof(T).GetMappingAttributeName()} Where {pkName} = ?{pkName}");
            int i= ExecuteNonQuery(sb.ToString(), new MySqlParameter(pkName, pkValue));
            return i;
        }
        public int Delete<T>(object[] keyValue) where T : class
        {
            if (dbTransaction == null)
            {
                BeginTrans();
                foreach (var item in keyValue)
                {
                    Delete<T>(item);
                }
                return Commit();
            }
            else
            {
                int i = 0;
                foreach (var item in keyValue)
                {
                    i += Delete<T>(item);
                }
                return i;
            }
        }
        public bool Delete<T>(string propertyName, object propertyValue) where T : class
        {
            if (string.IsNullOrEmpty(propertyValue.ToString()))
                return false;
            StringBuilder sb = new StringBuilder($"Delete From {typeof(T).GetMappingAttributeName()} Where {propertyName} = ?{propertyValue}");
            return ExecuteNonQuery(sb.ToString(), new MySqlParameter(propertyName, propertyValue)) > 0 ? true : false;
        }
        public bool Delete(string tableName, string propertyName, object propertyValue)
        {
            bool flag = string.IsNullOrEmpty(propertyValue.ToString());
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder(string.Concat(new string[]
                {
                    "Delete From ",
                    tableName,
                    " Where ",
                    propertyName,
                    " = ?",
                    propertyName
                }) ?? "");
                result = (this.ExecuteNonQuery(stringBuilder.ToString(), new DbParameter[]
                {
                    new MySqlParameter(propertyName, propertyValue)
                }) > 0);
            }
            return result;
        }

        #endregion
        #region 更新 4 个
        public int Update<T>(T entity) where T : class
        {
            return ExecuteNonQuery(DatabaseCommon.UpdateEntitySql<T>(entity).ToString(), DatabaseCommon.GetParameter<T>(entity));
        }
        public int Update<T>(T entity, string pkName) where T : class
        {
            return ExecuteNonQuery(DatabaseCommon.UpdateSql<T>(entity, pkName).ToString(), DatabaseCommon.GetParameter<T>(entity));
        }
        public int Update<T>(IEnumerable<T> entities) where T : class
        {
            if (dbTransaction == null)
            {
                BeginTrans();
                foreach (var item in entities)
                {
                    Update<T>(item);
                }
                return Commit();
            }
            else
            {
                int i = 0;
                foreach (var item in entities)
                {
                    i += Update<T>(item);
                }
                return i;
            }
        }

        public int Update<T>(Hashtable ht, Expression<Func<T, bool>> condition) where T : class, new()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($" Update {typeof(T).GetMappingAttributeName()} SET");
            bool isFirstValue = true;
            foreach (string key in ht.Keys)
            {
                if (ht[key] != null)
                {
                    if (isFirstValue)
                    {
                        isFirstValue = false;
                        sb.Append($"{key}=?{key}");
                    }
                    else
                    {
                        sb.Append($",{key}=?{key}");
                    }
                }
            }
            sb.Append(" Where ").Append(ExpressionToSql.GetWhereByLambda(condition));
            return ExecuteNonQuery(sb.ToString(), DatabaseCommon.GetParameter(ht));
        }
        #endregion

        #region 对象实体 查询
        #region 单个实体
        public T FindEntity<T>(object keyValue) where T : class
        {
            ValidationHelper.ArgumentNotNull(keyValue, "FindEntity:查询关键字");
            StringBuilder sb = new StringBuilder();
            sb.Append($"SELECT * FROM {typeof(T).GetMappingAttributeName()} WHERE ").Append(AttributeHelper.GetEntityPrimaryKey<T>()).Append("=?ID");
            return FindList<T>(sb.ToString(), new MySqlParameter("?ID", keyValue)).FirstOrDefault();

            //T model = Activator.CreateInstance<T>();
            //DataTable dt = FindTable(sb.ToString(), new MySqlParameter("?ID", keyValue));
            //if (dt != null && dt.Rows.Count > 0)
            //    return DataConvertHelper.DataRowToModel<T>(dt.Rows[0]);
            //return model;
        }
        //条件查询单个实体
        public T FindEntity<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            return this.FindList<T>(condition).FirstOrDefault();
        }
        #endregion
        #region 列表实体查询
        //查询where条件的列表数据
        public IEnumerable<T> FindList<T>(string where, string colNameSort) where T : class, new()
        {
            Type type = typeof(T);
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT * FROM {0} WHERE 1=1 ",type.GetMappingAttributeName());
            if (!string.IsNullOrEmpty(where))
                strSql.Append(where);
            if (!string.IsNullOrEmpty(colNameSort))
                strSql.AppendFormat(" Order BY {0}", colNameSort);

            return ConvertExtension.IDataReaderToList<T>(DatabaseCommon.ExecuteDataReader(this.dbConnection, strSql.ToString(), null));

        }
        /// <summary>
        /// Sql语句查询
        /// </summary>
        public IEnumerable<T> FindList<T>(string strSql, params DbParameter[] dbParameter) where T : class
        {
            return ConvertExtension.IDataReaderToList<T>(DatabaseCommon.ExecuteDataReader(this.dbConnection, strSql, dbParameter));
        }
        //条件查询实体
        public IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT * From {0} Where {1} ", typeof(T).GetMappingAttributeName(), ExpressionToSql.GetWhereByLambda(condition));
            DbParameter[] dbParameter = null;
            return this.FindList<T>(sb.ToString(), dbParameter);
        }
        //分页List查询 
        public IEnumerable<T> FindList<T>(int pageIndex, int pageSize, string orderField, bool isAsc, out int totalCount) where T : class, new()
        {
            StringBuilder sb = new StringBuilder();
            string strSql = string.Format("SELECT * From {0} ", typeof(T).GetMappingAttributeName());
            sb.AppendFormat(strSql);
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            int startNum = (pageIndex - 1) * pageSize;
            string OrderBy = "";
            if (!orderField.IsNullEmpty())
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    OrderBy = " Order By " + orderField;
                }
                else
                {
                    OrderBy = " Order By " + orderField + " " + (isAsc ? "ASC" : "DESC");
                }
            }
            else
            {
                OrderBy = " order by (select 0 )";
            }
            sb.Append(OrderBy);
            sb.AppendFormat(" limit {0},{1} ", startNum, pageSize);
            totalCount = this.FindObject("Select Count(1) From (" + strSql + ") As ttt").ToInt();
            return ConvertExtension.IDataReaderToList<T>(DatabaseCommon.ExecuteDataReader(this.dbConnection, sb.ToString()));

        }
        public IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class, new()
        {
            StringBuilder sb = new StringBuilder();
            string strSql = string.Format("SELECT * From {0} Where {1} ", typeof(T).GetMappingAttributeName(), ExpressionToSql.GetWhereByLambda(condition));//test1
            sb.AppendFormat(strSql);

            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            int num = (pageIndex - 1) * pageSize;
            int num1 = (pageIndex) * pageSize;
            string OrderBy = "";

            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    OrderBy = " Order By " + orderField;
                }
                else
                {
                    OrderBy = " Order By " + orderField + " " + (isAsc ? "ASC" : "DESC");
                }
            }
            else
            {
                OrderBy = " order by (select 0 )";
            }
            sb.Append(OrderBy);
            sb.Append(" limit " + num + "," + pageSize + "");
            total = Convert.ToInt32(this.FindObject("Select Count(1) From (" + strSql + ") As ttt"));
            return ConvertExtension.IDataReaderToList<T>(DatabaseCommon.ExecuteDataReader(this.dbConnection, sb.ToString()));

        }
        public IEnumerable<T> FindList<T>(string strSql, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class
        {
            return FindList<T>(strSql, null, orderField, isAsc, pageSize, pageIndex, out total);
        }
        public IEnumerable<T> FindList<T>(string strSql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class
        {

            StringBuilder sb = new StringBuilder();
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            int num = (pageIndex - 1) * pageSize;
            int num1 = (pageIndex) * pageSize;
            string OrderBy = "";

            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    OrderBy = " Order By " + orderField;
                }
                else
                {
                    OrderBy = " Order By " + orderField + " " + (isAsc ? "ASC" : "DESC");
                }
            }
            else
            {
                OrderBy = " order by (select 0 )";
            }
            sb.Append(strSql + OrderBy);
            sb.Append(" limit " + num + "," + pageSize + "");
            total = Convert.ToInt32(this.FindObject("Select Count(1) From (" + strSql + ") As ttt", dbParameter));
            var IDataReader = DatabaseCommon.ExecuteDataReader(this.dbConnection, sb.ToString(), dbParameter);
            return ConvertExtension.IDataReaderToList<T>(IDataReader);

        }

        public List<T> FindListTop<T>(int top) where T : new()
        {
            StringBuilder strSql = DatabaseCommon.SelectTopSql<T>(top);
            IDataReader dr = DatabaseCommon.ExecuteDataReader(this.dbConnection, strSql.ToString());
            return ConvertExtension.IDataReaderToList<T>(dr);
        }

        public List<T> FindListTop<T>(int top, string propertyName, object propertyValue) where T : new()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT * FROM {0} WHERE 1=1", typeof(T).GetMappingAttributeName());
            strSql.Append(" AND " + propertyName + " = " + DatabaseCommon.DbParmCharacter + propertyName);
            strSql.Append(" limit " + top);
            IDataReader dr = DatabaseCommon.ExecuteDataReader(this.dbConnection, strSql.ToString(), DbParameters.CreateDbParameter(propertyName, propertyValue));
            return ConvertExtension.IDataReaderToList<T>(dr);
        }

        public List<T> FindListTop<T>(int top, string WhereOrderSql, params DbParameter[] parameters) where T : new()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT * FROM {0} ", typeof(T).GetMappingAttributeName());
            strSql.Append("WHERE True " + WhereOrderSql);
            strSql.Append(" limit " + top);
            IDataReader dr = DatabaseCommon.ExecuteDataReader(this.dbConnection, strSql.ToString(), parameters);
            return ConvertExtension.IDataReaderToList<T>(dr);
        }

        public int FindCount<T>() where T : new()
        {
            string strSql = "Select Count(1) From {0} As ttt".FormatWith(typeof(T).GetMappingAttributeName());
            return this.ExecuteScalar(strSql).ToInt();
        }

        public int FindCount<T>(Expression<Func<T, bool>> condition) where T : new()
        {
            string strSql = "Select Count(1) From {0}  where {1}".FormatWith(typeof(T).GetMappingAttributeName(), ExpressionToSql.GetWhereByLambda(condition));
            return this.ExecuteScalar(strSql).ToInt();
        }


        /// <summary>
        /// 未实现 报错  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> IQueryable<T>() where T : class, new()
        {
            throw new NotImplementedException();//EF尽量使用IQueryable 根据条件生成sql
        }
        /// <summary>
        /// 未实现 报错
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public IQueryable<T> IQueryable<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {

            throw new NotImplementedException();

        }
        #endregion

        #region 数据源查询
        public DataTable FindTable(string strSql)
        {
            return FindTable(strSql, null);
        }
        public DataTable FindTable(string strSql, params DbParameter[] dbParameter)
        {
            var IDataReader = DatabaseCommon.ExecuteDataReader(this.dbConnection, strSql, dbParameter);
            return ConvertExtension.IDataReaderToDataTable(IDataReader);
        }
        public DataTable FindTable(string strSql, string orderField, bool isAsc, int pageSize, int pageIndex, out int total)
        {
            return FindTable(strSql, null, orderField, isAsc, pageSize, pageIndex, out total);
        }
        public DataTable FindTableByProc(string procName, params DbParameter[] dbParamete)
        {
            return DatabaseCommon.ExecuteDataTableByProc(this.dbConnection,procName, dbParamete);
        }
        public DataTable FindTable(string strSql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex, out int total)
        {

            StringBuilder sb = new StringBuilder();
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            int num = (pageIndex - 1) * pageSize;
            int num1 = (pageIndex) * pageSize;
            string OrderBy = "";

            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    OrderBy = " Order By " + orderField;
                }
                else
                {
                    OrderBy = " Order By " + orderField + " " + (isAsc ? "ASC" : "DESC");
                }
            }
            else
            {
                OrderBy = " order by (select 0 )";
            }
            sb.Append(strSql + OrderBy);
            sb.Append(" limit " + num + "," + pageSize + "");
            total = Convert.ToInt32(ExecuteScalar("Select Count(1) From (" + strSql + ") As ttt", dbParameter));
            var IDataReader = DatabaseCommon.ExecuteDataReader(this.dbConnection, sb.ToString(), dbParameter);
            DataTable resultTable = ConvertExtension.IDataReaderToDataTable(IDataReader);
            return resultTable;

        }
        #endregion
        public object FindObject(string strSql)
        {
            return FindObject(strSql, null);
        }
        public object FindObject(string strSql, params DbParameter[] dbParameter)
        {
            return this.ExecuteScalar(strSql, dbParameter);
        }

        public bool Exists<T>(object keyValue) where T : class
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT COUNT(1) FROM {0} where 1=1 ", typeof(T).GetMappingAttributeName());
            sb.AppendFormat("AND {0}=?ID", AttributeHelper.GetEntityPrimaryKey<T>());
            object obj = FindObject(sb.ToString(), new MySqlParameter("?ID", keyValue));
            int cmdresult = 0;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)) || (obj.ToString() == "0"))
                cmdresult = 1;
            return cmdresult == 1 ? false : true;
        }
        public bool Exists<T>(Expression<Func<T, bool>> condition) where T : class
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT COUNT(1) FROM {0} ", typeof(T).GetMappingAttributeName());
            if (condition != null)
            {
                sb.AppendFormat("where {0}", ExpressionToSql.GetWhereByLambda(condition));
            }
            object obj = FindObject(sb.ToString());
            bool cmdresult = true;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)) || (obj.ToString() == "0"))
                cmdresult = false;
            return cmdresult;
        }
        public int GetMaxCode<T>(string pkName) where T : class// 获取最大编号+1
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT MAX(" + pkName + ") FROM " + typeof(T).GetMappingAttributeName());
            object obj = FindObject(strSql.ToString());
            if (!string.IsNullOrEmpty(Convert.ToString(obj)))
            {
                return Convert.ToInt32(obj) + 1;
            }
            return 1;
        }
        public object GetMax<T>(string pkName) where T : class
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT MAX(" + pkName + ") FROM " + typeof(T).GetMappingAttributeName());
            object obj = FindObject(strSql.ToString());
            return obj;
        }

        public DataTable GetDataTablePageList(string tableName, string colNames, int pageIndex, int pageSize, string where, string colFieldAndSortType, ref int count)// //按名称查询分页
        {
            int startNum = (pageIndex - 1) * pageSize;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"Select {0} From {1} where 1=1 {2} Order By {3} limit {4},{5}", colNames, tableName, where, colFieldAndSortType, startNum, pageSize);
            count = this.FindObject(new StringBuilder().AppendFormat("SELECT  COUNT(1) FROM {0} WHERE 1=1 {1}", tableName, where).ToString()).ToInt();
            return FindTable(sb.ToString());
        }
        #endregion
    }
}

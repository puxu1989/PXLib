using PXLib.Attributes;
using PXLib.DataAccess.DbFactory;
using PXLib.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
namespace PXLib.DataAccess.Database.Extension
{
    public class DatabaseCommon
    {
        #region 数据类型处理
        /// <summary>
        /// 数据库类型 静态字段 一次设置多出可用
        /// </summary>
        public static DatabaseType DBType
        {
            get;
            set;
        }

        public static string DbParmCharacter { get { return DbParameters.CreateDbParmCharacter(); } }
        /// <summary>
        /// 创建连接队象和设置数据库类型
        /// </summary>
        /// <param name="connstringName"></param>
        /// <returns></returns>
        public static IDbConnection CreateDbConnection(string connstringName)
        {
            DatabaseType dbType = DatabaseType.SqlServer;
            IDbConnection connection;
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connstringName];
            string connectionString = connectionStringSettings.ConnectionString;
            string providerName = connectionStringSettings.ProviderName;
            if (string.IsNullOrEmpty(providerName))
            {
                throw new Exception(connstringName + "连接字符串未定义 ProviderName");
            }
            else if (providerName == "System.Data.SqlClient")
            {
                dbType = DatabaseType.SqlServer;
                connection = new System.Data.SqlClient.SqlConnection(connectionString);
            }
            //else if (providerName == "Oracle.DataAccess.Client" )
            //{
            //    dbType = dbType.Oracle;
            //    connection = new Oracle.DataAccess.Client.OracleConnection(strConn);
            //}
            else if (providerName == "System.Data.OracleClient")
            {
                dbType = DatabaseType.Oracle;
                connection = new System.Data.OracleClient.OracleConnection(connectionString);
            }
            else if (providerName == "MySql.Data.MySqlClient")
            {
                dbType = DatabaseType.MySql;
                connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            }
            else if (providerName == "System.Data.OleDb")
            {
                dbType = DatabaseType.Access;
                connection = new System.Data.OleDb.OleDbConnection(connectionString);
            }
            else if (providerName == "System.Data.SQLite")//未测试
            {
                dbType = DatabaseType.SQLite;
                connection = new System.Data.OleDb.OleDbConnection(connectionString);
            }
            else
            {
                throw new Exception(connstringName + "连接字符串未识别 ProviderName");
            }
            DBType = dbType;
            return connection;
        }

        /// <summary>
        /// 获取是否字段加双引号
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        public static bool GetWithQuotationMarks()
        {
            bool result = false;
            switch (DBType)
            {

                case DatabaseType.PostGreSql:
                case DatabaseType.Oracle:

                    result = true;
                    break;
            }
            return result;
        }
        /// <summary>
        /// 获得like语句链接符
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        public static string GetLikeConnectorWords()
        {
            string result = "+";
            switch (DBType)
            {

                case DatabaseType.PostGreSql:
                case DatabaseType.Oracle:
                case DatabaseType.MySql:
                case DatabaseType.SQLite:
                    result = "||";
                    break;
            }

            return result;
        }
        #endregion

        #region 执行命令操作
        /// <summary>
        /// 执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(IDbConnection dbConnection, string cmdText, params DbParameter[] parameters)
        {
            try
            {
                using (var conn = dbConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cmdText;
                        if (conn.State != ConnectionState.Open)
                            conn.Open();
                        if (parameters != null)
                        {
                            parameters = DbParameters.ToDbParameter(parameters);
                            foreach (var parameter in parameters)
                            {
                                cmd.Parameters.Add(parameter);

                            }
                        }
                        int result = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return result;
                    }
                }
            }
            catch (DbException e)
            {
                DbLog.WriteLog(e, cmdText);
                if (dbConnection != null)
                    dbConnection.Close();
                throw e;
                //return -1;
            }
        }

        /// <summary>
        /// 执行带事务的SQL语句，并返回受影响的行数（但未真正提交）
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQueryTran(IDbTransaction dbTransaction, string cmdText, params DbParameter[] parameters)
        {
            try
            {
                using (IDbCommand cmd = dbTransaction.Connection.CreateCommand())
                {
                    cmd.Transaction = dbTransaction;
                    cmd.CommandText = cmdText;

                    if (parameters != null)
                    {
                        parameters = DbParameters.ToDbParameter(parameters);
                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (DbException e)
            {
                DbLog.WriteLog(e, cmdText);
                throw e;
            }
        }
        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(IDbConnection dbConnection, string cmdText, params DbParameter[] parameters)
        {
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cmdText;
                        if (conn.State != ConnectionState.Open)
                            conn.Open();
                        if (parameters != null)
                        {
                            parameters = DbParameters.ToDbParameter(parameters);
                            foreach (var parameter in parameters)
                            {
                                cmd.Parameters.Add(parameter);
                            }
                        }
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        return obj;
                    }
                }
            }
            catch (DbException ex)
            {
                DbLog.WriteLog(ex, cmdText);
                if (dbConnection != null)
                    dbConnection.Close();
                throw ex;
                //return null;
            }
        }
        /// <summary>
        /// 执行存储过程  存储过程必须带Out的Out_Res参数
        /// </summary>
        /// <param name="dbConnection">连接</param>
        /// <param name="storedProcName">存储过程名称</param>
        /// <param name="parameters">入参数</param>
        /// <returns>Out_Res为Out参数的执行结果</returns>
        public static object ExecuteByProc(IDbConnection dbConnection, string storedProcName, DbParameter[] parameters, out int rowsAffected)
        {
            try
            {
                using (IDbConnection conn = dbConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = storedProcName;
                        if (conn.State != ConnectionState.Open)
                            conn.Open();
                        if (parameters != null)
                        {
                            parameters = DbParameters.ToDbParameter(parameters);

                            foreach (var parameter in parameters)
                            {
                                cmd.Parameters.Add(parameter);
                            }
                        }
                        cmd.Parameters.Add(DbParameters.CreateDbParameter("@Out_Res", "", DbType.String, 2000, ParameterDirection.Output));
                        rowsAffected = cmd.ExecuteNonQuery();
                        DbParameter obj = (DbParameter)cmd.Parameters["@Out_Res"]; //@Out_Res和具体的存储过程参数对应
                        cmd.Parameters.Clear();
                        return Convert.ToString(obj.Value);
                    }
                }
            }
            catch (DbException ex)
            {
                DbLog.WriteLog(ex, storedProcName);
                if (dbConnection != null)
                    dbConnection.Close();
                throw ex;
            }
        }
        public static DataTable ExecuteDataTableByProc(IDbConnection dbConnection, string storedProcName,params DbParameter[] parameters)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = storedProcName;
                    if (parameters != null)
                    {
                        parameters = DbParameters.ToDbParameter(parameters);

                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }
                    IDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);//DataReader关闭时dbConnection也会关闭
                    cmd.Parameters.Clear();
                    return ConvertExtension.IDataReaderToDataTable(dr);
                }

            }
            catch (DbException ex)
            {
                DbLog.WriteLog(ex, storedProcName);
                if (dbConnection != null)
                    dbConnection.Close();
                throw ex;
            }
        }
        /// <summary>
        /// IDataReader //这里先不用using 调用转换时使用了using 在数据没读到之前数据库连接不能关闭。
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static IDataReader ExecuteDataReader(IDbConnection dbConnection, string cmdText, params DbParameter[] parameters)
        {
            try
            {
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = cmdText;
                    if (parameters != null)
                    {
                        parameters = DbParameters.ToDbParameter(parameters);
                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.Add(parameter);

                        }
                    }
                    IDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);//DataReader关闭时dbConnection也会关闭
                    cmd.Parameters.Clear();
                    return dr;
                }

            }
            catch (DbException ex)
            {
                DbLog.WriteLog(ex, cmdText);
                if (dbConnection != null)
                    dbConnection.Close();
                throw ex;
                //return null;
            }
        }
        #endregion

        #region 对象参数转换DbParameter
        /// <summary>
        /// 对象参数转换DbParameter
        /// </summary>
        /// <returns></returns>
        public static DbParameter[] GetParameter<T>(T entity)
        {
            IList<DbParameter> parameter = new List<DbParameter>();
            DbType dbtype = new DbType();
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo pi in props)
            {
                if (pi.GetValue(entity, null) != null)
                {
                    switch (pi.PropertyType.ToString().ToLower())
                    {
                        case "system.nullable`1[system.int32]":
                            dbtype = DbType.Int32;
                            break;
                        case "system.nullable`1[system.decimal]":
                            dbtype = DbType.Decimal;
                            break;
                        case "system.nullable`1[system.datetime]":
                            dbtype = DbType.DateTime;
                            break;
                        case "system.nullable`1[system.int64]":
                            dbtype = DbType.Int64;
                            break;
                        case "system.nullable`1[system.boolean]":
                            dbtype = DbType.Boolean;
                            break;

                        default:
                            dbtype = DbType.String;
                            break;
                    }
                    parameter.Add(DbParameters.CreateDbParameter(DbParameters.CreateDbParmCharacter() + pi.GetMappingAttributeName(), pi.GetValue(entity, null), dbtype));
                }
            }
            return parameter.ToArray();
        }
        /// <summary>
        /// 对象参数转换DbParameter
        /// </summary>
        /// <returns></returns>
        public static DbParameter[] GetParameter(Hashtable ht)
        {
            IList<DbParameter> parameter = new List<DbParameter>();
            DbType dbtype = new DbType();
            foreach (string key in ht.Keys)
            {
                if (ht[key] is DateTime)
                    dbtype = DbType.DateTime;
                else
                    dbtype = DbType.String;
                parameter.Add(DbParameters.CreateDbParameter(DbParameters.CreateDbParmCharacter() + key, ht[key], dbtype));
            }
            return parameter.ToArray();
        }

        /// <summary>
        /// 为即将执行准备一个命令
        /// </summary>
        /// <param name="conn">SqlConnection对象</param>
        /// <param name="cmd">SqlCommand对象</param>
        /// <param name="isOpenTrans">DbTransaction对象</param>
        /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
        /// <param name="cmdText">存储过程名称或者T-SQL命令行, e.g. Select * from Products</param>
        /// <param name="dbParameter">执行命令所需的sql语句对应参数</param>
        private void PrepareCommand(IDbConnection conn, IDbCommand cmd, DbTransaction isOpenTrans, string cmdText, params DbParameter[] dbParameter)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (isOpenTrans != null)
                cmd.Transaction = isOpenTrans;
            //cmd.CommandType = cmdType;
            if (dbParameter != null)
            {
                dbParameter = DbParameters.ToDbParameter(dbParameter);
                foreach (var parameter in dbParameter)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        #endregion

        #region 拼接 Insert SQL语句
        /// <summary>
        /// 泛型方法，反射生成InsertSql语句
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns>int</returns>
        public static StringBuilder InsertEntitySql<T>(T entity)
        {
            Type type = entity.GetType();
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append($"Insert Into {type.GetMappingAttributeName()} (");
            StringBuilder sp = new StringBuilder();
            StringBuilder sb_prame = new StringBuilder();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity) != null)
                {
                    sb_prame.Append($",{prop.GetMappingAttributeName()}");
                    sp.Append($",{DbParameters.CreateDbParmCharacter() }{prop.GetMappingAttributeName()}");//?ID
                }
            }
            sbSql.Append(sb_prame.ToString().Substring(1, sb_prame.ToString().Length - 1) + ") Values (");
            sbSql.Append(sp.ToString().Substring(1, sp.ToString().Length - 1) + ")");
            return sbSql;
        }
        #endregion

        #region 拼接 Select SQL语句
        /// <summary>
        /// 拼接 查询 SQL语句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="Top">显示条数</param>
        /// <returns></returns>
        public static StringBuilder SelectTopSql<T>(int top)
        {
            StringBuilder strSql = new StringBuilder();
            Type entityType = typeof(T);
            string tableName = entityType.GetMappingAttributeName();
            if (DBType == DatabaseType.MySql)
            {
                strSql.Append($"SELECT * FROM {tableName} WHERE 1=1 limit {top}");
            }
            else//未测试
            {
                strSql.Append($"SELECT top {top} FROM {tableName} WHERE 1=1 ");
            }
            return strSql;
        }
        #endregion

        #region 拼接 Delete SQL语句
        /// <summary>
        /// 拼接删除SQL语句
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns></returns>
        public static StringBuilder DeleteEntitySql<T>(T entity)
        {
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            StringBuilder sb = new StringBuilder($"Delete From {type.GetMappingAttributeName()} Where 1=1");
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity) != null)
                {
                    sb.Append($" AND {prop.GetMappingAttributeName()} = { DbParameters.CreateDbParmCharacter() }{ prop.GetMappingAttributeName()}");
                }
            }
            return sb;
        }
        #endregion

        #region 拼接 Update SQL语句
        /// <summary>
        /// 泛型方法，反射生成UpdateSql语句
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <param name="pkName">主键</param>
        /// <returns>int</returns>
        public static StringBuilder UpdateSql<T>(T entity, string pkName)
        {
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append($"Update {type.GetMappingAttributeName()} SET ");
            bool isFirstValue = true;
            foreach (PropertyInfo prop in props)
            {
                string columnName = prop.GetMappingAttributeName();
                if (prop.GetValue(entity, null) != null && AttributeHelper.GetEntityPrimaryKey<T>() != columnName)//不更新主键
                {
                    if (isFirstValue)
                    {
                        isFirstValue = false;
                        sbSql.Append(columnName);
                        sbSql.Append("=");
                        sbSql.Append(DbParameters.CreateDbParmCharacter() + columnName);
                    }
                    else
                    {
                        sbSql.Append("," + columnName);
                        sbSql.Append("=");
                        sbSql.Append(DbParameters.CreateDbParmCharacter() + columnName);
                    }
                }
            }
            sbSql.Append(" Where ").Append(pkName).Append("=").Append(DbParameters.CreateDbParmCharacter() + pkName);
            return sbSql;
        }
        /// <summary>
        /// 泛型方法，反射生成UpdateSql语句
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns>int</returns>
        public static StringBuilder UpdateEntitySql<T>(T entity)
        {
            Type type = entity.GetType();
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append($"Update {type.GetMappingAttributeName()} SET ");
            bool isFirstValue = true;        
            string pkName = AttributeHelper.GetEntityPrimaryKey<T>();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                string columnName = prop.GetMappingAttributeName();
                if (prop.GetValue(entity, null) != null && pkName != columnName)//不更新主键
                {
                    if (isFirstValue)
                    {
                        isFirstValue = false;
                        sbSql.Append(columnName);
                        sbSql.Append("=");
                        sbSql.Append(DbParameters.CreateDbParmCharacter() + columnName);
                    }
                    else
                    {
                        sbSql.Append("," + columnName);
                        sbSql.Append("=");
                        sbSql.Append(DbParameters.CreateDbParmCharacter() + columnName);
                    }
                }
            }
            sbSql.Append(" Where ").Append(pkName).Append("=").Append(DbParameters.CreateDbParmCharacter() + pkName);
            return sbSql;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PXLib.DataAccess
{
    public class MsSqlHelper
    {
        public static readonly string connstr = ConfigurationManager.AppSettings["MsSqlConnectionString"].ToString().Trim();
        //<add key="MsSqlConnectionString" value="server=192.168.1.103\SQL2008; Database=ThesisMS; Uid=sa;Pwd=123456" />
        #region 基本操作
        /// <summary>
        /// 查询返回DataTable
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string cmdText, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                try
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = cmdText;
                        cmd.Parameters.AddRange(parameters);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            cmd.Parameters.Clear();
                            return dt;
                        }
                    }
                }
                catch (SqlException e)
                {
                    DbLog.WriteLog(e);
                    return null;
                }
            }
        }
        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string cmdText, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = cmdText;
                    cmd.Parameters.AddRange(parameters);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return obj;
                }
            }
        }
        /// <summary>
        /// 返回影响的行数
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string cmdText, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandText = cmdText;
                        cmd.Parameters.AddRange(parameters);
                        int result = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return result;
                    }
                    catch (SqlException e)
                    {
                        DbLog.WriteLog(e);
                        return -1;
                    }
                }
            }
        }
        public static SqlDataReader ExecuteDataReader(string cmdText, params SqlParameter[] parameters)//这里先不用using 调用转换时使用了using
        {
            try
            {
                SqlConnection conn = new SqlConnection(connstr);
                SqlCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = cmdText;
                cmd.Parameters.AddRange(parameters);
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return dr;
            }
            catch (SqlException ex)
            {
                DbLog.WriteLog(ex);
                return null;
            }
        }
        /// <summary>
        /// 返回首行首列--与ExecuteScalar一样
        /// </summary>
        /// <param name="SQLString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object GetSingle(string SQLString, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddRange(parameters);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)))
                            return null;
                        else
                            return obj;
                    }
                    catch (Exception e)
                    {
                        conn.Close();
                        DbLog.WriteLog(e);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 是否存在记录
        /// </summary>
        /// <param name="strSql">查询count(1)既可或查询字段值不为0的sql语句</param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {

            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            return cmdresult == 0 ? false : true;
        }

        // 是否存在记录
        public static bool Exists(string tableName, string where, params SqlParameter[] cmdParms)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT COUNT(1) FROM {0} where 1=1 ", tableName);
            sb.Append(where);
            object obj = GetSingle(sb.ToString(), cmdParms);
            int cmdresult = 0;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)) || (obj.ToString() == "0"))
                cmdresult = 1;
            return cmdresult == 1 ? false : true;
        }
        /// <summary>
        ///  批量执行多条SQL语句，实现数据库事务,不好用 没考虑特殊字符。
        /// </summary>
        /// <param name="SQLStringList">List集合中是多条SQL语句</param>		
        public static int ExecuteSqlTran(List<String> SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    int count = 0;
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch (SqlException e)
                {
                    DbLog.WriteLog(e);
                    tx.Rollback();
                    return 0;
                }
            }
        }
        /// <summary>
        /// //批量执行sql，实现事务 返回>=1 成功
        /// </summary>
        /// <param name="sqls">多条sql</param>
        /// <param name="param">sql参数化</param>
        /// <returns></returns>
        public static int BatchExecuteBySql(object[] sqls, object[] param)
        {
            int num = 0;
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int i = 0; i < sqls.Length; i++)
                    {
                        StringBuilder builder = (StringBuilder)sqls[i];
                        if (builder != null)
                        {
                            cmd.CommandText = builder.ToString();
                            if (param != null)
                            {
                                SqlParameter[] paramArray = (SqlParameter[])param[i];
                                cmd.Parameters.AddRange(paramArray);
                            }
                            num += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (Exception e)
                {
                    num = 0;
                    DbLog.WriteLog(e);
                    tx.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                    tx.Dispose();
                }
            }
            return num;
        }
        #endregion

    }
}

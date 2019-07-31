using MySql.Data.MySqlClient;
using PXLib.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PXLib.DataAccess
{
    public class MySqlDBHelper
    {

        public static readonly string connstr = ConfigurationManager.AppSettings["MySqlConnectionString"].ToString().Trim();
        // connectionString="Server=localhost;Database=drivetop_base; User=otnp80;Password=123;Use Procedure Bodies=false;Charset=utf8;Allow Zero Datetime=True; Pooling=True; Max Pool Size=50; "

        #region 基本操作
        /// <summary>
        /// 查询返回DataTable
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string cmdText, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                try
                {
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = cmdText;
                        cmd.Parameters.AddRange(parameters);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            cmd.Parameters.Clear();
                            return dt;
                        }
                    }
                }
                catch (MySqlException e)
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
        public static object ExecuteScalar(string cmdText, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                using (MySqlCommand cmd = conn.CreateCommand())
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
        public static int ExecuteNonQuery(string cmdText, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                using (MySqlCommand cmd = conn.CreateCommand())
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
                    catch (MySqlException e)
                    {
                        DbLog.WriteLog(e);
                        return -1;
                    }
                }
            }
        }
        public static MySqlDataReader ExecuteDataReader(string cmdText, params MySqlParameter[] parameters)//这里先不用using 调用转换时使用了using
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(connstr);
                MySqlCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = cmdText;
                cmd.Parameters.AddRange(parameters);
                MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return dr;
            }
            catch (MySqlException ex)
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
        public static object GetSingle(string SQLString, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, conn))
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
        public static bool Exists(string strSql, params MySqlParameter[] cmdParms)
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
        public static bool Exists(string tableName, string where, params MySqlParameter[] cmdParms)
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
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                MySqlTransaction tx = conn.BeginTransaction();
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
                catch (MySqlException e)
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
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    MySqlTransaction tx = conn.BeginTransaction();
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
                                    MySqlParameter[] paramArray = (MySqlParameter[])param[i];
                                    cmd.Parameters.AddRange(paramArray);
                                }
                                num += cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch (Exception e)
                    {
                        num = 0; e.ToString();
                        tx.Rollback();
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                        tx.Dispose();
                    }
                }
            }
            catch
            {
                //DbLog.WriteException(e);
            }
            return num;
        }
        #endregion
        #region 存储过程相关 未测试
        ///// <summary>
        ///// 返回DataTable,参数带有状态码的存储过程 oracle的存储过程参数必须定义输出参数rcode和dt游标类型，且名字不能改
        ///// </summary>
        ///// <param name="cmdText"></param>
        ///// <param name="rcode">返回码</param>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //public static DataTable ExecuteDataTableByProcSQL(string cmdText, ref int rcode, params MySqlParameter[] parameters)
        //{
        //    DataTable dt = new DataTable();
        //    using (MySqlConnection conn = new MySqlConnection(connstr))
        //    {

        //        using (MySqlCommand cmd = new MySqlCommand(cmdText, conn))
        //        {
        //            try
        //            {
        //                conn.Open();
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.AddRange(parameters);
        //                cmd.Parameters.Add(new MySqlParameter("rcode", MySqlDbType.Int32));
        //                cmd.Parameters["rcode"].Direction = ParameterDirection.Output;
        //                cmd.Parameters.Add(new MySqlParameter("dt", MySqlDbType.c));
        //                cmd.Parameters["dt"].Direction = ParameterDirection.Output;
        //                //这里没有@符号：区别SqlServer的写法,Sql中返回结果集可以直接使用select语句,
        //                //而Orcale中返回结果集要有游标
        //                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
        //                da.Fill(dt);
        //                rcode = Convert.ToInt32(cmd.Parameters["rcode"].Value);
        //            }
        //            catch (MySqlException ex)
        //            {
        //                log.WriteLog(ex);
        //                return null;
        //            }
        //        }
        //    }
        //    return dt;
        //}

        /// <summary>
        /// 调用存储过程返回DataTable,输出参数为dt的游标类型
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTableByProcSQL(string cmdText, params MySqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        if(!parameters.IsNullEmpty())
                        cmd.Parameters.AddRange(parameters);
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (MySqlException ex)
                    {
                        DbLog.WriteLog(ex);
                        return null;
                    }
                }
            }
            return dt;
        }
        /// <summary>
        /// 执行存储过程返回一系列字符 输出参数必须定义为o_resStr
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ExecuteStringByProcSQL(string cmdText, params MySqlParameter[] parameters)
        {
            string str = "";
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters);
                        cmd.Parameters.Add(new MySqlParameter("o_resStr", MySqlDbType.VarChar, 4000));//这里需要设置长度
                        cmd.Parameters["o_resStr"].Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();//执行成功返回默认影响行数为1
                        str = Convert.ToString(cmd.Parameters["o_resStr"].Value);
                    }
                    catch (MySqlException ex)
                    {
                        DbLog.WriteLog(ex);
                        return ex.Message.ToString();
                    }
                }
            }
            return str;
        }
        #endregion

        #region ---------------------------------------------其他操作-----------------------------------------------------
        #region 参数化
        public static string ParamKey
        {
            get { return "?"; }  //参数化oracle(:或者&) sqlServer里是(@)，MySQL里是(?,但在.net里也可以使用@)
        }
        public static MySqlParameter[] GetParameter(Hashtable ht)
        {
            MySqlDbType dbtype = new MySqlDbType();
            IList<MySqlParameter> sqlparam = new List<MySqlParameter>();
            foreach (string key in ht.Keys)
            {
                if (ht[key] != null)
                {
                    if (ht[key] is DateTime)
                        dbtype = MySqlDbType.DateTime;
                    else if (Convert.ToString(ht[key]).Length >= 4000 && !string.IsNullOrEmpty(ht[key] as string))
                        dbtype = MySqlDbType.String;
                    else
                        dbtype = MySqlDbType.VarChar;
                    sqlparam.Add(new MySqlParameter(ParamKey + key, dbtype, 0, ParameterDirection.Input, true, 0, 0, "", DataRowVersion.Current, ht[key]));
                    //这里处理日期类型 
                }
            }
            return sqlparam.ToArray();
        }
        public static MySqlParameter[] GetParameter<T>(T entity)
        {
            MySqlDbType dbtype = new MySqlDbType();
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            IList<MySqlParameter> sqlparam = new List<MySqlParameter>();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    if (prop.PropertyType.ToString() == "System.Nullable`1[System.DateTime]")
                        dbtype = MySqlDbType.DateTime;
                    else if (!string.IsNullOrEmpty(prop.GetValue(entity, null) as string) && Convert.ToString(prop.GetValue(entity, null)).Length >= 4000)
                        dbtype = MySqlDbType.String;
                    else
                        dbtype = MySqlDbType.VarChar;
                    sqlparam.Add(new MySqlParameter(ParamKey + prop.Name, dbtype, 0, ParameterDirection.Input, true, 0, 0, "", DataRowVersion.Default, prop.GetValue(entity, null)));

                }
            }
            return sqlparam.ToArray();
        }

        #endregion
        #region 插入
        public static StringBuilder InsertSql(string tableName, Hashtable ht)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Insert Into ");
            sb.Append(tableName);
            sb.Append("(");
            StringBuilder sp = new StringBuilder();
            StringBuilder sb_prame = new StringBuilder();
            foreach (string key in ht.Keys)
            {
                if (ht[key] != null)
                {
                    sb_prame.Append("," + key);
                    sp.Append("," + ParamKey + "" + key);
                }
            }
            sb.Append(sb_prame.ToString().Substring(1, sb_prame.ToString().Length - 1) + ") Values (");
            sb.Append(sp.ToString().Substring(1, sp.ToString().Length - 1) + ")");
            return sb;
        }
        public static bool Insert(string tableName, Hashtable ht)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Insert Into ");
            sb.Append(tableName);
            sb.Append("(");
            StringBuilder sp = new StringBuilder();
            StringBuilder sb_prame = new StringBuilder();
            foreach (string key in ht.Keys)
            {
                if (ht[key] != null)
                {
                    sb_prame.Append("," + key);
                    sp.Append("," + ParamKey + "" + key);
                }
            }
            sb.Append(sb_prame.ToString().Substring(1, sb_prame.ToString().Length - 1) + ") Values (");
            sb.Append(sp.ToString().Substring(1, sp.ToString().Length - 1) + ")");
            //拼接好后插入数据库
            return ExecuteNonQuery(sb.ToString(), GetParameter(ht)) == 1 ? true : false;
        }
        public static bool Insert<T>(T entity)
        {
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            StringBuilder sb = new StringBuilder();
            sb.Append(" Insert Into ");
            sb.Append(type.Name);
            sb.Append("(");
            StringBuilder sp = new StringBuilder();
            StringBuilder sb_prame = new StringBuilder();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    sb_prame.Append("," + (prop.Name));
                    sp.Append("," + ParamKey + "" + (prop.Name));
                }
            }
            sb.Append(sb_prame.ToString().Substring(1, sb_prame.ToString().Length - 1) + ") Values (");
            sb.Append(sp.ToString().Substring(1, sp.ToString().Length - 1) + ")");
            return ExecuteNonQuery(sb.ToString(), GetParameter(entity)) == 1 ? true : false;
        }
        #endregion
        #region 更新
        public static bool UpdateByName(string tableName, string pkName, Hashtable ht)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Update ");
            sb.Append(tableName);
            sb.Append(" Set ");
            bool isFirstValue = true;
            foreach (string key in ht.Keys)
            {
                if (ht[key] != null)
                {
                    if (isFirstValue)
                    {
                        isFirstValue = false;
                        sb.Append(key);
                        sb.Append("=");
                        sb.Append(ParamKey + key);
                    }
                    else
                    {
                        sb.Append("," + key);
                        sb.Append("=");
                        sb.Append(ParamKey + key);
                    }
                }
            }
            sb.Append(" Where ").Append(pkName).Append("=").Append(ParamKey + pkName);
            return ExecuteNonQuery(sb.ToString(), GetParameter(ht)) == 1 ? true : false;
        }
        public static bool UpdateByName<T>(T entity, string pkName)
        {
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            StringBuilder sb = new StringBuilder();
            sb.Append(" Update ");
            sb.Append(type.Name);
            sb.Append(" Set ");
            bool isFirstValue = true;
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    if (isFirstValue)
                    {
                        isFirstValue = false;
                        sb.Append(prop.Name);
                        sb.Append("=");
                        sb.Append(ParamKey + prop.Name);
                    }
                    else
                    {
                        sb.Append("," + prop.Name);
                        sb.Append("=");
                        sb.Append(ParamKey + prop.Name);
                    }
                }
            }
            sb.Append(" Where ").Append(pkName).Append("=").Append(ParamKey + pkName);
            return ExecuteNonQuery(sb.ToString(), GetParameter(entity)) == 1 ? true : false;
        }
        public static void UpdateDataSet(string commandText, DataSet ds, string tablename)//未使用过
        {
            MySqlConnection mySqlConnection = new MySqlConnection(connstr);
            mySqlConnection.Open();
            MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(commandText, mySqlConnection);
            //MySqlCommandBuilder mySqlCommandBuilder = new MySqlCommandBuilder(mySqlDataAdapter);
            //mySqlCommandBuilder.ToString();
            mySqlDataAdapter.Update(ds, tablename);
            mySqlConnection.Close();
        }
        #endregion
        #region 删除
        //单条件删除语句
        public static bool DeleteByName(string tableName, string pkName, object pkValue)
        {
            if (string.IsNullOrEmpty(pkValue.ToString()))
                return false;
            StringBuilder sb = new StringBuilder("Delete From " + tableName + " Where " + pkName + " = " + ParamKey + pkName + "");
            return ExecuteNonQuery(sb.ToString(), new MySqlParameter(pkName, pkValue)) > 0 ? true : false;
        }
        //单条件删除语句
        public static int DeleteWhere(string tableName, string where, params MySqlParameter[] p)
        {
            StringBuilder sb = new StringBuilder("Delete From " + tableName);
            if (!string.IsNullOrEmpty(where))
            {
                sb.Append(" Where 1=1 AND " + where);
            }
            return ExecuteNonQuery(sb.ToString(), p);
        }
        #endregion
        #region 查询
        public static DataTable GetDataTableByName(string tableName, string pkName, object pkVal)
        {
            DataTable dt = null;
            if (string.IsNullOrEmpty(pkVal.ToString()))
                return null;
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM ").Append(tableName).Append(" Where ").Append(pkName).Append("=" + ParamKey + "ID");
            dt = ExecuteDataTable(sb.ToString(), new MySqlParameter(ParamKey + "ID", pkVal));
            return dt;
        }

        public static T GetModelByName<T>(string pkName, object pkVal)
        {
            if (string.IsNullOrEmpty(pkVal.ToString()))
                return default(T);
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM ").Append(type.Name).Append(" Where ").Append(pkName).Append("=" + ParamKey + "ID");
            DataTable dt = ExecuteDataTable(sb.ToString(), new MySqlParameter(ParamKey + "ID", pkVal));
            if (dt != null && dt.Rows.Count > 0)
                return DataConvertHelper.DataRowToModel<T>(dt.Rows[0]);
            return model;
        }
        public static T GetModelByHash<T>(Hashtable ht)//多条件查询
        {
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT * FROM " + type.Name + " WHERE 1=1");
            foreach (string key in ht.Keys)
            {
                strSql.Append(" AND " + key + " = " + ParamKey + "" + key + "");
            }
            DataTable dt = ExecuteDataTable(strSql.ToString(), GetParameter(ht));
            if (dt.Rows.Count > 0)
            {
                return DataConvertHelper.DataRowToModel<T>(dt.Rows[0]);
            }
            return model;
        }


        // 获取所有Where条件下的所有数据（适合小数量数据，大数量数据请使用分页）
        public static DataTable GetListWhere(string tableName, string where)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT * FROM {0} WHERE 1=1 ", tableName);
            strSql.Append(where);
            //strSql.Append(" Order BY SortCode");
            return ExecuteDataTable(strSql.ToString());
        }
        //查询所传字段名条件where的所有数据
        public static DataTable GetListWhereOrderBy(string tableName, string colNames, string where, string colNameSort)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT {0} FROM {1} WHERE 1=1 ", colNames, tableName);
            strSql.Append(where);
            strSql.AppendFormat(" Order BY {0} ", colNameSort);
            return ExecuteDataTable(strSql.ToString());
        }
        /// <summary>
        /// 获取Ilist colNameSort==null不排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="colNameSort"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static List<T> GetListWhere<T>(string where, string colNameSort, params MySqlParameter[] cmdParms)//List<T>==IList
        {
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT * FROM {0} WHERE 1=1 ", type.Name);
            if (!string.IsNullOrEmpty(where))
                strSql.Append(where);
            if (!string.IsNullOrEmpty(colNameSort))
                strSql.AppendFormat(" Order BY {0}", colNameSort);
            return DataConvertHelper.ReaderToList<T>(ExecuteDataReader(strSql.ToString(), cmdParms));
        }
        public static object GetMax(string tableName, string pkName)// 获取最大编号
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT MAX(" + pkName + ") FROM " + tableName + "");
            object obj = GetSingle(strSql.ToString());
            if (!string.IsNullOrEmpty(Convert.ToString(obj)))
            {
                return Convert.ToInt32(obj) + 1;
            }
            return 1;
        }
        //返回前几条条数据
        public static DataTable GetTop(string tableName, string colName, string where, string colNameSort, int topNum)
        {
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat("select {0} from {1} where 1=1 {2} order by {3} LIMIT {4}", colName, tableName, where, colNameSort, topNum);
            return ExecuteDataTable(strSql.ToString());
        }
        //返回一个用 | 分隔的字符串
        public static string GetStringList(string sqlString, params MySqlParameter[] cmdParms)
        {
            string rs = string.Empty;
            MySqlDataReader myReader = ExecuteDataReader(sqlString, cmdParms);
            while (myReader.Read())
            {
                if (rs.Length > 0)
                    rs = rs + "|" + myReader[0].ToString();
                else
                    rs = myReader[0].ToString();
            }
            myReader.Close();
            return rs;
        }
        public static DataTable GetPageList(string tableName, int pageIndex, int pageSize, string where, string colFieldAndSortType, ref int count)//数据分页
        {
            int startNum = (pageIndex - 1) * pageSize;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"Select * From {0} where 1=1 {1} Order By {2} limit {3},{4}", tableName, where, colFieldAndSortType, startNum, pageSize);
            count = Convert.ToInt32(GetSingle(new StringBuilder().AppendFormat("SELECT  COUNT(1) FROM {0} WHERE 1=1 {1}", tableName, where).ToString()));
            return ExecuteDataTable(sb.ToString());
        }
        //按列名查询分页
        public static DataTable GetPageList(string tableName, string colNames, int pageIndex, int pageSize, string where, string colFieldAndSortType, ref int count)
        {
            int startNum = (pageIndex - 1) * pageSize;
            //int endNum =  pageSize;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"Select {0} From {1} where 1=1 {2} Order By {3} limit {4},{5}", colNames, tableName, where, colFieldAndSortType, startNum, pageSize);
            count = Convert.ToInt32(GetSingle(new StringBuilder().AppendFormat("SELECT  COUNT(1) FROM {0} WHERE 1=1 {1}", tableName, where).ToString()));
            return ExecuteDataTable(sb.ToString());
        }
        #endregion

        #endregion

    }
}

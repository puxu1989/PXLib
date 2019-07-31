using PXLib.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PXLib.DataAccess
{
    public  class OracleDBHelper
    {
        protected static LogHelper log = new LogHelper("OrclDB数据库帮助类");

        public static readonly string connstr = ConfigurationManager.AppSettings["OracleConnectionString"].ToString().Trim();
        // <add key="OracleConnectionString" value="Data Source=ZFKJORCL;Persist Security Info=True;User ID=E3D;Password=E3D;Unicode=True"/>

        #region 基本操作
        public static OracleConnection OpenConnection()
        {
            OracleConnection conn = new OracleConnection(connstr);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            return conn;

        }
        /// <summary>
        /// 查询返回DataTable
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string cmdText, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                try
                {
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = cmdText;
                        cmd.Parameters.AddRange(parameters);
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            cmd.Parameters.Clear();
                            return dt;
                        }
                    }
                }
                catch (OracleException e)
                {
                    log.WriteLog(e);
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
        public static object ExecuteScalar(string cmdText, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                using (OracleCommand cmd = conn.CreateCommand())
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
        public static int ExecuteNonQuery(string cmdText, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                using (OracleCommand cmd = conn.CreateCommand())
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
                    catch (OracleException e)
                    {
                        log.WriteLog(e);
                        return -1;
                    }
                }
            }
        }
        public static OracleDataReader ExecuteDataReader(string cmdText, params OracleParameter[] parameters)//这里先不用using 调用转换时使用了using
        {
            try
            {
                OracleConnection conn = new OracleConnection(connstr);
                OracleCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = cmdText;
                cmd.Parameters.AddRange(parameters);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return dr;
            }
            catch (OracleException ex)
            {
                log.WriteLog(ex);
                return null;
            }
        }
        /// <summary>
        /// 返回首行首列--与ExecuteScalar一样
        /// </summary>
        /// <param name="SQLString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object GetSingle(string SQLString, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, conn))
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
                        log.WriteLog(e);
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
        public static bool Exists(string strSql, params OracleParameter[] cmdParms)
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
        public static bool Exists(string tableName, string where, params OracleParameter[] cmdParms)
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
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                OracleTransaction tx = conn.BeginTransaction();
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
                catch (OracleException e)
                {
                    log.WriteLog(e);
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
                using (OracleConnection conn = new OracleConnection(connstr))
                {
                    conn.Open();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;
                    OracleTransaction tx = conn.BeginTransaction();
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
                                    OracleParameter[] paramArray = (OracleParameter[])param[i];
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

        #region 存储过程相关
        /// <summary>
        /// 返回DataTable,参数带有状态码的存储过程 oracle的存储过程参数必须定义输出参数rcode和dt游标类型，且名字不能改
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="rcode">返回码</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTableByProcSQL(string cmdText, ref int rcode, params OracleParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (OracleConnection conn = new OracleConnection(connstr))
            {

                using (OracleCommand cmd = new OracleCommand(cmdText, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters);
                        cmd.Parameters.Add(new OracleParameter("rcode", OracleType.Int32));
                        cmd.Parameters["rcode"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(new OracleParameter("dt", OracleType.Cursor));
                        cmd.Parameters["dt"].Direction = ParameterDirection.Output;
                        //这里没有@符号：区别SqlServer的写法,Sql中返回结果集可以直接使用select语句,
                        //而Orcale中返回结果集要有游标
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);
                        rcode = Convert.ToInt32(cmd.Parameters["rcode"].Value);
                    }
                    catch (OracleException ex)
                    {
                        log.WriteLog(ex);
                        return null;
                    }
                }

            }
            return dt;
        }
        /// <summary>
        /// 调用存储过程返回DataTable,输出参数为dt的游标类型
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="rcode">返回码</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTableByProcSQL(string cmdText, params OracleParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters);
                        cmd.Parameters.Add(new OracleParameter("dt", OracleType.Cursor));
                        cmd.Parameters["dt"].Direction = ParameterDirection.Output;
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (OracleException ex)
                    {
                        log.WriteLog(ex);
                        return null;
                    }
                }
            }
            return dt;
        }
        /// <summary>
        /// 执行存储过程返回一系列字符 输出参数必须定义为o_str
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ExecuteStringByProcSQL(string cmdText, params OracleParameter[] parameters)
        {
            string str = "";
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters);
                        cmd.Parameters.Add(new OracleParameter("o_str", OracleType.VarChar, 4000));//这里需要设置长度
                        cmd.Parameters["o_str"].Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();//执行成功返回默认影响行数为1
                        str = Convert.ToString(cmd.Parameters["o_str"].Value);
                    }
                    catch (OracleException ex)
                    {
                        log.WriteLog(ex);
                        return ex.Message.ToString();
                    }
                }
            }
            return str;
        }
        #endregion

        #region 其他
        //如果需要设计为返回输出多个参数的方法 那么需要定义一个参数数组OracleParameter 往里面加（new）参数,设置方向等,执行过程后再从参数里取值既可

        //---------------------------------------------其他操作-----------------------------------------------------
        #region 参数化
        public static string ParamKey
        {
            get { return "&"; }  //参数化oracle(:或者&) sqlServer里是(@)，MySQL里是(?)
        }
        public static OracleParameter[] GetParameter(Hashtable ht)
        {
            OracleType dbtype = new OracleType();
            // DbType dbtype = new DbType();
            IList<OracleParameter> sqlparam = new List<OracleParameter>();
            foreach (string key in ht.Keys)
            {
                if (ht[key] != null)
                {
                    if (ht[key] is DateTime)
                        dbtype = OracleType.DateTime;
                    else if (Convert.ToString(ht[key]).Length >= 4000 && !string.IsNullOrEmpty(ht[key] as string))
                        dbtype = OracleType.Clob;//Clob不能为null 最大长度问题
                    else
                        dbtype = OracleType.VarChar;
                    sqlparam.Add(new OracleParameter(ParamKey + key, dbtype, 0, ParameterDirection.Input, "", DataRowVersion.Current, true, ht[key]));
                    //这里处理日期类型 参数化to-date  tochar都比较麻烦   如果数据库的类型是date 这里传入系统时间则要传入日期类型 否者报错
                    //size单位bit,0表示不限制根据类型长度自行推断
                }
            }
            return sqlparam.ToArray();
        }
        public static OracleParameter[] GetParameter<T>(T entity)
        {
            OracleType dbtype = new OracleType();
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            IList<OracleParameter> sqlparam = new List<OracleParameter>();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    if (prop.PropertyType.ToString() == "System.Nullable`1[System.DateTime]")
                        dbtype = OracleType.DateTime;
                    else if (!string.IsNullOrEmpty(prop.GetValue(entity, null) as string) && Convert.ToString(prop.GetValue(entity, null)).Length >= 4000)
                        dbtype = OracleType.Clob;
                    else
                        dbtype = OracleType.VarChar;
                    sqlparam.Add(new OracleParameter(ParamKey + prop.Name, dbtype, 0, ParameterDirection.Input, "", DataRowVersion.Default, false, prop.GetValue(entity, null)));

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
        #endregion
        #region 删除
        //单条件删除语句
        public static int DeleteByName(string tableName, string pkName, string pkValue)
        {
            StringBuilder sb = new StringBuilder("Delete From " + tableName + " Where " + pkName + " = " + ParamKey + pkName + "");
            return ExecuteNonQuery(sb.ToString(), new OracleParameter(pkName, pkValue));
        }
        // 多条件删除语句
        public static int Delete(string tableName, Hashtable ht)
        {

            StringBuilder sb = new StringBuilder("Delete From " + tableName + " Where 1=1");
            foreach (string key in ht.Keys)
            {
                sb.Append(" AND " + key + " = " + ParamKey + "" + key + "");
            }
            return ExecuteNonQuery(sb.ToString(), GetParameter(ht));
        }
        #endregion
        #region 查询
        public static DataTable GetDataTableByName(string tableName, string pkName, string pkVal)
        {
            DataTable dt = null;
            if (string.IsNullOrEmpty(pkVal))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM ").Append(tableName).Append(" Where ").Append(pkName).Append("=" + ParamKey + "ID");
            dt = ExecuteDataTable(sb.ToString(), new OracleParameter(ParamKey + "ID", pkVal));
            return dt;
        }

        public static T GetModelByName<T>(string pkName, string pkVal)
        {
            if (string.IsNullOrEmpty(pkVal))
            {
                return default(T);
            }
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM ").Append(type.Name).Append(" Where ").Append(pkName).Append("=" + ParamKey + "ID");
            DataTable dt = ExecuteDataTable(sb.ToString(), new OracleParameter(ParamKey + "ID", pkVal));
            if (dt != null && dt.Rows.Count > 0)
            {
                return DataConvertHelper.DataRowToModel<T>(dt.Rows[0]);
            }
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
            strSql.Append(" Order BY SortCode");
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
        /// 返回Ilist或者List<T> 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="colNameSort">null 则不排序</param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static List<T> GetListWhere<T>(string where, string colNameSort, params OracleParameter[] cmdParms)
        {
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT * FROM {0} WHERE 1=1 ", type.Name);
            strSql.Append(where);
            if (!string.IsNullOrEmpty(colNameSort))
                strSql.AppendFormat(" Order BY {0}", colNameSort);
            return DataConvertHelper.ReaderToList<T>(ExecuteDataReader(strSql.ToString(), cmdParms));
        }
        public static object GetMax(string tableName, string pkName)// 获取最大编号
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT MAX(to_number(" + pkName + ")) FROM " + tableName + "");//MAX区别SQL 排序字段可能为varchar2类型时这里要to_number一下  
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
            strSql.AppendFormat("select * from (select {0} from {1} where 1=1 {2} order by {3}) where rownum <={4}", colName, tableName, where, colNameSort, topNum);
            return ExecuteDataTable(strSql.ToString());
        }
        //返回一个用 | 分隔的字符串
        public static string GetStringList(string sqlString, params OracleParameter[] cmdParms)
        {
            string rs = string.Empty;
            OracleDataReader myReader = ExecuteDataReader(sqlString, cmdParms);
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
        public static DataTable GetPageList(string tableName, int pageIndex, int pageSize, string where, string colField, string sortType, ref int count)//数据分页
        {
            int endNum = pageIndex * pageSize;
            int startNum = endNum - pageSize + 1;
            //int startNum = (pageIndex - 1) * pageSize+1;
            //int endNum = pageIndex * pageSize;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"SELECT ROW_NUMBER,  TT.*
              FROM (SELECT ROWNUM ROW_NUMBER,  
                      T.*  FROM (SELECT *
                              FROM {0} R WHERE 1=1 {1} ORDER BY {2} {3},ROWNUM) T
                     WHERE ROWNUM <= {4}) TT
             WHERE ROW_NUMBER >= {5}", tableName, where, colField, sortType, endNum, startNum);

            count = Convert.ToInt32(GetSingle(new StringBuilder().AppendFormat("SELECT  COUNT(1) FROM {0} WHERE 1=1 {1}", tableName, where).ToString()));
            return ExecuteDataTable(sb.ToString());
        }
        //按列名查询分页
        public static DataTable GetPageList(string tableName, string colNames, int pageIndex, int pageSize, string where, string colField, string sortType, ref int count)
        {
            int startNum = (pageIndex - 1) * pageSize + 1;
            int endNum = pageIndex * pageSize;
            StringBuilder sb = new StringBuilder();
            StringBuilder sbCol = new StringBuilder();

            sb.AppendFormat(@"SELECT ROW_NUMBER,  TT.*
              FROM (SELECT ROWNUM ROW_NUMBER,  
                      T.*  FROM (SELECT {6}
                              FROM {0} R WHERE 1=1 {1} ORDER BY {2} {3},ROWNUM) T
                     WHERE ROWNUM <= {4}) TT
             WHERE ROW_NUMBER >= {5}", tableName, where, colField, sortType, endNum, startNum, colNames);

            count = Convert.ToInt32(GetSingle(new StringBuilder().AppendFormat("SELECT  COUNT(1) FROM {0} WHERE 1=1 {1}", tableName, where).ToString()));
            return ExecuteDataTable(sb.ToString());
        }
        #endregion
        #endregion
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PXLib.DataAccess.Database.Extension
{
 
    /// <summary>
    /// 根据数据库类型进行SQL参数化
    /// </summary>
    public class DbParameters
    {
        /// <summary>
        /// 根据配置文件中所配置的数据库类型
        /// 来获取命令参数中的参数符号oracle为":",sqlserver为"@"
        /// </summary>
        /// <returns></returns>
        public static string CreateDbParmCharacter()
        {
            string character = string.Empty;
            switch (DatabaseCommon.DBType)
            {
                case DatabaseType.SqlServer:
                    character = "@";
                    break;
                case DatabaseType.Oracle:
                    character = ":";
                    break;
                case DatabaseType.MySql:
                    character = "?";
                    break;
                case DatabaseType.Access:
                    character = "@";
                    break;
                case DatabaseType.SQLite:
                    character = "@";
                    break;
                default:
                    throw new Exception("数据库类型目前不支持！");
            }
            return character;
        }
        /// <summary>
        /// 根据配置文件中所配置的数据库类型
        /// 来创建相应数据库的参数对象
        /// </summary>
        /// <returns></returns>
        public static DbParameter CreateDbParameter()
        {

            DbParameter prm = null;
            switch (DatabaseCommon.DBType)
            {
                case DatabaseType.SqlServer:
                    prm= new SqlParameter();
                    break;
                case DatabaseType.Oracle:
                    prm= new OracleParameter();
                    break;
                case DatabaseType.MySql:
                    prm= new MySqlParameter();
                    break;
                //case DatabaseType.Access:
                //    prm = new 
                //    break;
                //case DatabaseType.SQLite:
                //   prm=new
                //    break;
                default:
                    throw new Exception("数据库类型目前不支持！");
            }
            return prm;
        }
        /// <summary>
        /// 根据配置文件中所配置的数据库类型
        /// 来创建相应数据库的参数对象
        /// </summary>
        /// <returns></returns>
        public static DbParameter CreateDbParameter(string paramName, object value)
        {
            DbParameter param = DbParameters.CreateDbParameter();
            param.ParameterName = paramName;
            param.Value = value;
            return param;
        }
        public static DbParameter CreateDbParameter(string paramName, object value, DbType dbType, int dbTypeSize, ParameterDirection direction)
        {
            DbParameter param = DbParameters.CreateDbParameter();
            param.ParameterName = paramName;
            param.Value = value;
            param.Direction = direction;
            param.DbType = dbType;
            param.Size = dbTypeSize;
            return param;
        }
        /// <summary>
        /// 根据配置文件中所配置的数据库类型
        /// 来创建相应数据库的参数对象
        /// </summary>
        /// <returns></returns>
        public static DbParameter CreateDbParameter(string paramName, object value, DbType dbType)
        {
            DbParameter param = DbParameters.CreateDbParameter();
            param.DbType = dbType;
            param.ParameterName = paramName;
            param.Value = value;
            return param;
        }
        /// <summary>
        /// 转换对应的数据库参数 (通用character?)
        /// </summary>
        /// <param name="dbParameter">参数</param>
        /// <returns></returns>
        public static DbParameter[] ToDbParameter(DbParameter[] dbParameter)
        {
            int i = 0;
            int size = dbParameter.Length;
            DbParameter[] _dbParameter = null;
            switch (DatabaseCommon.DBType)
            {
                case DatabaseType.SqlServer:
                    _dbParameter = new SqlParameter[size];
                    while (i < size)
                    {
                        _dbParameter[i] = new SqlParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                        i++;
                    }
                    break;
                case DatabaseType.MySql:
                    _dbParameter = new MySqlParameter[size];
                    while (i < size)
                    {
                        _dbParameter[i] = new MySqlParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                        i++;
                    }
                    break;
                case DatabaseType.Oracle:
                    _dbParameter = new OracleParameter[size];
                    while (i < size)
                    {
                        _dbParameter[i] = new OracleParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                        i++;
                    }
                    break;
                case DatabaseType.Access:
                    _dbParameter = new OleDbParameter[size];
                    while (i < size)
                    {
                        _dbParameter[i] = new OleDbParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                        i++;
                    }
                    break;
                //case DatabaseType.SQLite:
                //    _dbParameter = new SQLiteParameter[size];
                //    while (i < size)
                //    {
                //        _dbParameter[i] = new SQLiteParameter(dbParameter[i].ParameterName, dbParameter[i].Value);
                //        i++;
                //    }
                //    break;
                default:
                    throw new Exception("数据库类型目前不支持！");
            }
            return _dbParameter;
        }
    }
}

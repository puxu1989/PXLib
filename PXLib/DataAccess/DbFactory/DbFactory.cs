using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using PXLib.DataAccess.Database;
using PXLib.DataAccess.Database.Extension;
using PXLib.ObjectManage.UnityIoc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PXLib.DataAccess.DbFactory
{
    /// <summary>
    /// 数据工厂使用基本入口 给数据仓储调用
    /// </summary>
   public class DbFactory
    {
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="connString">连接字符串</param>
        /// <returns></returns>
        public static IDatabase Base(string connStringName)
        {
            return UnityIocHelper.DBInstance.GetService<IDatabase>(new ParameterOverride[]
			{
				new ParameterOverride("connStringName", connStringName),
				new ParameterOverride("DbType", DatabaseCommon.DBType.ToString())
			});
        }
        /// <summary>
        /// 连接基础库 获取数据库服务
        /// </summary>
        /// <returns></returns>
        public static IDatabase Base()
        {
            //DatabaseCommon.DBType = (DatabaseType)Enum.Parse(typeof(DatabaseType), UnityIocHelper.GetmapToByName("DBcontainer", "IDbContext"));//报错
            return UnityIocHelper.DBInstance.GetService<IDatabase>(new ParameterOverride[]
            {
                new ParameterOverride("connStringName", "DBConnectionSting"),
                new ParameterOverride("DbType",DatabaseCommon.DBType.ToString())//其他类型的数据库入口
            });
        }
    }
}

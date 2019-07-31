using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.DataAccess.DbFactory
{
    /// <summary>
    /// 数据仓库获取工厂
    /// </summary>
   public class RepositoryFactory
    {
        public IRepository BaseRepository(string connStringName)
        {
            return new Repository(DbFactory.Base(connStringName));
        }
        public IRepository BaseRepository()
        {
            return new Repository(DbFactory.Base());
        }
        /// <summary>
        /// 获取数据仓库 =BaseRepository
        /// </summary>
        /// <returns></returns>
        public IRepository GetRepository() 
        {
            return this.BaseRepository();
        }
    }
}

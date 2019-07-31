using PXLib.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.DataAccess.DbFactory
{
    /// <summary>
    /// 调用入口  数据业务逻辑层  或者业务逻辑层继承于该类 调用时可以省去泛型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RepositoryFactory<T> where T : class, new()
    {
        public IRepository<T> BaseRepository(string connStringName)
        {
            return new Repository<T>(DbFactory.Base(connStringName));
        }
        public IRepository<T> BaseRepository()
        {
            return new Repository<T>(DbFactory.Base());
        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage
{
    /// <summary>
    /// IObjectRetriever 对象提取器。比如，智能缓存用于从其它地方获取缓存中不存在的object。    
    /// 从数据库获取对象，也可以从文件获取，甚至是可以从网络获取，或者从其它的服务器获取
    /// 通过IObjectRetriever接口，我们就将“对象的加载”与“对象的管理”两种不同的职能区分开来了
    /// </summary>  
    public interface IObjectRetriever<Tkey, TVal>
    {
        /// <summary>
        /// Retrieve 根据ID获取目标对象。
        /// </summary>
        TVal Retrieve(Tkey id);

        /// <summary>
        /// RetrieveAll 获取所有的对象。
        /// </summary>      
        IDictionary<Tkey, TVal> RetrieveAll();
    }
}

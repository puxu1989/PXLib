using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.ObjectManage.UnityIoc
{
    /// <summary>
    /// UnityContainer的最终目的动态地解析和注入依赖，最终提供（创建新对象或者提供现有对象）一个符合你要求的对象  需要添加Microsoft.Practices.Unity;3个包
    /// 面向接口实现有很多好处，可以提供不同灵活的子类实现，增加代码稳定和健壮性等，但是接口一定是需要实现的，如果一个子类实现换成另一个子类实现，就需要在代码中改动，
    /// 或者建立一个工厂来根据条件生成，还是存着着一定的耦合关系。
    //容器在需要的时候把这个依赖关系形成，即把需要的接口实现注入到需要它的类中，当需要换一个实现子类将会变成很简单（一般这样的对象都是实现于某种接口的），
    //只要修改XML就可以，这样可以实现对象的热插拨（有点象USB接口）。注意看unity节点的配置
    /// </summary>
    public class UnityIocHelper : IServiceProvider
    {
        private readonly IUnityContainer _container;
        private static readonly UnityIocHelper dbinstance = new UnityIocHelper("DBContainer");
        private UnityIocHelper(string containerName)
        {
            _container = new UnityContainer();
            UnityConfigurationSection section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            section.Configure(_container, containerName);
        }
        /// <summary>
        /// 通过配置文件读类型 未通过
        /// </summary>
        public static string GetmapToByName(string containerName, string itype, string name = "")
        {
            try
            {
                UnityConfigurationSection section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
                var _Containers = section.Containers;
                foreach (var _Container in _Containers)
                {
                    if (_Container.Name == containerName)
                    {
                        var _Registrations = _Container.Registrations;
                        foreach (var _Registration in _Registrations)
                        {
                            if (name == "" && string.IsNullOrEmpty(_Registration.Name) && _Registration.TypeName == itype)
                            {
                                return _Registration.MapToName;
                            }
                        }
                        break;
                    }
                }
                return "";
            }
            catch
            {
                throw;
            }
        }
        public static UnityIocHelper DBInstance
        {
            get { return dbinstance; }
        }
        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
        public T GetService<T>()
        {
            return _container.Resolve<T>();
        }
        /// <summary>
        /// 获取数据库服务入口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public T GetService<T>(params ParameterOverride[] obj)
        {
            return _container.Resolve<T>(obj);
        }
        public T GetService<T>(string name, params ParameterOverride[] obj)
        {
            return _container.Resolve<T>(name, obj);
        }
    }
}

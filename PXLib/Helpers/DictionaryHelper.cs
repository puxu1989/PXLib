using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Helpers
{
   public class DictionaryHelper
    {
        /// <summary>
        /// ConvertToDictionary 将集合中符合条件的对象添加到新的字典中。通过func获取object对应的Key
        /// </summary>       
        public static Dictionary<TKey, TObject> ConvertToDictionary<TKey, TObject>(IEnumerable<TObject> source, Func<TObject, TKey> func, Predicate<TObject> predicate)
        {
            Dictionary<TKey, TObject> dictionary = new Dictionary<TKey, TObject>();
            foreach (TObject current in source)
            {
                if (predicate(current))
                {
                    dictionary.Add(func(current), current);
                }
            }
            return dictionary;
        }

        /// <summary>
        /// ConvertToDictionary 将集合中符合条件的对象添加到新的字典中。通过func获取object对应的Key
        /// </summary>  
        public static Dictionary<TKey, TObject> ConvertToDictionary<TKey, TObject>(IEnumerable<TObject> source, Func<TObject, TKey> func)
        {
            return DictionaryHelper.ConvertToDictionary<TKey, TObject>(source, func, (TObject obj) => true);
        }

        /// <summary>
        /// RemoveOneByValue 从字典中删除第一个值与val相等的记录
        /// </summary>      
        public static void RemoveOneByValue<TKey, TValue>(IDictionary<TKey, TValue> dic, TValue val)
            where TKey : class
            where TValue : IEquatable<TValue>
        {
            TKey oneByValue = DictionaryHelper.GetOneByValue<TKey, TValue>(dic, val);
            if (oneByValue != null)
            {
                dic.Remove(oneByValue);
            }
        }

        /// <summary>
        /// GetOneByValue 从字典中找出第一个值与val相等的记录的key
        /// </summary>      
        public static TKey GetOneByValue<TKey, TValue>(IDictionary<TKey, TValue> dic, TValue val)
            where TKey : class
            where TValue : IEquatable<TValue>
        {
            return CollectionHelper.FindFirstSpecification<TKey>(dic.Keys, delegate(TKey cur)
            {
                TValue tValue = dic[cur];
                return tValue.Equals(val);
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Maths
{
    /// <summary>
    /// 算法类
    /// </summary>
    public class Arithmetic<T> where T : IComparable
    {
        /// <summary>
        /// BinarySearcher折半查找 返回的是目标值所在的索引，如果不存在则返回-1
        /// </summary>        
        public static int BinarySearcher(IList<T> list, T value)
        {
            int num = 0;
            int i = list.Count;
            int result;
            while (i >= num)
            {
                int num2 = (num + i) / 2;
                T t = list[num2];
                if (t.CompareTo(value) == 0)
                {
                    result = num2;
                    return result;
                }
                t = list[num2];
                if (t.CompareTo(value) > 0)
                {
                    i = num2 - 1;
                }
                else
                {
                    num = num2 + 1;
                }
            }
            result = -1;
            return result;
        }
    }
}

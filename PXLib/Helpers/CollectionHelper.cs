using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Helpers
{
    public class CollectionHelper
    {
        /// <summary>
        /// Find 从集合中选取符合条件的元素
        /// </summary>       
        public static List<TObject> Find<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate)
        {
            List<TObject> list = new List<TObject>();
            CollectionHelper.ActionOnSpecification<TObject>(source, delegate(TObject ele)
            {
                list.Add(ele);
            }, predicate);
            return list;
        }

        /// <summary>
        /// FindFirstSpecification 返回符合条件的第一个元素
        /// </summary>      
        public static TObject FindFirstSpecification<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate)
        {
            TObject result;
            foreach (TObject current in source)
            {
                if (predicate(current))
                {
                    result = current;
                    return result;
                }
            }
            result = default(TObject);
            return result;
        }

        /// <summary>
        /// ContainsSpecification 集合中是否包含满足predicate条件的元素。
        /// </summary>       
        public static bool ContainsSpecification<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate, out TObject specification)
        {
            specification = default(TObject);
            bool result;
            foreach (TObject current in source)
            {
                if (predicate(current))
                {
                    specification = current;
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }

        /// <summary>
        /// ContainsSpecification 集合中是否包含满足predicate条件的元素。
        /// </summary>       
        public static bool ContainsSpecification<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate)
        {
            TObject tObject;
            return CollectionHelper.ContainsSpecification<TObject>(source, predicate, out tObject);
        }

        /// <summary>
        /// ActionOnSpecification 对集合中满足predicate条件的元素执行action。如果没有条件，predicate传入null。
        /// </summary>       
        public static void ActionOnSpecification<TObject>(IEnumerable<TObject> collection, Action<TObject> action, Predicate<TObject> predicate)
        {
            if (collection != null)
            {
                if (predicate == null)
                {
                    foreach (TObject current in collection)
                    {
                        action(current);
                    }
                }
                else
                {
                    foreach (TObject current in collection)
                    {
                        if (predicate(current))
                        {
                            action(current);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ActionOnEach  对集合中的每个元素执行action。
        /// </summary>        
        public static void ActionOnEach<TObject>(IEnumerable<TObject> collection, Action<TObject> action)
        {
            CollectionHelper.ActionOnSpecification<TObject>(collection, action, null);
        }

        public static T[] GetPart<T>(T[] ary, int startIndex, int count)
        {
            return CollectionHelper.GetPart<T>(ary, startIndex, count, false);
        }

        public static T[] GetPart<T>(T[] ary, int startIndex, int count, bool reverse)
        {
            T[] result;
            if (startIndex >= ary.Length)
            {
                result = null;
            }
            else
            {
                if (ary.Length < startIndex + count)
                {
                    count = ary.Length - startIndex;
                }
                T[] array = new T[count];
                if (!reverse)
                {
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = ary[startIndex + i];
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = ary[ary.Length - startIndex - 1 - i];
                    }
                }
                result = array;
            }
            return result;
        }

        /// <summary>
        /// BinarySearch 从已排序的列表中，采用二分查找找到目标在列表中的位置。
        /// 如果刚好有个元素与目标相等，则返回true，且minIndex会被赋予该元素的位置；否则，返回false，且minIndex会被赋予比目标小且最接近目标的元素的位置
        /// </summary>       
        public static bool BinarySearch<T>(IList<T> sortedList, T target, out int minIndex) where T : IComparable
        {
            bool result;
            if (target.CompareTo(sortedList[0]) == 0)
            {
                minIndex = 0;
                result = true;
            }
            else if (target.CompareTo(sortedList[0]) < 0)
            {
                minIndex = -1;
                result = false;
            }
            else if (target.CompareTo(sortedList[sortedList.Count - 1]) == 0)
            {
                minIndex = sortedList.Count - 1;
                result = true;
            }
            else if (target.CompareTo(sortedList[sortedList.Count - 1]) > 0)
            {
                minIndex = sortedList.Count - 1;
                result = false;
            }
            else
            {
                int num = 0;
                int num2 = sortedList.Count - 1;
                while (num2 - num > 1)
                {
                    int num3 = (num + num2) / 2;
                    if (target.CompareTo(sortedList[num3]) == 0)
                    {
                        minIndex = num3;
                        result = true;
                        return result;
                    }
                    if (target.CompareTo(sortedList[num3]) < 0)
                    {
                        num2 = num3;
                    }
                    else
                    {
                        num = num3;
                    }
                }
                minIndex = num;
                result = false;
            }
            return result;
        }

        /// <summary>
        /// GetIntersection 高效地求两个List元素的交集。
        /// </summary>        
        public static List<T> GetIntersection<T>(List<T> list1, List<T> list2) where T : IComparable
        {
            List<T> list3 = (list1.Count > list2.Count) ? list1 : list2;
            List<T> list4 = (list3 == list1) ? list2 : list1;
            list3.Sort();
            int num = 0;
            List<T> list5 = new List<T>();
            foreach (T current in list4)
            {
                if (CollectionHelper.BinarySearch<T>(list3, current, out num))
                {
                    list5.Add(current);
                }
            }
            return list5;
        }

        /// <summary>
        /// GetUnion 高效地求两个List元素的并集。
        /// </summary> 
        public static List<T> GetUnion<T>(List<T> list1, List<T> list2)
        {
            SortedDictionary<T, int> sortedDictionary = new SortedDictionary<T, int>();
            foreach (T current in list1)
            {
                if (!sortedDictionary.ContainsKey(current))
                {
                    sortedDictionary.Add(current, 0);
                }
            }
            foreach (T current in list2)
            {
                if (!sortedDictionary.ContainsKey(current))
                {
                    sortedDictionary.Add(current, 0);
                }
            }
            return CopyAllToList<T>(sortedDictionary.Keys);
        }
        /// <summary>
        /// --------------------------------------------------转换----------------------------------------------------
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<TObject> CopyAllToList<TObject>(IEnumerable<TObject> source)
        {
            List<TObject> list = new List<TObject>();
            CollectionHelper.ActionOnEach<TObject>(source, delegate(TObject t)//对source进行foreache 这里的delegete 是Action返回的结果t
            {
                list.Add(t);
            });
            return list;
        }
        /// <summary>
        /// ConvertAll 将source中的每个元素转换为TResult类型
        /// </summary>       
        public static List<TResult> ConvertAll<TObject, TResult>(IEnumerable<TObject> source, Func<TObject, TResult> converter)
        {
            return ConvertSpecification<TObject, TResult>(source, converter, null);
        }
        /// <summary>
        /// ConvertSpecification 将source中的符合predicate条件元素转换为TResult类型
        /// </summary>       
        public static List<TResult> ConvertSpecification<TObject, TResult>(IEnumerable<TObject> source, Func<TObject, TResult> converter, Predicate<TObject> predicate)
        {
            List<TResult> list = new List<TResult>();
            CollectionHelper.ActionOnSpecification<TObject>(source, delegate(TObject ele)
            {
                list.Add(converter(ele));
            }, predicate);
            return list;
        }

    }
}

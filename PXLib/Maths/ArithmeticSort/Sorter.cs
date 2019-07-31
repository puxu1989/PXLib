using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Maths
{
    /// <summary>
    /// 排序
    /// </summary>    
    public static class Sorter<T> where T : IComparable //IComparable接口 可以比较的对象相同
    {
        #region  HeapSort堆排序
        public static void HeapSort(IList<T> list, bool isAsc)
        {
            for (int i = list.Count / 2 - 1; i >= 0; i--)
            {
                HeapSortAdjust(list, i, list.Count - 1, isAsc);
            }
            for (int i = list.Count - 1; i > 0; i--)
            {
                SortHelper<T>.Swap(list, 0, i);
                HeapSortAdjust(list, 0, i - 1, isAsc);
            }
        }

        private static void HeapSortAdjust(IList<T> list, int nodeIndx, int maxAdjustIndx, bool isAsc)
        {
            T t = list[nodeIndx];
            T value = list[nodeIndx];
            int i = 2 * nodeIndx + 1;
            while (i <= maxAdjustIndx)
            {
                if (isAsc)
                {
                    bool arg_5A_0;
                    if (i < maxAdjustIndx)
                    {
                        T t2 = list[i];
                        arg_5A_0 = (t2.CompareTo(list[i + 1]) >= 0);
                    }
                    else
                    {
                        arg_5A_0 = true;
                    }
                    if (!arg_5A_0)
                    {
                        i++;
                    }
                    if (t.CompareTo(list[i]) > 0)
                    {
                        break;
                    }
                    list[(i - 1) / 2] = list[i];
                    i = 2 * i + 1;
                }
                else
                {
                    bool arg_DD_0;
                    if (i < maxAdjustIndx)
                    {
                        T t2 = list[i];
                        arg_DD_0 = (t2.CompareTo(list[i + 1]) <= 0);
                    }
                    else
                    {
                        arg_DD_0 = true;
                    }
                    if (!arg_DD_0)
                    {
                        i++;
                    }
                    if (t.CompareTo(list[i]) < 0)
                    {
                        break;
                    }
                    list[(i - 1) / 2] = list[i];
                    i = 2 * i + 1;
                }
            }
            list[(i - 1) / 2] = value;
        }
        #endregion
        #region InsertionSort 插入排序
        public static void InsertionSort(IList<T> list, bool isAsc)
        {
            for (int i = 1; i < list.Count; i++)
            {
                T t = list[i];
                int num;
                if (isAsc)
                {
                    num = i - 1;
                    while (true)
                    {
                        bool arg_5D_0;
                        if (num >= 0)
                        {
                            T t2 = list[num];
                            arg_5D_0 = (t2.CompareTo(t) > 0);
                        }
                        else
                        {
                            arg_5D_0 = false;
                        }
                        if (!arg_5D_0)
                        {
                            break;
                        }
                        list[num + 1] = list[num];
                        num--;
                    }
                }
                else
                {
                    num = i - 1;
                    while (true)
                    {
                        bool arg_A8_0;
                        if (num >= 0)
                        {
                            T t2 = list[num];
                            arg_A8_0 = (t2.CompareTo(t) < 0);
                        }
                        else
                        {
                            arg_A8_0 = false;
                        }
                        if (!arg_A8_0)
                        {
                            break;
                        }
                        list[num + 1] = list[num];
                        num--;
                    }
                }
                list[num + 1] = t;
            }
        }
        #endregion
        #region MergeSort 归并排序
        public static void MergeSort(IList<T> list, bool isAsc)
        {
            int i = 1;
            IList<T> list2 = new List<T>(list.Count);
            foreach (T current in list)
            {
                list2.Add(current);
            }
            while (i < list.Count)
            {
                MergePass(list, list2, i, isAsc);
                i *= 2;
                MergePass(list2, list, i, isAsc);
                i *= 2;
            }
        }
        private static void MergePass(IList<T> list, IList<T> sortedList, int length, bool isASC)
        {
            int i;
            for (i = 0; i <= list.Count - 2 * length; i += 2 * length)
            {
                MergeSort(list, i, i + length - 1, i + 2 * length - 1, sortedList, isASC);
            }
            if (i + length < list.Count)
            {
                MergeSort(list, i, i + length - 1, list.Count - 1, sortedList, isASC);
            }
            else
            {
                for (int j = i; j < list.Count; j++)
                {
                    sortedList[j] = list[j];
                }
            }
        }

        /// <summary>
        /// 归并两个已经排序的子序列为一个有序的序列
        /// </summary>
        /// <param name="list">原始序列</param>
        /// <param name="startIndx">归并起始index</param>
        /// <param name="splitIndx">第一段结束的index</param>
        /// <param name="endIndx">第二段结束的index</param>
        /// <param name="sortedList">排序后的序列</param>
        /// <param name="isASC">是否升序</param>
        private static void MergeSort(IList<T> list, int startIndx, int splitIndx, int endIndx, IList<T> sortedList, bool isASC)
        {
            int num = splitIndx + 1;
            int num2 = startIndx;
            int num3 = startIndx;
            while (num2 <= splitIndx && num <= endIndx)
            {
                if (isASC)
                {
                    T t = list[num2];
                    if (t.CompareTo(list[num]) <= 0)
                    {
                        sortedList[num3++] = list[num2++];
                    }
                    else
                    {
                        sortedList[num3++] = list[num++];
                    }
                }
                else
                {
                    T t = list[num2];
                    if (t.CompareTo(list[num]) >= 0)
                    {
                        sortedList[num3++] = list[num2++];
                    }
                    else
                    {
                        sortedList[num3++] = list[num++];
                    }
                }
            }
            if (num2 > splitIndx)
            {
                for (int i = num; i <= endIndx; i++)
                {
                    sortedList[num3 + i - num] = list[i];
                }
            }
            else
            {
                for (int i = num2; i <= splitIndx; i++)
                {
                    sortedList[num3 + i - num2] = list[i];
                }
            }
        }
        #endregion
        #region QuickSort 快速排序
        public static void QuickSort(IList<T> list, bool isAsc)
        {
            QuickSort(list, 0, list.Count - 1, isAsc);
        }
        private static void QuickSort(IList<T> list, int left, int right, bool isAsc)
        {
            if (left < right)
            {
                int i = left;
                int num = right + 1;
                if (isAsc)
                {
                    while (i < num)
                    {
                        i++;
                        T t = list[left];
                        while (true)
                        {
                            bool arg_62_0;
                            if (i < right)
                            {
                                T t2 = list[i];
                                arg_62_0 = (t2.CompareTo(t) < 0);
                            }
                            else
                            {
                                arg_62_0 = false;
                            }
                            if (!arg_62_0)
                            {
                                break;
                            }
                            i++;
                        }
                        num--;
                        while (true)
                        {
                            bool arg_98_0;
                            if (num >= left)
                            {
                                T t2 = list[num];
                                arg_98_0 = (t2.CompareTo(t) > 0);
                            }
                            else
                            {
                                arg_98_0 = false;
                            }
                            if (!arg_98_0)
                            {
                                break;
                            }
                            num--;
                        }
                        if (i < num)
                        {
                            SortHelper<T>.Swap(list, i, num);
                        }
                    }
                }
                else
                {
                    while (i < num)
                    {
                        i++;
                        T t = list[left];
                        while (true)
                        {
                            bool arg_105_0;
                            if (i < right)
                            {
                                T t2 = list[i];
                                arg_105_0 = (t2.CompareTo(t) > 0);
                            }
                            else
                            {
                                arg_105_0 = false;
                            }
                            if (!arg_105_0)
                            {
                                break;
                            }
                            i++;
                        }
                        num--;
                        while (true)
                        {
                            bool arg_13B_0;
                            if (num >= left)
                            {
                                T t2 = list[num];
                                arg_13B_0 = (t2.CompareTo(t) < 0);
                            }
                            else
                            {
                                arg_13B_0 = false;
                            }
                            if (!arg_13B_0)
                            {
                                break;
                            }
                            num--;
                        }
                        if (i < num)
                        {
                            SortHelper<T>.Swap(list, i, num);
                        }
                    }
                }
                SortHelper<T>.Swap(list, left, num);
                QuickSort(list, left, num - 1, isAsc);
                QuickSort(list, num + 1, right, isAsc);
            }

        }
        #endregion
    }
}

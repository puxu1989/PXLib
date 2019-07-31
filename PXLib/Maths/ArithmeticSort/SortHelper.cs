using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Maths
{
   public class SortHelper<T>
    {
       public static void Swap(IList<T> list, int i, int j)
       {
           T value = list[i];
           list[i] = list[j];
           list[j] = value;
       }
       /// 对List进行随机排序  
       /// </summary>  
       /// <param name="ListT"></param>  
       /// <returns></returns>  
       //public static List<T> RandomSortList<T>(List<T> ListT)
       //{
       //    Random random = new Random();
       //    List<T> newList = new List<T>();
       //    foreach (T item in ListT)
       //    {
       //        Console.WriteLine("Index--" + random.Next(newList.Count));
       //        newList.Insert(random.Next(newList.Count), item);
       //    }
       //    return newList;
       //}
       public static List<T> RandomSortList<T>(List<T> inputList)
       {
           //Copy to a array
           T[] copyArray = new T[inputList.Count];
           inputList.CopyTo(copyArray);

           //Add range
           List<T> copyList = new List<T>();
           copyList.AddRange(copyArray);

           //Set outputList and random
           List<T> outputList = new List<T>();
           Random rd = new Random(DateTime.Now.Millisecond);

           while (copyList.Count > 0)
           {
               //Select an index and item
               int rdIndex = rd.Next(0, copyList.Count);
               T remove = copyList[rdIndex];

               //remove it from copyList and add it to output
               copyList.Remove(remove);
               outputList.Add(remove);
           }
           copyList.Clear();
           copyList.AddRange(outputList);
           outputList.Clear();
           while (copyList.Count > 0)
           {
               //Select an index and item
               int rdIndex = rd.Next(0, copyList.Count);
               T remove = copyList[rdIndex];

               //remove it from copyList and add it to output
               copyList.Remove(remove);
               outputList.Add(remove);
           }

           return outputList;
       }

    }
}

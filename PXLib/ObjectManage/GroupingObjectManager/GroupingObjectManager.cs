using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage.GroupingObjectManager
{
    /// <summary>
    /// GroupingObjectManager的实现是线程安全的，所以可以在多线程的环境中使用。我们对其内部的两个字典集合都进行了加锁控制。
    /// 只要一个对象有唯一的ID，并且有分组的标志，那么这个对象就可以被对象分组管理器进行管理了。
    ///适用场合（1）被管理的每个对象都有唯一的ID。
    ///（2）被管理的对象可以依据某个标志进行分组。
    ///（3）经常需要根据分组标志来查询符合该标志的对象列表。
    ///（4）经常需要向管理器中增加/移除被分组的对象。
    ///（5）经常需要根据对象ID快速查找对应的对象。
    /// </summary>
    public class GroupingObjectManager<TGroupKey, TObjectKey, TObject>  where TObject : IGroupingObject<TGroupKey, TObjectKey>
    {
        private IDictionary<TGroupKey, IDictionary<TObjectKey, TObject>> groupDictionary = new Dictionary<TGroupKey, IDictionary<TObjectKey, TObject>>();

        private IDictionary<TObjectKey, TObject> objectDictionary = new Dictionary<TObjectKey, TObject>();

        private object locker = new object();
        public int TotalObjectCount
        {
            get
            {
                return this.objectDictionary.Count;
            }
        }
        public int TotalGroupCount
        {
            get
            {
                return this.groupDictionary.Count;
            }
        }
        /// <summary>
        /// 添加 存在就覆盖
        /// </summary>
        /// <param name="obj"></param>
        public void Add(TObject obj)
        {
            lock (this.locker)
            {
                if (!this.groupDictionary.ContainsKey(obj.GroupID))
                {
                    this.groupDictionary.Add(obj.GroupID, new Dictionary<TObjectKey, TObject>());
                }
                if (this.groupDictionary[obj.GroupID].ContainsKey(obj.ID))
                {
                    this.groupDictionary[obj.GroupID].Remove(obj.ID);
                }
                this.groupDictionary[obj.GroupID].Add(obj.ID, obj);
                if (this.objectDictionary.ContainsKey(obj.ID))
                {
                    this.objectDictionary.Remove(obj.ID);
                }
                this.objectDictionary.Add(obj.ID, obj);
            }
        }

        public void Remove(TObjectKey objectID)
        {
            lock (this.locker)
            {
                if (this.objectDictionary.ContainsKey(objectID))
                {
                    TObject tObject = this.objectDictionary[objectID];
                    this.objectDictionary.Remove(objectID);
                    if (this.groupDictionary[tObject.GroupID].ContainsKey(objectID))
                    {
                        this.groupDictionary[tObject.GroupID].Remove(objectID);
                        if (this.groupDictionary[tObject.GroupID].Count == 0)
                        {
                            this.groupDictionary.Remove(tObject.GroupID);
                        }
                    }
                }
            }
        }

        public IList<TObject> GetObjectsCopy(TGroupKey groupID)
        {
            IList<TObject> result;
            lock (this.locker)
            {
                if (!this.groupDictionary.ContainsKey(groupID))
                {
                    result = new List<TObject>();
                }
                else
                {
                    result = CollectionHelper.CopyAllToList<TObject>(this.groupDictionary[groupID].Values);
                }
            }
            return result;
        }

        public IList<TObject> GetAllObjectsCopy()
        {
            IList<TObject> result;
            lock (this.locker)
            {
                result = CollectionHelper.CopyAllToList<TObject>(this.objectDictionary.Values);
            }
            return result;
        }

        public IList<TGroupKey> GetGroupsCopy()
        {
            IList<TGroupKey> result;
            lock (this.locker)
            {
                result = CollectionHelper.CopyAllToList<TGroupKey>(this.groupDictionary.Keys);
            }
            return result;
        }

        public TObject Get(TObjectKey objectID)
        {
            TObject result;
            lock (this.locker)
            {
                if (this.objectDictionary.ContainsKey(objectID))
                {
                    result = this.objectDictionary[objectID];
                }
                else
                {
                    result = default(TObject);
                }
            }
            return result;
        }

        public int GetCountOfGroup(TGroupKey groupID)
        {
            int result;
            lock (this.locker)
            {
                if (!this.groupDictionary.ContainsKey(groupID))
                {
                    result = 0;
                }
                else
                {
                    result = this.groupDictionary[groupID].Count;
                }
            }
            return result;
        }

        public void Clear()
        {
            lock (this.locker)
            {
                this.groupDictionary.Clear();
                this.objectDictionary.Clear();
            }
        }
    }
}

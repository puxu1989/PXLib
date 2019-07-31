using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.ObjectManage
{
    // <summary>
    /// Circle 圈结构。非线程安全。使用案例：在跳棋游戏中，当一个人走一步棋之后，控制权就轮到下一家，如此轮询，一圈之后控制权又回到自己，然后再继续轮圈下去
    /// </summary>
    /// <typeparam name="T">圈的每个节点存储的对象的类型</typeparam>
    public class Circle<T>
    {
        private IList<T> list = new List<T>();

        private int currentPosition = 0;
        /// <summary>
        /// 头
        /// </summary>
        public T Header
        {
            get
            {
                T result;
                if (this.list.Count == 0)
                {
                    result = default(T);
                }
                else
                {
                    result = this.list[0];
                }
                return result;
            }
        }
        
        /// <summary>
        /// 尾
        /// </summary>
        public T Tail
        {
            get
            {
                T result;
                if (this.list.Count == 0)
                {
                    result = default(T);
                }
                else
                {
                    result = this.list[this.list.Count - 1];
                }
                return result;
            }
        }
        /// <summary>
        /// 条数
        /// </summary>
        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }
        /// <summary>
        /// 单前对象 默认第一个 否则就是postion指定的那个
        /// </summary>
        public T Current
        {
            get
            {
                T result;
                if (this.list.Count == 0)
                {
                    result = default(T);
                }
                else
                {
                    result = this.list[this.currentPosition];
                }
                return result;
            }
        }
        #region 构造
        public Circle()
        {
        }

        public Circle(IList<T> _list)
        {
            if (_list != null)
            {
                this.list = _list;
            }
        }
        #endregion
        public void MoveNext()
        {
            if (this.list.Count != 0)
            {
                this.currentPosition = (this.currentPosition + 1) % this.list.Count;
            }
        }

        public void MoveBack()
        {
            if (this.list.Count != 0)
            {
                this.currentPosition = (this.currentPosition + this.list.Count - 1) % this.list.Count;
            }
        }
        /// <summary>
        /// 获取下一个对象
        /// </summary>
        /// <returns></returns>
        public T PeekNext()
        {
            this.MoveNext();
            T current = this.Current;
            this.MoveBack();
            return current;
        }
        /// <summary>
        /// 获取上一个对象
        /// </summary>
        /// <returns></returns>
        public T PeekBack()
        {
            this.MoveBack();
            T current = this.Current;
            this.MoveNext();
            return current;
        }
        /// <summary>
        /// 将控制权交给T val对象 如果该对象不存在则不进行任何操作
        /// </summary>
        /// <param name="val"></param>
        public void SetCurrent(T val)
        {
            T t = this.Current;
            if (!t.Equals(val))
            {
                for (int i = 0; i < this.list.Count; i++)
                {
                    t = this.list[i];
                    if (t.Equals(val))
                    {
                        this.currentPosition = i;
                        break;
                    }
                }
            }
        }

        public void Append(T obj)
        {
            this.list.Add(obj);
        }

        public void InsertAt(T obj, int postionIndex)
        {
            if (this.list.Count == 0)
            {
                this.list.Add(obj);
            }
            else
            {
                int num = postionIndex % this.list.Count;
                this.list.Insert(num, obj);
                if (num <= this.currentPosition)
                {
                    this.currentPosition++;
                }
            }
        }

        public void RemoveTail()
        {
            if (this.list.Count != 0)
            {
                this.RemoveAt(this.list.Count - 1);
            }
        }

        public void RemoveAt(int postionIndex)
        {
            if (this.list.Count != 0)
            {
                int num = postionIndex % this.list.Count;
                this.list.RemoveAt(num);
                if (num < this.currentPosition)
                {
                    this.currentPosition--;
                }
            }
        }
    }
}

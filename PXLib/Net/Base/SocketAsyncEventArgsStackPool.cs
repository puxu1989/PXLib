using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace PXLib.Net
{
    public class SocketAsyncEventArgsStackPool
    {
        private  Stack<SocketAsyncEventArgs> stackPool;//装入SocketAsyncEventArgs的Stack<T>
        //构造函数初始空间
        public SocketAsyncEventArgsStackPool(int capacity)
        {
            stackPool = new Stack<SocketAsyncEventArgs>(capacity);
        }
        /// <summary>
        /// 添加元素到队列
        /// </summary>
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) return;
            lock (stackPool)
            {
                stackPool.Push(item);
            }
        }
        /// <summary>
        /// 从池里取出一个SocketAsyncEventArgs对象并删除该对象
        /// </summary>
        /// <returns>要被从池里删除的对象</returns>
        public SocketAsyncEventArgs Pop()
        {
            lock (stackPool)
            {
                return stackPool.Pop();
            }
        }
        public int Count
        {
            get { return stackPool.Count; }
        }
        //移除所有对象
        public void Cleal()
        {
            stackPool.Clear();
        }
    }
}

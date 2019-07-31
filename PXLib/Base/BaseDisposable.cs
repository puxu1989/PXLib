using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace PXLib
{
    /// <summary>
    /// 对象显示回收基类 当我们自定义的类含有非托管资源，就需要实现IDisposable接口，实现对这些资源对象的垃圾回收。
    /// </summary>
    public abstract class BaseDisposable : IDisposable
    {
        protected int _isDisposed;//标识是否已经释放对象
        /// <summary>
        /// 由垃圾回收器调用  在垃圾收前执行其他清理操作
        /// </summary>
        ~BaseDisposable()
        {
            this.Dispose(false);
        }
        /// <summary>
        /// IDisposable的接口 由类的使用者，在外部显示调用，释放类资源
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);//这里如果子类被重写就先调用子类的 然后再调用调用Base的virtual释放
          
        }
        protected virtual void Dispose(bool disposing)//子类一定要调用base.Dispose()或者写到上面的Dispose里，这里不做任何操作
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _isDisposed, 1);
                GC.SuppressFinalize(this);
            } 
        }
        /// <summary>
        /// 检查对象是否已被显示释放了
        /// </summary>
        protected void CheckDisposed()
        {
            if (_isDisposed == 1)
            {
                throw new Exception(string.Format("The {0} object has be disposed.", this.GetType().Name));
            }
        }
      
        /*
        托管资源指的是.NET可以自动进行回收的资源，主要是指托管堆上分配的内存资源。托管资源的回收工作是不需要人工干预的，有.NET运行库在合适调用垃圾回收器进行回收。
         非托管资源指的是.NET不知道如何回收的资源，最常见的一类非托管资源是包装操作系统资源的对象，例如文件，窗口，网络连接，数据库连接，画刷，图标等。这类资源，垃圾回收器在清理的时候会调用Object.Finalize()方法。默认情况下，方法是空的，对于非托管对象，需要在此方法中编写回收非托管资源的代码，以便垃圾回收器正确回收资源。
         在.NET中，Object.Finalize()方法是无法重载的，编译器是根据类的析构函数来自动生成Object.Finalize()方法的，所以对于包含非托管资源的类，可以将释放非托管资源的代码放在析构函数。
         注意，不能在析构函数中释放托管资源，因为析构函数是有垃圾回收器调用的，可能在析构函数调用之前，类包含的托管资源已经被回收了，从而导致无法预知的结果。
         本来如果按照上面做法，非托管资源也能够由垃圾回收器进行回收，但是非托管资源一般是有限的，比较宝贵的，而垃圾回收器是由CRL自动调用的，这样就无法保证及时的释放掉非托管资源，因此定义了一个Dispose()方法，让使用者能够手动的释放非托管资源。Dispose()方法释放类的托管资源和非托管资源，使用者手动调用此方法后，垃圾回收器不会对此类实例再次进行回收。Dispose()方法是由使用者调用的，在调用时，类的托管资源和非托管资源肯定都未被回收，所以可以同时回收两种资源。
         Microsoft为非托管资源的回收专门定义了一个接口：IDisposable，接口中只包含一个Dispose()方法。任何包含非托管资源的类，都应该继承此接口。
         在一个包含非托管资源的类中，关于资源释放的标准做法是：
         （1）     继承IDisposable接口；
         （2）     实现Dispose()方法，在其中释放托管资源和非托管资源，并将对象本身从垃圾回收器中移除（垃圾回收器不在回收此资源）；
         （3）     实现类析构函数，在其中释放非托管资源。
         在使用时，显示调用Dispose()方法，可以及时的释放资源，同时通过移除Finalize()方法的执行，提高了性能；如果没有显示调用Dispose()方法，垃圾回收器也可以通过析构函数来释放非托管资源，垃圾回收器本身就具有回收托管资源的功能，从而保证资源的正常释放，只不过由垃圾回收器回收会导致非托管资源的未及时释放的浪费。
         在.NET中应该尽可能的少用析构函数释放资源。在没有析构函数的对象在垃圾处理器一次处理中从内存删除，但有析构函数的对象，需要两次，第一次调用析构函数，第二次删除对象。而且在析构函数中包含大量的释放资源代码，会降低垃圾回收器的工作效率，影响性能。所以对于包含非托管资源的对象，最好及时的调用Dispose()方法来回收资源，而不是依赖垃圾回收器。
         上面就是.NET中对包含非托管资源的类的资源释放机制，只要按照上面要求的步骤编写代码，类就属于资源安全的类。
         析构函数只能由垃圾回收器调用。
         Despose()方法只能由类的使用者调用。
         在C#中，凡是继承了IDisposable接口的类，都可以使用using语句，从而在超出作用域后，让系统自动调用Dispose()方法。
         一个资源安全的类，都实现了IDisposable接口和析构函数。提供手动释放资源和系统自动释放资源的双保险。
        */

    }
}

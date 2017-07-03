C#类成员变量初始化与构造函数执行的顺序（类成员变量初始化先执行，后执行构造函数）
子类静态成员变量初始化
子类成员变量初始化
父类静态成员变量初始化 
父类成员变量初始化 
父类构造函数 
子类构造函数

尾递归：如果一个函数中所有递归形式的调用都出现在函数的末尾，我们称这个递归函数是尾递归的。
		当递归调用是整个函数体中最后执行的语句且它的返回值不属于表达式的一部分时，这个递归调用就是尾递归。
解析：  尾递归就是从最后开始计算, 每递归一次就算出相应的结果, 也就是说, 函数调用出现在调用者函数的尾部,
		因为是尾部, 所以根本没有必要去保存任何局部变量. 直接让被调用的函数返回时越过调用者, 返回到调用者的调用者去.

.NET 中，编译器直接支持的数据类型称为基元类型（primitive type).
基元类型和.NET框架类型（FCL)中的类型有直接的映射关系，例如：在C#中，int直接映射为System.Int32类型。
编译器直接支持的类型。
sbyte / byte / short / ushort /int / uint / long / ulong
char / float / double / bool

.NET是微软的一个框架，.NET是一个平台，一个抽象的平台的概念。.NET平台其本身实现的方式其实还是库，抽象层面上来看是一个平台
。C#是微软创造的语言，C#是一个程序设计语言，仅仅是一个语言那时候可以说就是为.NET设计的,但是微软的.NET事实上还支持VB...
也有开源的C#版本，叫Mono，Linux和Mac上用，也带一个.NET实现
Mono跨平台基于Mono VM： http://www.cnblogs.com/murongxiaopifu/p/4211964.html

托管资源指的是.NET可以自动进行回收的资源，一般是指被CLR控制的内存资源;
主要是指托管堆上分配的内存资源,例如程序中分配的对象，作用域内的变量等
托管资源的回收工作是不需要人工干预的，有.NET运行库在合适调用垃圾回收器进行回收。

非托管资源指的是.NET不知道如何回收的资源，这类资源一般不存在于Heap（堆，内存中用于存储对象实例的地方）中。 
最常见的一类非托管资源是包装操作系统资源的对象，例如文件，窗口，网络连接，数据库连接，画刷，图标等。
这类资源，垃圾回收器在清理的时候会调用Object.Finalize()方法。

通过使用一种语言构造,如使用using语句，从而在超出作用域后，让系统自动调用Dispose()方法,释放类的托管资源和非托管资源
例：using (var enumeratorData=sceneDataDic.GetEnumerator())
	{
		while (enumeratorData.MoveNext())
		{
		}
	}

属性的方法
public bool IsCompleted { get; private set; }

接口：声明变量
    public interface IAsyncObject
    {
        /// <summary>
        /// 最终加载结果的资源
        /// </summary>
        object AsyncResult { get; }
	}
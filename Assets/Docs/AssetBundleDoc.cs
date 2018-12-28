/*  
 *  AssetBundle原理
 *  https://blog.csdn.net/lodypig/article/details/51863683
 *  
 *  序列化：将数据(变量)从内存中变成可存储或传输的过程；序列化之后就可以把序列化的内容写入磁盘，或者通过网络传输到别的机器。如转化为二进制就是一种序列化或者通过Protobuffer、XML、Json
 *  反序列化：把数据（变量内容）从序列化的对象重新读到内存里称。
 *  
 *  AssetBundle即资源包，是Unity Pro提供和推荐的资源打包方式，它可以把多个自定义的游戏对象或者资源以二进制形式保存到Assetbundle文件中。
 *  Assetbundle支持所有unity可识别的格式：模型、贴图、音频、整个场景等，其中最为方便的是可以将关联的内容制作成一个prefab。
 *  例如一个模型的贴图、动作和模型等，然后将整个prefab导出到AssetBundle，Unity会自己收集该Prefab使用到的关联文件，将其一并打入AssetBundle文件，并保留prefab中资源和脚本之间相互关联。
 *  
 *  压缩
 *  
 *  块压缩（chunk-based、LZ4）、流压缩（stream-based、LZMA）两种方式。
 *  流压缩（LZMA）在处理整个数据块时使用同一个字典，它提供了最大可能的压缩率但只支持顺序读取,使用时需要整体解包。优点是打包后体积小，缺点是解包时间长，且占用内存。
 *  块压缩（LZ4）指的是原始数据被分成大小相同的子块并单独压缩。如果你想要实时解压/随机读取开销小，则应该使用这种。
 *  注:LZMA压缩方式的优点在于使用同一个字典压缩率较高，但只能顺序读取意味着加载任意一个资源时，都需要将整个AssetBundle解压，造成卡顿和额外内存占用。
 *     LZ4基于块压缩率较低（测试LZMA换LZ4：86.9M -> 108M），但只需解压需要的块即可，不会有大的卡顿和额外内存占用。后面会详细对比两种压缩方式。
 * 
 * AssetBundle 内存
 * https://blog.csdn.net/lodypig/article/details/51879702
 * 
 * 1. www类本身占用内存，通过www接口加载AssetBundle才会有这部分内存，www对象保留一份对WebStream的引用，
 * 使用www =null 或者 www.dispose()释放。其中www.dispose()会立即释放，而www = null会等待垃圾回收。释放www后WebStream的引用计数会相应减一。
 * 
 * 2.WebStream，数据真正的存储区域。当AssetBundle被加载进来这部分内存就分配了，包含3个内容：压缩后的AssetBundle本身、解压后的资源、解压缓冲区（decompression buffer）。
 * www 和 AssetBundle对象，都只是有一个结构指向WebSteam，从而能对外部提供操作真正资源数据的方法。而当www和AssetBundle对象释放时，WebStream的引用计数会相应减少为1。
 * 当WebStream引用计数为0时，系统自动释放；但为了不频繁地开辟和销毁decompression buffer，Unity会至少保留一份decompression buffer.
 * 例如同时加载3个AssetBundle时，系统会生成3个decompression buffer，当解压完成后，系统会销毁两个。
 * 
 * 3.AssetBundle对象，引用WebSteam数据部分，并提供了从WebStream数据加载资源的接口。通过AssetBundle.Unload(bool unloadAllLoadedObjects)释放。
 * 如果调用AssetBundle.Unload(false),将释放AssetBundle对象本身，对WebStream的引用计数减1，可能引起WebStream的释放。我们也就无法再通过接口或依赖关系从该AssetBundle加载资源，但已加载的资源可以正常使用。
 * 如果调用的AssetBundle.Unload(true),释放AssetBundle对象本身，释放WebStream部分，释放所有被加载出来资源。
 * 无论true或false，AssetBundle.Unload()都将释放AssetBundle，释放后调用该AssetBundle对象的任何方法都不会生效，所以这个接口只能被调用一次，不能先调用unload(false)在调用unload(true)
 * 
 * 4.解压后的资源，从AssetBundle加载出来的原始资源，属于WebStream
 * 
 * 5.实例化的资源，我们通过Instantiate创建的GameObject所包含的资源。这些包含的资源有根据类型，与AssetBundle原始资源(WebStream资源部分)有不同的关系。
 * 如Texture、shader资源通常只是使用不会改动，所以仅仅是引用；而对于gameobject完全复制一份；对于Mesh、Materail，则是引用+复制。
 * 
 * 6. AssetBundle加载后会在内存中生成AssetBundle的序列化架构的占用，一般来说远远小于资源本身，除非包含复杂的序列化信息(复杂多层级关系或复杂静态数据的prefab等)
 * 
 * AssetBundle 优化
 * 1. 一方面降低AB的大小、包体大小，另外一方面也能减少AB加载时所占用的内存，从而降低游戏因内存不足崩溃的概率。对AB中包含的资源进行深入分析，可以将无用或者错用的资源误打入到AB中。
 * 
 * 2. 打包基本策略：将公共依赖打入到一个公共AB中，其他的AB依赖于公共AB；统计资源被所有ab引用的次数，被多个ab引用的资源为公共依赖。
 * 使用EditorUtility.CollectDependencies()得到ab依赖的所有资源的路径(包含内置资源)，注：收集到的资源包含了脚本，dll和编辑器资源，这些资源无需打进ab中。
 * 
 * 3. 处理被依赖的外部资源
 * shader，所有的shader合到一个包中，防止冗余
 * material，删除废弃的粒子效果
 * 
 * 4.处理被依赖的内部资源(shader,material,Texture,Sprite)，提取Unity的内置资源，在打ab前进行预处理，再按着外部资源处理，或者通过资源检测，除shader外不用内置资源
 * shader,可以从Unity官网下载，后三种可以通过
 * Object[] UnityAssets = AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra");
 * foreach (var asset in UnityAssets)
 * {
 *   // create asset...
 * }
 * 
 * 5.Unity资源类型分为两种类型：
 * 基本类型：Mesh，Texture，AudioClip，AnimationClip等
 * 复合类型：.unit, .prefab, .material, .asset 4种
 * 
 * 6.在低速存储设备上加载预制体时，读取预制体的序列化数据消耗的时间很容易超过实例化预制体所花费的时间。也就是说，加载操作的开销受到了存储设备I/O时间的限制。
 * 实例化预制体的整体开销中，文件读取时间占了占了很大比重。
 * 对于AssetBundle仍需要进行适当的合包
 * 
 * 7. 分包策略/粒度规划 https://www.jianshu.com/p/5d4145cd900c 待了解
 * 建议：粒度过细，一方面会导致加载IO次数过多，从而增大了硬件设备耗能和发热的压力,资源加载效率过低
 *  
 *  分组策略1：
 *      a.把经常更新的资源放在一个单独的包里面，跟不经常更新的包分离 =》设置标签、配表、目录
 *      b.把需要同时加载的资源放在一个单独的包里 =》依赖资源打包
 *      c.共享资源放在一个包里 =》依赖资源打包
 *      d.需要同时加载的小资源打包成一个包
 *      e.不同资源版本更替，通过后缀区分v1 v2
 *  
 *  分组策略2：
 *      1，逻辑实体分组
            a,一个UI界面或者所有UI界面一个包（这个界面里面的贴图和布局信息一个包）
            b,一个角色或者所有角色一个包（这个角色里面的模型和动画一个包）
            c,所有的场景所共享的部分一个包（包括贴图和模型）
        2，按照类型分组
            所有声音资源打成一个包，所有shader打成一个包，所有模型打成一个包，所有材质打成一个包
        3，按照使用分组
            把在某一时间内使用的所有资源打成一个包。可以按照关卡分，一个关卡所需要的所有资源包括角色、贴图、声音等打成一个包。也可以按照场景分，一个场景所需要的资源一个包
 *  
 *  某项目策略:
 *      1.基于依赖打包
 *      1.将所有Shader合成一个包，独立成包
 *      2.将特定目录下的常用资源打成公共资源包
 *      3.将特定目录的文件采取特定的分包策略打包
 *      4.无分包策略的文件将依据依赖打包策略
 *      
 *  
 *  首先制定美术规范：美术命名规范、以及目录规范，再结合项目实际情况再决定分包策略
 *  粗略思想：
 *      1.基于依赖打包
 *      2.Shader合成一个包，进入游戏加载到内存中，不用调用Warmup，warmup相当于预编译，shader放在Resources下，相当于提前加载到内存中，也是用到现编译
 *      3.将常用资源打到公共资源包(依据引用次数)
 *      4.基于配表、设置不同目录(不同资源类型)的分包策略
 * 
 * */

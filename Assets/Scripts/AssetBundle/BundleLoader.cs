/*
 * AB加载器：BundleLoader
 */
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;

public class BundleLoader
{
    public const string AbPostfix = "";         //ab后缀，
    public const string AbDownloadPath = "";    //下载路径，Application.persistentDataPath +
    public const string AbShippedpath = "";     //包内路径，Application.streamingAssetsPath +


    private class ABInfo
    {
        public const int DELAY_DEL = 30; //延迟删除帧数，Application.targetFrameRate;

        public AssetBundle ab;
        public AssetRefList abDeps; //当前直接依赖的ab列表（引用的依赖资源）
        public int usedFrame;       //标记ab最近使用帧
        public int unuseFrame;      //标记ab停止使用帧,停止使用后（无引用）超过一定帧才卸载ab
        //public int type;            //ab的类型（公共、场景、跨场景）

        public static bool Empty(ABInfo abInfo)
        {
            if (abInfo == null || abInfo.ab == null)
                return true;

            return false;
        }

    }

    private AssetBundleManifest m_abmf;
    //所有的ab
    private HashSet<string> m_abAll = new HashSet<string>();
    //缓存已下载ab文件；File.Exist(涉及到 IO)不适合在性能敏感大量使用
    private HashSet<string> m_abDownload = new HashSet<string>();
    //已加载的ab
    private Dictionary<string, ABInfo> m_abLoaded = new Dictionary<string, ABInfo>();
    //常用ab(这里只标记主ab，因为主ab未释放,ab的依赖会被引用着，也不会释放);缓存一定的ab数量在内存中；LRU策略：最近最少使用的淘汰
    private FreQueue<string> m_abFreQueue = new FreQueue<string>(20);
    //ab的引用管理
    private AssetRefManager m_abRefMgr = new AssetRefManager();
    //记录资源Id映射主ab引用，用于查询并不增加引用。ab加载的资源只约束[主ab]的引用,并不引用[依赖ab]；<从ab中加载的asset的instanceId，主ab的引用计数器>
    private Dictionary<int, AssetRef> m_cacheAsset2abRef = new Dictionary<int, AssetRef>();
    
    //卸载函数用到的变量，防止函数被频繁的调用，产生大量GC
    List<string> m_toUnloadAbs = new List<string>();//准备卸载的ab资源
    List<int> m_invalidRefs = new List<int>();
    int m_maxUnloadCount = 5;//每帧限制卸载的最大数量
    int m_maxUnloadTime = 30;//每帧限制卸载的最大时长(单位：μs)
    Stopwatch m_stopWatch = new Stopwatch(); //秒表计时

    //初始化
    public void Init()
    {
        //缓存下载已下载的资源
        string[] abDownloads = { };
        m_abDownload.CopyTo(abDownloads);

        //m_abmf缓存
        string abmfPath = GetAbPath("");
        AssetBundle ab = AssetBundle.LoadFromFile(abmfPath);
        if (ab == null)
            return;
        m_abmf = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        if(m_abmf)
            m_abAll.CopyTo(m_abmf.GetAllAssetBundles());
    }

    //释放
    public void Release()
    {
        //清理数据
        //释放mainifest ab
    }

    //释放无用的ab资源(在非加载阶段轮询)
    public void UnloadUnused()
    {
        //筛选可卸载ab
        m_toUnloadAbs.Clear();
        foreach (var kv in m_abLoaded)
        {
            var abPath = kv.Key;
            var abInfo = kv.Value;

            //基础ab资源不卸载
            //待写

            //是否被引用
            if (HasRef(abInfo))
            {
                continue;
            }

            //常用队列保护
            if(m_abFreQueue.Contains(abPath))
            {
                continue;
            }

            //ab资源加载后，一定时间保护
            if (Time.frameCount - abInfo.usedFrame < ABInfo.DELAY_DEL)
            {
                continue;
            }

            //ab资源不使用后，一定时间保护
            if(abInfo.unuseFrame == 0)
            {
                abInfo.unuseFrame = Time.frameCount;
                continue;
            }
            if (Time.frameCount - abInfo.unuseFrame < ABInfo.DELAY_DEL)
            {
                continue;
            }

            m_toUnloadAbs.Add(abPath);
        }

        //限制每帧卸载数量（分帧卸载，依据卸载ab的时间、数量）
        //注：分帧卸载其实是在允许一定时间内，加载资源，超过这个时间则等到下一帧卸载
        //1s = 1000ms,30帧每秒的fps才能保证画面渲染流畅，1000/ 30 ≈ 30 ms（用来处理其他逻辑时间）
        //保证cpu的dc时间，需要控制cpu在其他方面的消耗（比如资源加载、资源卸载的时间，通过控制每帧卸载的数量、或加载的数量）
        int unloadedCount = 0;
        m_stopWatch.Reset();
        m_stopWatch.Start();
        foreach (var toUnloadAb in m_toUnloadAbs)
        {
            if (!UnloadAb(toUnloadAb))
            {
                continue;
            }

            unloadedCount++;
            if(unloadedCount > m_maxUnloadCount)
            {
                break;
            }

            if(m_stopWatch.ElapsedMilliseconds > m_maxUnloadTime)
            {
                break;
            }
        }

        //移除无效的记录资源Id映射主ab引用
        m_invalidRefs.Clear();
        foreach(var kv in m_cacheAsset2abRef)
        {
            var info = kv.Value;
            if (kv.Value == null)
                continue;
            if (info.Invalid())
                m_invalidRefs.Add(kv.Key);
        }

        foreach(var instId in m_invalidRefs)
        {
            m_cacheAsset2abRef.Remove(instId);
        }
    }

    //1.1 从ab中加载资源(sync)
    public T LoadObject<T>(string abPath, string assetName) where T : Object
    {
        AssetBundle ab = LoadAb(abPath);
        if (ab == null)
            return null;

        //添加到常用队列
        m_abFreQueue.Use(abPath);

        T asset = ab.LoadAsset<T>(assetName);
        if(asset != null)
        {
            //记录资源id映射ab引用，此处不增加引用计数
            var assetRef = m_abRefMgr.GetOrCreateRef(ab);
            int instId = asset.GetInstanceID();
            m_cacheAsset2abRef.Add(instId, assetRef);

            //上层调用接口，根据需求再定制是否引用
        }

        return asset;
    }

    //1.2 从ab中异步加载资源(同步加载ab，异步从ab中加载资源);待考虑：1上层调用时，异步加载的优先级 2限制每帧加载数量（分帧加载，依据加载ab的时间、数量））
    public IEnumerator LoadObjectAsync<T>(string abPath, string assetName) where T : Object
    {
        AssetBundle ab = LoadAb(abPath);
        if (ab == null)
            yield break;

        m_abFreQueue.Use(abPath);
        AssetBundleRequest req = ab.LoadAssetAsync<T>(assetName);
        yield return req;
        T asset = req.asset as T;
        if (asset != null)
        {
            //记录资源id映射ab引用，此处不增加引用计数
            var assetRef = m_abRefMgr.GetOrCreateRef(ab);
            int instId = asset.GetInstanceID();
            m_cacheAsset2abRef.Add(instId, assetRef);

            //上层调用接口，根据需求再定制是否引用
        }
    }

    //2.加载ab（包括ab的依赖）
    private AssetBundle LoadAb(string abPath)
    {
        ABInfo abInfo = LoadAbRecursive(abPath);
        if(abInfo != null)
        {
            return abInfo.ab;
        }

        return null;
    }

    //3.递归加载所有ab
    private ABInfo LoadAbRecursive(string abPath)
    {
        bool isLoaded = false;
        ABInfo abInfo = LoadAbReal(abPath, out isLoaded);
        //已加载，则直接返回
        if (isLoaded)
            return abInfo;

        var deps = m_abmf.GetDirectDependencies(abPath);
        foreach (var dep in deps)
        {
            var depAbInfo = LoadAbRecursive(dep);
            if(depAbInfo == null)
            {
                //部分资源丢失
                UnityEngine.Debug.LogWarningFormat("dep:{0} is null", dep);
                continue;
            }

            //增加对依赖ab的引用
            var abDepAssetRef = m_abRefMgr.GetOrCreateRef(depAbInfo.ab);
            depAbInfo.abDeps.AddRef(abDepAssetRef);
        }

        return abInfo;
    }

    //4.真正的加载ab或获取已加载的ab
    private ABInfo LoadAbReal(string abPath, out bool isLoaded)
    {
        //已加载，直接返回
        if (m_abLoaded.ContainsKey(abPath))
        {
            var abInfo = m_abLoaded[abPath];
            if(!ABInfo.Empty(abInfo))
            {
                isLoaded = true;
                abInfo.usedFrame = Time.frameCount;
                return abInfo;
            }
        }
        isLoaded = false;
        //未加载，走加载流程
        //过滤无用路径和无效路径
        if (string.IsNullOrEmpty(abPath))
            return null;

        if (!m_abAll.Contains(abPath))
            return null;

        var realPath = GetAbPath(abPath);
        var ab = AssetBundle.LoadFromFile(abPath);
        if(ab != null)
        {
            var abInfo = new ABInfo();
            abInfo.ab = ab;
            abInfo.abDeps = new AssetRefList();
            abInfo.usedFrame = Time.frameCount;

            m_abLoaded.Add(abPath, abInfo);
        }

        return null;
    }

    //由ab的相对路径,优先判断是否为下载路径
    private string GetAbPath(string abPath)
    {
        if (m_abDownload.Contains(abPath))
        {
            return Path.Combine(AbDownloadPath, abPath);
        }

        return Path.Combine(AbShippedpath, abPath);
    }

    //卸载ab
    private bool UnloadAb(string abPath)
    {
        //是否在已加载列表
        if (!m_abLoaded.ContainsKey(abPath))
            return false;

        //是否在常用队列中
        if (m_abFreQueue.Contains(abPath))
            return false;

        ABInfo abinfo = m_abLoaded[abPath];
        if (abinfo == null)
        {
            m_abLoaded.Remove(abPath);
            return false;
        }

        //是否有引用
        if (HasRef(abPath))
            return false;

        //卸载
        m_abLoaded.Remove(abPath);
        abinfo.ab.Unload(true);

        //释放依赖的引用
        abinfo.abDeps.ClearRef();

        return true;
    }

    //ab是否有被引用
    private bool HasRef(string abPath)
    {
        if(!m_abLoaded.ContainsKey(abPath))
            return false;

        return HasRef(m_abLoaded[abPath]);
    }

    //ab是否有被引用
    private bool HasRef(ABInfo abInfo)
    {
        if (abInfo == null)
            return false;

        return HasRef(abInfo.ab);
    }

    //ab是否有被引用
    private bool HasRef(Object asset)
    {
        if (asset == null)
            return false;

        var instId = asset.GetInstanceID();
        return m_abRefMgr.HasRef(instId);
    }

    private AssetRef GetAssetRef(Object asset)
    {
        if (asset == null)
            return null;

        int instId = asset.GetInstanceID();
        if (m_cacheAsset2abRef.ContainsKey(instId))
        {
            var assetRef = m_cacheAsset2abRef[instId];
            if(assetRef != null && !assetRef.Invalid())
            {
                return assetRef;
            }
        }

        return null;
    }
}
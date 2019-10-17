/*
 * 引用计数器:用于对资源的引用计数
 * 创建的引用计数器
 * 1.ab本身就是资源（示例：BundleLoader中m_cacheAsset2abRef就是缓存ab的引用计数器）
 * 2.也可以用于从ab加载的资源，暂时未用到
 * 注：AutoAssetRef表面上是对ab加载的资源的引用（本质是通过ab加载的资源，获取ab的引用计数器，最终也是增加ab的引用计数）
 * 注：AssetCacheManager表面上也是对ab加载的资源的引用（本质是通过ab加载的资源，获取ab的引用计数器，最终也是增加ab的引用计数）
 * 
 * 通过判断ab是否被引用，释放ab。Unload(true),释放ab资源，以及从该ab中加载的资源。
 */
using System;
using System.Collections.Generic;

public class AssetRef
{
    //被引用资源实例id
    public int InstId { get; set; }
    //引用计数
    public int RefCount { get; private set; }

    public AssetRefManager Mgr;

    //引用
    public void Ref()
    {
        RefCount++;
    }

    //去引用
    public void UnRef()
    {
        RefCount--;
        if(RefCount <= 0)
        {
            //去掉该引用
            if(Mgr != null)
            {
                Mgr.DelRef(InstId);
                Mgr = null;
            }
        }
    }

    public bool HasRef()
    {
        return (RefCount > 0);
    }

    public bool Invalid()
    {
        return Mgr == null;
    }

#if UNITY_DEV

    public string AssetName;
    public override string ToString()
    {
        return AssetName;
    }
#endif
}

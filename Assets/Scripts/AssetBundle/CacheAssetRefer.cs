/*
 * CacheAssetRefer :AB主动引用器
 * 与autoAssetRef配合使用，通过对ab加载后的资源引用（本质还是增加ab引用计数），防止ab被卸载
 * AutoAssetRef：只能保证资源被引用时不被卸载，导致有些资源被反复加载、卸载
 * 本类就是，主动对ab加载后的资源引用（本质还是增加ab引用计数），防止ab被卸载
 * weakRefList: 只保存本场景的ab引用，切场景则释放引用
 * strongRefList: 不限制场景，不主动移除，则
 */
using UnityEngine;
using System.Collections.Generic;

public class CacheAssetRefer
{
    public AssetRefList strongRefs = new AssetRefList();
    public AssetRefList weakRefs = new AssetRefList();

    public void CacheAsset(AssetRef assetRef, bool isStrong = false)
    {
        if (!isStrong)
        {
            weakRefs.AddRef(assetRef);
        }
        else
        {
            strongRefs.AddRef(assetRef);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void RemoveCache(AssetRef assetRef)
    {
        weakRefs.DelRef(assetRef);
        strongRefs.DelRef(assetRef);
    }

    /// <summary>
    /// 切场景时调用
    /// </summary>
    public void ClearAllWeak()
    {
        weakRefs.ClearRef();
    }
}

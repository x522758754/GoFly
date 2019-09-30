/*
 * 资源引用管理
 * 1 生成/获取引用计数器
 * 2 移除计数器
 * 3 资源是否有引用
 */
using UnityEngine;
using System.Collections.Generic;

public class AssetRefManager
{
    //aseet的InstanceId，该asset的引用计数器
    Dictionary<int, AssetRef> m_assetRefs = new Dictionary<int, AssetRef>();

    //获取资源的引用计数器，无则创建
    //Object: Base class for all objects Unity can reference
    public AssetRef GetOrCreateRef(Object asset)
    {
        if (asset == null)
            return null;

        int instId = asset.GetInstanceID();
        return GetOrCreateRef(instId);
    }

    public AssetRef GetOrCreateRef(int instId)
    {
        if (m_assetRefs.ContainsKey(instId))
        {
            return m_assetRefs[instId];
        }

        AssetRef ar = new AssetRef()
        {
            InstId = instId,
            Mgr = this,
        };
        m_assetRefs.Add(instId, ar);

        return ar;
    }

    //移除资源的计数器（移除时，应保证资源引用为0）
    public void DelRef(Object asset)
    {
        if (asset == null)
            return;

        int instId = asset.GetInstanceID();
        DelRef(instId);
    }

    public void DelRef(int instId)
    { 
        if (m_assetRefs.ContainsKey(instId))
        {
            m_assetRefs.Remove(instId);
        }
    }

    //资源是否有引用
    public bool HasRef(Object asset)
    {
        if (asset == null)
            return false;

        int instId = asset.GetInstanceID();
        return HasRef(instId);
    }

    public bool HasRef(int instId)
    {
        if (!m_assetRefs.ContainsKey(instId))
            return false;

        var ar = m_assetRefs[instId];
        return ar.HasRef();
    }
}
/*
 * 引用列表
 * 用于记录资源(A)对其他资源(BCD)的引用列表
 * 1.用于维护资源的引用计数
 * 2.不生成引用计数器或者销毁计数器
 */
using UnityEngine;
using System;
using System.Collections.Generic;

public class AssetRefList
{
    Dictionary<int, AssetRef> m_assetRefs = new Dictionary<int, AssetRef>();
    
    public void AddRef(AssetRef assetRef)
    {
        if (assetRef == null)
            return;

        int instId = assetRef.InstId;

        if(!m_assetRefs.ContainsKey(instId))
        {
            m_assetRefs.Add(instId,assetRef);
        }
        assetRef.Ref();
    }

    public void DelRef(AssetRef assetRef)
    {
        if (assetRef == null)
            return;

        int instId = assetRef.InstId;
        if (m_assetRefs.ContainsKey(instId))
        {
            assetRef.UnRef();
            m_assetRefs.Remove(instId);
        }
    }

    public void ClearRef()
    {
        foreach (var kv in m_assetRefs)
        {
            var ar = kv.Value;
            ar.UnRef();
        }

        m_assetRefs.Clear();
    }
}
/*
 * 引用计数器
 * 创建的引用计数器，
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
}

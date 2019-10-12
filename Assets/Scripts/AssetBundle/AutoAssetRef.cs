/*
 * 游戏中动态加载的obj（Texture，Sound，animation等),都需要关联GameObject上
 * GameObject加载时，对引用的Asset的ab增加一次引用，防止关联的ab被卸载
 * 当GameObject销毁时，释放引用
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAssetRef : MonoBehaviour
{
    private AssetRefList assetRefList = new AssetRefList();

    void OnDestroy()
    {
        assetRefList.ClearRef();
    }

    public void AddRef(AssetRef assetRef)
    {
        if (assetRef == null)
        {
            return;
        }
        assetRefList.AddRef(assetRef);//增加引用
    }

    public void RemoveRef(AssetRef assetRef)
    {
        assetRefList.DelRef(assetRef);
    }

    /*
     * 用法示例
     */

    public static AssetRef GetAssetRef(Object asset)
    {
        return null;
    }

    public static void RefAssetWithGameObject(GameObject go, Object asset)
    {
        AssetRef abRef = GetAssetRef(asset);
        if (abRef != null)
        {
            RefAssetWithGameObject(go, abRef);
        }
    }

    public static void RefAssetWithGameObject(GameObject go, AssetRef assetRef)
    {
        if (!assetRef.Invalid())
        {
            //Debug.LogError("Invalid assetRef!" + go.name);
            return;
        }

        AutoAssetRef com = go.GetComponent<AutoAssetRef>();
        if (com == null)
            com = go.AddComponent<AutoAssetRef>();
        com.AddRef(assetRef);
    }
}

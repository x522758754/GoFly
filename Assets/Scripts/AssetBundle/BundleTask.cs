using System;
using UnityEngine;

//T：需要加载的资源
public class BundleTask<T> where T: UnityEngine.Object
{
    public string abPath;

    public T asset;
    public Action<T> Action;

    public void DoActon()
    {
        if(Action != null)
        {
            Action(asset);
        }
    }
}

//T：需要加载的资源
//U: 透传参数
public class BundleTask<T,U> where T : UnityEngine.Object
{
    public string abPath;
    public T asset;
    public U Agr1;

    public Action<T, U> Action;

    public void DoActon()
    {
        if (Action != null)
        {
            Action(asset, Agr1);
        }
    }
}

//T：需要加载的资源
//U,V: 透传参数
public class TaskData<T, U, V> where T : UnityEngine.Object
{
    public string abPath;
    public T asset;
    public U Agr1;
    public V Agr2;

    public Action<T, U, V> Action;

    public void DoActon()
    {
        if (Action != null)
        {
            Action(asset, Agr1, Agr2);
        }
    }
}
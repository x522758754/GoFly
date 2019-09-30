/*
 * 常用队列
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreQueue<T>
{
    List<T> list = null;

    public FreQueue(int size)
    {
        list = new List<T>(size);
    }

    //LRU，淘汰最近最久未使用
    public void Use(T t)
    {
        if(list.Contains(t))
        {
            list.Remove(t);
        }
        else if(list.Count >= list.Capacity)
        {
            list.RemoveAt(list.Count - 1);
        }

        list.Insert(0, t);
    }

    public bool Contains(T t)
    {
        return list.Contains(t);
    }

    void Clear()
    {
        list.Clear();
    }
}

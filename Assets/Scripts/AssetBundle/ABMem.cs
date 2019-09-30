using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABMem<K, V>
{
    Dictionary<K, V> loaded = new Dictionary<K, V>();

    private FreQueue<K> freQueue = null;
    private FreQueue<K> freQueueDep = null;
    

    public ABMem(int frequeueSize)
    {
        freQueue = new FreQueue<K>(frequeueSize);
        freQueueDep = new FreQueue<K>(frequeueSize * 5);
    }

    public void Add(K k, V v)
    {
        loaded.Add(k, v);
    }

    public bool Remove(K k)
    {
        return loaded.Remove(k);
    }

    public bool TryGetValue(K k, out V v)
    {
        return loaded.TryGetValue(k, out v);
    }

    public void AddFreq(K k, bool bDep)
    {
        if (bDep)
            freQueueDep.Use(k);
        freQueue.Use(k);
    }


}

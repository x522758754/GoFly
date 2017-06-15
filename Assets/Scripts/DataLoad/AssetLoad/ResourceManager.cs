/**************************************************
 * Author： clog
 * Date:
 * Description: 资源管理，功能完备性待检验
 * ***********************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Util;


namespace DataLoad
{
    /// <summary>
    /// 资源管理
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        /// <summary>
        /// 每帧加载资源的最大数量
        /// </summary>
        private readonly int _MaxLoadCountPerFrame = 4;

        /// <summary>
        /// 异步加载完成回调
        /// </summary>
        public delegate void OnLoadResAsyncDone(string assetPath, Object assetObject);

        /// <summary>
        /// 在用资源列表
        /// </summary>
        private Dictionary<string, AssetCounter> m_assetUsedList = new Dictionary<string, AssetCounter>();

        /// <summary>
        /// 待移除资源队列
        /// </summary>
        private List<AssetRemover> m_assetRemovingQueue = new List<AssetRemover>();

        /// <summary>
        /// 正在(异步)加载的资源回调列表
        /// </summary>
        private Dictionary<string, OnLoadResAsyncDone> m_callbackLoadingList = new Dictionary<string, OnLoadResAsyncDone>();

        /// <summary>
        /// 等待加载的资源队列
        /// </summary>
        private List<string> m_assetWaitLoadQueue = new List<string>();

        /// <summary>
        /// 正在(异步)加载的资源列表
        /// </summary>
        private List<string> m_assetLoadingList = new List<string>();


        /// <summary>
        /// 阻塞加载资源
        /// </summary>
        public Object LoadResourceBlock(string assetPath)
        {
            Object assetObj = LoadAssetFromCache(assetPath);
            if(null == assetObj)
            {
                assetObj = Resources.Load(assetPath);
                if(null != assetObj)
                {
                    m_assetUsedList.Add(assetPath, new AssetCounter(assetObj));
                }
                else
                {
                    LoggerHelper.Error(string.Format("LoadRes Failed: {0} not exist.", assetPath));
                }
            }

            return assetObj;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadResourceAsync(string assetPath, OnLoadResAsyncDone asynCallback)
        {
            Object assetObj = LoadAssetFromCache(assetPath);
            if (null == assetObj)
            {
                if(m_callbackLoadingList.ContainsKey(assetPath))
                {
                    m_callbackLoadingList[assetPath] += asynCallback;
                }
                else
                {
                    m_callbackLoadingList.Add(assetPath, asynCallback);

                    //理论上 该资源不会包含在等待队列、加载队列中
                    if (!m_assetWaitLoadQueue.Contains(assetPath) && !m_assetLoadingList.Contains(assetPath))
                        m_assetWaitLoadQueue.Add(assetPath);
                }
            }
            else
            {
                if(null != asynCallback)
                {
                    asynCallback(assetPath, assetObj);
                }
            }
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="resPath"></param>
        public void UnloadResource(string assetPath)
        {
            if(m_assetUsedList.ContainsKey(assetPath))
            {
                AssetCounter ac = m_assetUsedList[assetPath];
                ac.SubtractReferenceCount();

                if (0 == ac._iReferenceCount) //将其从在用资源列表中移除
                {
                    m_assetUsedList.Remove(assetPath);

                    if (IsNeedCacheUnloadAsset())
                    {
                        AssetRemover ar = new AssetRemover(assetPath, ac._objAsset, Time.time);
                        m_assetRemovingQueue.Add(ar);
                    }
                    else
                    {
                        Resources.UnloadAsset(ac._objAsset);
                    }
                }
            }
            else
            {
                LoggerHelper.Warning(string.Format("Unload Res Failed: {0} not in asset list.", assetPath));
            }
        }

        /// <summary>
        /// 供系统定时调用处理资源加载
        /// </summary>
        public void UpdateResource()
        {
            UpdateAssetToRemove();

            UpdateForAsyncLoad();
        }

        public void Release()
        {
            m_assetUsedList.Clear();
            m_assetRemovingQueue.Clear();
            m_callbackLoadingList.Clear();
            m_assetWaitLoadQueue.Clear();
            m_assetLoadingList.Clear();

            m_assetUsedList = null;
            m_assetRemovingQueue = null;
            m_callbackLoadingList = null;
            m_assetWaitLoadQueue = null;
            m_assetLoadingList = null;
        }


        /// <summary>
        /// 定时调用:移除资源
        /// </summary>
        private void UpdateAssetToRemove()
        {
            //是否可以移除资源
            if(Time.time - AssetRemover.s_lastTimeToRemoveAsset > AssetRemover.c_assetRemoveCacheMinTime)
            {
                AssetRemover.s_lastTimeToRemoveAsset = Time.time;
                while (m_assetRemovingQueue.Count > 0)
                {
                    AssetRemover ar = m_assetRemovingQueue[0];
                    if (Time.time - ar._fRemoveTime >= AssetRemover.c_assetRemoveCacheTime)
                    {
                        m_assetRemovingQueue.RemoveAt(0);
                        Resources.UnloadAsset(ar._objAsset);
                    }
                }
            }
            else
            {

            }

        }

        /// <summary>
        /// 定时调用:异步加载
        /// </summary>
        private void UpdateForAsyncLoad()
        {
            //这里缺少超时检查，可能会存在一种情况，程序一直在加载某4个
            //但是一直未加载出，导致不能正常加载
            //可以通过设置加载超时，来处理

            if(m_assetLoadingList.Count < _MaxLoadCountPerFrame)
            {
                while(m_assetWaitLoadQueue.Count > 0)
                {
                    string assetPath = m_assetWaitLoadQueue[0];

                    m_assetLoadingList.Add(assetPath);
                    m_assetWaitLoadQueue.RemoveAt(0);

                    RunCoroutine.Instance.Run(LoadResourceAsync(assetPath));
                }
            }
            else
            {
                //已达到资源同时加载最大数量
            }
        }

        /// <summary>
        /// 待写，
        /// 是否需要缓存一段时间后，再卸载
        /// </summary>
        /// <returns></returns>
        private bool IsNeedCacheUnloadAsset()
        {
            return false;
        }

        /// <summary>
        /// 从缓存(RemovingQueue or UseDict)中加载资源
        /// </summary>
        private Object LoadAssetFromCache(string assetPath)
        {
            Object assetObj = LoadAssetFromRemovingQueue(assetPath);
            if (null == assetObj)
            {
                assetObj = LoadAssetFromUseDict(assetPath);
            }

            return assetObj;
        }

        /// <summary>
        /// 从待移除列表中加载资源
        /// </summary>
        private Object LoadAssetFromRemovingQueue(string assetPath)
        {
            Object assetObj = null;
            for(int i= m_assetRemovingQueue.Count-1; i != 0; --i)
            {
                AssetRemover ar = m_assetRemovingQueue[i];
                if(ar._assetPath == assetPath && null != ar._objAsset)
                {
                    m_assetRemovingQueue.RemoveAt(i);

                    m_assetUsedList.Add(assetPath, new AssetCounter(ar._objAsset));
                }
            }

            return assetObj;
        }

        /// <summary>
        /// 从在用资源列表加载资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private Object LoadAssetFromUseDict(string assetPath)
        {
            Object assetObj = null;

            if (m_assetUsedList.ContainsKey(assetPath))
            {
                var assetCounter = m_assetUsedList[assetPath];
                assetCounter.AddReferenceCount();
                assetObj = assetCounter._objAsset;
            }

            return assetObj;
        }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerator LoadResourceAsync(string assetPath)
        {
            ResourceRequest request = Resources.LoadAsync(assetPath);

            while(!request.isDone)
            {
                yield return 0;
            }

            if (m_callbackLoadingList.ContainsKey(assetPath))
            {
                Object assetObj = request.asset;

                if (null != assetObj)
                {
                    m_assetUsedList[assetPath] = new AssetCounter(assetObj);
                }
                else
                {
                    LoggerHelper.Error(string.Format("LoadResAsync Fail: {0} not exist.", assetPath));
                }

                OnLoadResAsyncDone asyncCallback = m_callbackLoadingList[assetPath];
                m_callbackLoadingList.Remove(assetPath);
                m_assetLoadingList.Remove(assetPath);

                if (null != asyncCallback)
                {
                    asyncCallback(assetPath, assetObj);
                }
            }
            else
            {
                LoggerHelper.Error(string.Format("LoadResAsync Callback Error: key {0} not exist.", assetPath));
                throw new MissingReferenceException(string.Format("LoadResAsync Callback Error: key {0} not exist.", assetPath));
            }

        }
    }


    /// <summary>
    /// 资源计数器
    /// </summary>
    public class AssetCounter
    {
        /// <summary>
        /// 资源引用数
        /// </summary>
        private int m_iReferenceCount;
        public int _iReferenceCount { get { return m_iReferenceCount; } }

        /// <summary>
        /// 资源
        /// </summary>
        private Object m_objAsset;
        public Object _objAsset { get { return m_objAsset; } }

        public AssetCounter(Object obj, int count = 1)
        {
            m_iReferenceCount = count;
            m_objAsset = obj;
        }

        public void AddReferenceCount()
        {
            ++m_iReferenceCount;
        }

        public void SubtractReferenceCount()
        {
            --m_iReferenceCount;

            //             if (0 >= m_iReferenceCount)
            //             {
            //                 if(null != m_objAsset)
            //                 {
            //                     Resources.UnloadAsset(m_objAsset);
            //                     m_objAsset = null;
            //                 }
            //             }
        }

    }

    /// <summary>
    /// 资源移除器
    /// </summary>
    public class AssetRemover
    {
        /// <summary>
        /// 资源从标记为dirty到真正移除的时间(单位:秒)
        /// </summary>
        public static readonly float c_assetRemoveCacheTime = 10f;
        //资源之间移除时间间隔
        public static readonly float c_assetRemoveCacheMinTime = 2f;


        /// <summary>
        /// 上一次移除的时间点(单位:秒)
        /// </summary>
        public static float s_lastTimeToRemoveAsset = 0f;
        /// <summary>
        /// 移除时间
        /// </summary>
        private float m_fRemoveTime;
        public float _fRemoveTime { get { return m_fRemoveTime; } }

        /// <summary>
        /// 资源
        /// </summary>
        private Object m_objAsset;
        public Object _objAsset { get { return m_objAsset; } }

        private string m_assetPath;
        public string _assetPath { get { return m_assetPath; } }

        public AssetRemover(string assetPath, Object assetObj, float timeRemove)
        {
            m_assetPath = assetPath;
            m_objAsset = assetObj;
            m_fRemoveTime = timeRemove;
        }
    }
}

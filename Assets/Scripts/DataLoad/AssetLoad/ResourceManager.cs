/********资源管理，功能完备性待检验***************
 * Author： clog
 * Date:
 * Description:
 * ***********************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Util;


namespace DataLoad
{
    /// <summary>
    /// 异步加载完成回调
    /// </summary>
    public delegate void OnLoadResAsyncDone(string assetPath, Object assetObject);

    /// <summary>
    /// 资源管理
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        /// <summary>
        /// 每帧加载资源的最大数量
        /// </summary>
        public const int _MaxLoadCountPerFrame = 4;

        /// <summary>
        /// 等待加载的资源队列
        /// </summary>
        private List<string> m_assetWaitLoadQueue = new List<string>();

        /// <summary>
        /// 正在(异步)加载的资源列表
        /// </summary>
        private List<string> m_assetLoadingList = new List<string>();

        /// <summary>
        /// 已加载资源列表
        /// </summary>
        private Dictionary<string, AssetLoader> m_assetLoadedDict = new Dictionary<string, AssetLoader>();

        /// <summary>
        /// 资源回收列表
        /// </summary>
        private List<AssetRemover> m_assetRecycleList = new List<AssetRemover>();

        /// <summary>
        /// 正在(异步)加载的资源回调列表
        /// </summary>
        private Dictionary<string, OnLoadResAsyncDone> m_callbackLoadingDict = new Dictionary<string, OnLoadResAsyncDone>();

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
                    m_assetLoadedDict.Add(assetPath, new AssetLoader(assetObj));
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
                if(m_callbackLoadingDict.ContainsKey(assetPath))
                {
                    //资源为空，说明资源等待加载或在正在加载( 该资源在等待队列或加载队列中)
                    m_callbackLoadingDict[assetPath] += asynCallback;
                }
                else
                {
                    m_callbackLoadingDict.Add(assetPath, asynCallback);

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
            if(m_assetLoadedDict.ContainsKey(assetPath))
            {
                AssetLoader ac = m_assetLoadedDict[assetPath];
                ac.SubtractRefCount();

                if (0 <= ac.RefCount) //将其从在用资源列表中移除
                {
                    m_assetLoadedDict.Remove(assetPath);

                    if (IsNeedCacheUnloadAsset())
                    {
                        AssetRemover ar = new AssetRemover(assetPath, ac.Asset, Time.time);
                        m_assetRecycleList.Add(ar);
                    }
                    else
                    {
                        m_assetLoadedDict.Remove(assetPath);
                        Resources.UnloadAsset(ac.Asset);
                    }
                }
            }
            else
            {
                LoggerHelper.Warning(string.Format("Unload Res Failed: {0} not in asset list.", assetPath));
            }
        }

        /// <summary>
        /// 供系统帧调用，处理资源加载
        /// </summary>
        public void UpdateResource()
        {
            UpdateAssetToRemove();

            UpdateForAsyncLoad();
        }

        public void Release()
        {
            m_assetLoadedDict.Clear();
            m_assetRecycleList.Clear();
            m_callbackLoadingDict.Clear();
            m_assetWaitLoadQueue.Clear();
            m_assetLoadingList.Clear();

            m_assetLoadedDict = null;
            m_assetRecycleList = null;
            m_callbackLoadingDict = null;
            m_assetWaitLoadQueue = null;
            m_assetLoadingList = null;
        }


        /// <summary>
        /// 定时调用:移除资源
        /// </summary>
        private void UpdateAssetToRemove()
        {
            if(Time.time - AssetRemover.s_lastTimeToRemoveAsset >= AssetRemover.c_assetRemoveCacheMinTime)
            {
                //移除回收列表的寿命到期的资源
                for (int i= m_assetRecycleList.Count; i != -1; --i)
                {
                    var ar = m_assetRecycleList[i];
                    if(Time.time - ar.DirtyStartTime >= AssetRemover.c_assetRemoveCacheTime)
                    {
                        m_assetRecycleList.Remove(ar);
                        Resources.UnloadAsset(ar.Asset);
                    }
                }

                AssetRemover.s_lastTimeToRemoveAsset = Time.time;
            }

        }

        /// <summary>
        /// 定时调用异步加载
        /// </summary>
        private void UpdateForAsyncLoad()
        {
            //这里缺少超时检查，可能会存在一种情况，程序一直在加载某4个
            //但是一直未加载出，导致不能正常加载
            //可以通过设置加载超时，来处理

            if(m_assetLoadingList.Count < _MaxLoadCountPerFrame)
            {
                //待改进
                //加载队列，可以设置优先级 <iPriority, Queue>
                
                while(m_assetWaitLoadQueue.Count > 0)
                {
                    string assetPath = m_assetWaitLoadQueue[0];
                    m_assetWaitLoadQueue.RemoveAt(0);

                    RunCoroutine.Instance.Run(LoadResourceAsync(assetPath));
                    m_assetLoadingList.Add(assetPath);

                    if (m_assetLoadingList.Count >= _MaxLoadCountPerFrame)
                    {
                        break;
                    }

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
            return true;
        }

        /// <summary>
        /// 从缓存(RemovingQueue or UseDict)中加载资源
        /// </summary>
        private Object LoadAssetFromCache(string assetPath)
        {
            Object assetObj = LoadAssetFromRecycleList(assetPath);
            if (null == assetObj)
            {
                assetObj = LoadAssetFromUseDict(assetPath);
            }

            return assetObj;
        }

        /// <summary>
        /// 从回收列表中加载资源
        /// </summary>
        private Object LoadAssetFromRecycleList(string assetPath)
        {
            Object assetObj = null;
            for(int i= m_assetRecycleList.Count-1; i != -1; --i)
            {
                AssetRemover ar = m_assetRecycleList[i];
                if(ar.AssetPath == assetPath && null != ar.Asset)
                {
                    m_assetRecycleList.RemoveAt(i);

                    m_assetLoadedDict.Add(assetPath, new AssetLoader(ar.Asset));
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

            if (m_assetLoadedDict.ContainsKey(assetPath))
            {
                var assetCounter = m_assetLoadedDict[assetPath];
                assetCounter.AddRefCount();
                assetObj = assetCounter.Asset;
            }

            return assetObj;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        private IEnumerator LoadResourceAsync(string assetPath)
        {
            ResourceRequest request = Resources.LoadAsync(assetPath);

            while(!request.isDone)
            {
                yield return null;
            }

            if (m_callbackLoadingDict.ContainsKey(assetPath))
            {
                Object assetObj = request.asset;

                if (null != assetObj)
                {
                    m_assetLoadedDict[assetPath] = new AssetLoader(assetObj);
                }
                else
                {
                    LoggerHelper.Error(string.Format("LoadResAsync Fail: {0} not exist.", assetPath));
                }

                OnLoadResAsyncDone asyncCallback = m_callbackLoadingDict[assetPath];
                m_callbackLoadingDict.Remove(assetPath);
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

}

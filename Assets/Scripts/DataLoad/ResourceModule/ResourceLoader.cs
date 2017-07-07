/* 缺陷：
 * 目前一个产品（assetPath）对应一个工厂（loader）
 * 太过浪费
 * */

using System;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace AssetLoad
{

    /// <summary>
    /// 资源加载基类,(删除前缓存一段时间)
    /// </summary>
    public abstract class ResLoader:IAsyncObject
    {
        /// <summary>
        /// 资源加载回调
        /// </summary>
        public delegate void OnLoadResDone(bool isOK, object assetObject);

        /// <summary>
        /// 资源Loader类型->（资源路径，资源Loader实例化）
        /// </summary>
        private readonly static Dictionary<Type, Dictionary<string, ResLoader>>
            m_loaderPool = new Dictionary<Type, Dictionary<string, ResLoader>>();

        /// <summary>
        /// 间隔多久将标记为脏的Loader卸载
        /// </summary>
        public const float c_DirtyToUnloadInterval = 0;

        /// <summary>
        /// 间隔多久清理列表
        /// </summary>
        public const float c_CleanRecyleInterval = 5;

        /// <summary>
        /// 上一次清空回收站的时间/上一次删除的时间点
        /// </summary>
        public static float s_LastTimeCleanRecyle = 0;

        /// <summary>
        /// 回收列表
        /// </summary>
        private readonly static List<ResLoader> m_recycleList = new List<ResLoader>();


        public object RetResult { get; private set; }

        public bool IsDone { get; private set; }

        public bool IsSuccess { get; private set; }

        public string AysncMessage { get; protected set; }


        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// true，则表示可以回收
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// 标记Dirty的时间
        /// </summary>
        public float DirtyTime { get; private set; }

        /// <summary>
        /// 进度百分比（0-1）
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// 回调列表
        /// </summary>
        private readonly List<OnLoadResDone> m_callbackList = new List<OnLoadResDone>();

        /// <summary>
        /// 工厂方法
        /// </summary>
        protected static T CreateNewLoader<T>(string path, OnLoadResDone callback, params object[] args) where T: ResLoader, new()
        {
            Dictionary<string, ResLoader> resLoaders = GetLoaderDictWithType(typeof(T));
            ResLoader loader = null;
            if(string.IsNullOrEmpty(path))
            {
                LoggerHelper.Error(string.Format("{0}: CreateNewLoader path is null", typeof(T)));
            }

            if(!resLoaders.TryGetValue(path, out loader))
            {
                //资源加载器不包含该资源
                loader = new T();
                resLoaders[path] = loader;
                loader.Init(path, args);

                //此处编辑器可以生成对应的Loader GameObject
            }
            else
            {
                if(0 < loader.RefCount)
                {
                    LoggerHelper.Error(string.Format("loader.RefCount: {0}", loader.RefCount));
                    loader.RefCount = 0;
                    loader.IsDirty = false;
                }
            }

            loader.RefCount++;

            if(m_recycleList.Contains(loader))
            {
                m_recycleList.Remove(loader);
            }

            loader.AddCallback(callback);

            return loader as T;
        }

        /// <summary>
        /// 获取对应类型的工厂，无则创建
        /// </summary>
        protected static Dictionary<string, ResLoader> GetLoaderDictWithType(Type t)
        {
            Dictionary<string, ResLoader> dict;
            if(!m_loaderPool.TryGetValue(t, out dict))
            {
                dict = m_loaderPool[t] = new Dictionary<string, ResLoader>();
            }

            return dict;
        }

        /// <summary>
        /// 帧调用，卸载无用资源 
        /// </summary>
        public static void UpdateResToUnload()
        {
            if(Time.time - s_LastTimeCleanRecyle > c_CleanRecyleInterval)
            {
                for (int i = m_recycleList.Count - 1; i != -1; --i)
                {
                    var loader = m_recycleList[i];
                    if (loader.IsDirty && Time.time - loader.DirtyTime >= c_DirtyToUnloadInterval)
                    {
                        m_recycleList.Remove(loader);
                        loader.Unload();
                    }
                }

                s_LastTimeCleanRecyle = Time.time;
            }
        }

        /// <summary>
        /// 添加加载完成回调
        /// </summary>
        /// <param name=""></param>
        protected void AddCallback(OnLoadResDone callback)
        {
            if(null != callback)
            {
                if(IsDone)
                {
                    //如果加载已完成 则直接调用
                    callback(IsSuccess, RetResult);
                }
                else
                {
                    //添加调用列表中
                    m_callbackList.Add(callback);
                }
            }
        }

        /// <summary>
        /// 执行回调列表中的回调
        /// </summary>
        protected void DoCallback(bool isOk, object retResult)
        {
            for(int i=0; i != m_callbackList.Count; ++i)
            {
                var callback = m_callbackList[i];
                if(null != callback)
                {
                    callback(isOk, retResult);
                }
            }

            m_callbackList.Clear();
        }

        /// <summary>
        /// Loader初始化
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        protected virtual void Init(string path, params object[] args)
        {
            RetResult = null;
            IsDone = false;
            IsSuccess = false;

            IsDirty = false;
            AssetPath = path;
            Progress = 0;
        }

        /// <summary>
        /// Loader加载完成时调用
        /// </summary>
        /// <param name="retResult"></param>
        protected virtual void OnFinish(object retResult)
        {
            this.RetResult = retResult;
            Progress = 1;
            IsSuccess = retResult == null;

            IsDone = true;
            DoCallback(IsSuccess, retResult);
        }

        /// <summary>
        /// 释放(减少引用计数)
        /// </summary>
        public virtual void Release()
        {
            RefCount--;

            //如果没有被引用，则标记为脏，等待卸载
            if (RefCount <= 0)
            {
                IsDirty = true;
                DirtyTime = Time.time;
                if (!m_recycleList.Contains(this))
                {
                    m_recycleList.Add(this);
                }
                else
                {
                    LoggerHelper.Error(string.Format("{0} already exist.", this));
                }

                OnReadyUnload();
            }
        }

        /// <summary>
        /// 引用为0时触发，准备卸载
        /// </summary>
        protected virtual void OnReadyUnload()
        {

        }

        /// <summary>
        /// 卸载
        /// </summary>
        private void Unload()
        {
            var type = GetType();
            Dictionary<string, ResLoader> resLoaders = GetLoaderDictWithType(type);
            var bRemove = resLoaders.Remove(AssetPath);

            if(IsDone)
            {
                DoUnload();
            }
            else
            {
                // 未完成，在OnFinish时会执行DoUnload
            }
        }

        /// <summary>
        /// 卸载操作
        /// </summary>
        protected virtual void DoUnload()
        {

        }
    }
}

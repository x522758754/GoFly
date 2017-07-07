using UnityEngine;

namespace DataLoad
{
    /// <summary>
    /// 资源加载器
    /// </summary>
    public class AssetLoader
    {
        /// <summary>
        /// 资源引用数
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// 资源
        /// </summary>
        public Object Asset { get; private set; }

        public AssetLoader(Object obj, int count = 1)
        {
            RefCount = count;
            Asset = obj;
        }

        public void AddRefCount()
        {
            ++RefCount;
        }

        public void SubtractRefCount()
        {
            --RefCount;
        }

    }
}

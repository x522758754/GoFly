/* 思想说明
 * 1.资源不会立刻被释放，有几秒的缓冲期
 * 2.资源可能重新从池里恢复
 * 当一个加载对象被引用计数减为0的时候，他不会被立刻释放。
 * 因为存在这样一种场景：当引用变成0的同一时间，同样的资源又被创建一份新的，引用计数立刻变回1。
 * 所以如果说当他引用计数为0时候，立刻就被清理了，同时又被创建，这里就会造成了重复的对这份内存资源创建和释放。
 * */
using UnityEngine;

namespace DataLoad
{

    /// <summary>
    /// 资源移除器
    /// </summary>
    public class AssetRemover
    {
        /// <summary>
        /// 资源从标记为dirty到真正移除的时间(单位:秒)
        /// </summary>
        public static readonly float c_assetRemoveCacheTime = 10f;
        /// <summary>
        /// 多久一次做资源清理
        /// </summary>
        public static readonly float c_assetRemoveCacheMinTime = 2f;
        /// <summary>
        /// 上一次移除的时间点(单位:秒)
        /// </summary>
        public static float s_lastTimeToRemoveAsset = 0f;
        /// <summary>
        /// 标记Dirty的时间点
        /// </summary>
        public float DirtyStartTime { get; private set; }

        /// <summary>
        /// 资源
        /// </summary>
        public Object Asset { get; private set; }

        public string AssetPath { get; private set; }

        public AssetRemover(string assetPath, Object assetObj, float timeRemove)
        {
            AssetPath = assetPath;
            Asset = assetObj;
            DirtyStartTime = timeRemove;
        }
    }
}

/**************************************************
 * Author： clog
 * Date:
 * Description: 对象池接口ObjectPool，管理对象池中对象的借出、归还等必要的操作
 * ***********************************************/

using UnityEngine;

namespace Util
{
    public interface IObjectPool
    {
        /// <summary>
        /// 从对象池取出对象
        /// </summary>
        Object BorrowObject();

        /// <summary>
        /// 将对象返回给对象池
        /// </summary>
        void ReturnObject(Object obj);

        /// <summary>
        /// 返回当前对象池所申请对象的上限数目
        /// </summary>
        /// <returns></returns>
        int GetMaxNum();

        /// <summary>
        /// 返回对象池已经借出的对象数目
        /// </summary>
        /// <returns></returns>
        int GetActiveNum();

        /// <summary>
        /// 返回对象池中空闲的对象数目
        /// </summary>
        /// <returns></returns>
        int GetIdleNum();

        /// <summary>
        /// 清除对象池所有的空闲对象s
        /// </summary>
        void Clear();

        /// <summary>
        ///关闭对象池，并释放所占资源
        /// </summary>
        void Close();

        /// <summary>
        /// 设置池化对象管理工厂，用于管理对象池中的对象
        /// </summary>
        /// <param name="facory"></param>
        //void SetFactory(IPoolableObjectFactory facory);
    }
}

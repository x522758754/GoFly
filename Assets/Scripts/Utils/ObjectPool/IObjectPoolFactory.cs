/**************************************************
 * Author： clog
 * Date:
 * Description: 对象池工厂ObjectPoolFactory,采用工厂模式生产对象
 * ***********************************************/

using UnityEngine;

namespace Util
{
    public interface IObjectPoolFactory
    {
        /// <summary>
        /// 对象池工厂 用于生成对象池
        /// </summary>
        /// <param name="factory">池化对象管理工厂</param>
        /// <param name="maxNum">对象池所申请的最大对象数目</param>
        /// <param name="type">对象类型</param>
        IObjectPool CreatePool(IPoolableObjectFactory factory, int maxNum);
    }
}

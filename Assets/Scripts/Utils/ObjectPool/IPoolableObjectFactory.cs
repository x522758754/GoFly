/**************************************************
 * Author： clog
 * Date:
 * Description: 池化对象管理工厂接口,用于管理对象池中的对象的创建、还原、销毁
 * ***********************************************/
using UnityEngine;

namespace Util
{
    public interface IPoolableObjectFactory
    {
        /// <summary>
        /// 创建新对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        Object CreateObject();

        /// <summary>
        /// 对象在使用过程中内部状态会发生变化
        /// 当归还对象池可能需要将对象还原为初始状态
        /// </summary>
        Object ClearObject(Object obj);

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="obj"></param>
        void DestroyObject(Object obj);
    }
}

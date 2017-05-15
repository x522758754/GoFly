using System;
using UnityEngine;

namespace Util
{
    public class GameObjectPoolableObjectFactory: Singleton<GameObjectPoolableObjectFactory>, IPoolableObjectFactory
    {
        public UnityEngine.Object CreateObject()
        {
            return new GameObject();
        }

        public UnityEngine.Object ClearObject(UnityEngine.Object obj)
        {
            return obj;
        }

        public void DestroyObject(UnityEngine.Object obj)
        {

        }
    }
}

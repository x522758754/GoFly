using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class GameObjectPoolFactory: Singleton<GameObjectPoolFactory>, IObjectPoolFactory
    {
        public IObjectPool CreatePool(IPoolableObjectFactory factory, int maxNum)
        {
            return new GameObjectPool(factory, maxNum);
        }
    }
}

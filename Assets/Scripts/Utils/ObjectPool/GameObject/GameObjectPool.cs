using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Util
{
    public class GameObjectPool : IObjectPool
    {
        protected Stack<Object> m_pool = null;

        protected IPoolableObjectFactory factory = null;

        protected int m_maxNum = 100;

        protected int m_activeNum = 0;

        public GameObjectPool(IPoolableObjectFactory factory, int maxNum)
        {
            this.factory = factory;
            this.m_activeNum = 0;
            this.m_maxNum = maxNum;
            this.m_pool = new Stack<Object>();
        }

        public Object BorrowObject()
        {
            Object currObj = null;

            if (0 != m_pool.Count)
            {
                currObj = m_pool.Pop();
            }
            else
            {
                if (m_activeNum < m_maxNum)
                {
                    if (null == factory)
                    {
                        throw new System.Exception();
                    }

                    currObj = factory.CreateObject() as GameObject;
                    if (null == currObj)
                    {
                        throw new System.Exception("obj is null");
                    }
                }
                else
                {
                    //这里标记为空
                    return null;
                }
            }

            ++m_activeNum;

            return currObj;
        }

        public void ReturnObject(Object obj)
        {
            m_pool.Push(obj);

            --m_activeNum;
        }

        public void Clear()
        {
            m_pool.Clear();
        }

        public void Close()
        {
            m_pool.Clear();
            m_pool = null;

            m_maxNum = -1;
            m_activeNum = -1;
        }

        public int GetActiveNum()
        {
            return m_activeNum;
        }

        public int GetIdleNum()
        {
            return m_maxNum - m_activeNum;
        }

        public int GetMaxNum()
        {
            return m_maxNum;
        }
        
    }

}

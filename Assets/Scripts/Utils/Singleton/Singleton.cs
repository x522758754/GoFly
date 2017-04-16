

namespace Util
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly object localLock = new object();
        protected static T m_instance = null;
        public static T Instance
        {
            get
            {
                if (null == m_instance)
                {
                    lock (localLock)
                    {
                        if (null == m_instance)
                        {
                            m_instance = new T();
                        }
                    }
                }

                return m_instance;
            }
        }

    }

}

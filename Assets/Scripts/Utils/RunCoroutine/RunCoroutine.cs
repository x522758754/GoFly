/**************************************************
 * Author： clog
 * Date:
 * Description: 基于Unity的协程,功能完备性待检验
 * ***********************************************/
using UnityEngine;
using System.Collections;

namespace Util
{
    /// <summary>
    /// 基于Unity的协程
    /// </summary>
    public class RunCoroutine :MonoBehaviour
    {
        private static readonly string m_strGoName = "RunCoroutine";
        private static GameObject m_go;

        private static RunCoroutine m_instace;
        public static RunCoroutine Instance
        {
            get
            {
                if(null == m_go)
                {
                    m_go = new GameObject(m_strGoName);
                    DontDestroyOnLoad(m_go);
                }
                
                RunCoroutine rc = m_go.GetComponent<RunCoroutine>();
                if (null == rc)
                {
                    rc = m_go.AddComponent<RunCoroutine>();
                }

                return rc;
            }
        }

        public Coroutine Run(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void Stop(IEnumerator routine)
        {
            StopCoroutine(routine);
        }

        public void Release()
        {
            if(null != m_go)
            {
                GameObject.Destroy(m_go);
                m_go = null;
            }
            StopAllCoroutines();
        }

    }
}

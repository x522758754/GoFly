using UnityEngine;
using Util;
using System.Collections.Generic;

namespace UI
{
    public class UITool
    {
        public static GameObject CreateObj(string path)
        {
            Object obj = Resources.Load(path);
            if(null != obj)
            {
                GameObject go = GameObject.Instantiate(obj) as GameObject;
                return go;   
            }

            return null;
        }

        public static GameObject AddChild(GameObject child, GameObject parent)
        {
            if(null == child || null == parent)
            {
                LoggerHelper.Error("child or parent is null");
                return null;
            }
            else
            {
                child.transform.parent = parent.transform;
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;

                return child;
            }
        }
    }
}

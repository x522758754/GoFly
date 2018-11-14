using UnityEngine;
using Util;
using DataLoad;
using System.Collections.Generic;

namespace UI
{
    public class UITool
    {
        #region 实例物体
        public static GameObject CreateObj(GameObject obj)
        {
            if(null != obj)
            {
                //备注：prefab不可直接实例化,unity有保护机制
                //Instantiate 类似于拷贝构造函数
                GameObject go = GameObject.Instantiate(obj) as GameObject;
                return go;   
            }

            return null;
        }

        public static GameObject CreateObj(string path)
        {
            Object obj = ResourceManager.Instance.LoadResourceBlock(path);
            if (obj != null)
            {
                GameObject creatObj = GameObject.Instantiate(obj) as GameObject;
                return creatObj;
            }
            else
            {
                return null;
            }
        }

        public static void CreateObjAsync(string path, System.Action<GameObject> fnCallback)
        {
            ResourceManager.Instance.LoadResourceAsync(path, (name, obj) =>
            {
                GameObject creatObj = null;
                if (obj != null)
                {
                    creatObj = GameObject.Instantiate(obj) as GameObject;
                    ResourceManager.Instance.UnloadResource(path);
                }
                if (null != fnCallback)
                    fnCallback(creatObj);
            });
        }

        #endregion

        #region 添加物体
        public static GameObject AddChild(GameObject item, GameObject parent)
        {
            if (null != parent && null != item)
            {
                item.transform.SetParent(parent.transform);
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
                item.transform.localEulerAngles = Vector3.zero;

                return item;
            }
            else
            {
                LoggerHelper.Error(string.Format("parent is {0} or item is {1}", parent, item));
                return null;
            }
        }

        public static GameObject AddChildSafe(GameObject child, GameObject parent)
        {
            GameObject go = AddChild(child, parent);
            if(null != go)
            {
                //调整层级（panel层级或者粒子特效）
            }

            return go;
        }

        #endregion

        #region 获得物体

        public static GameObject FindChild(GameObject parent, string childName)
        {
            if (null != parent)
                return parent.transform.Find(childName).gameObject;
            else
                return null;
        }

        public static GameObject FindChildRecursion(GameObject parent, string childName)
        {
            Transform[] transforms = parent.GetComponentsInChildren<Transform>();
            for(int i=0; i != transforms.Length; ++i)
            {
                var transform = transforms[i];
                if (0 == string.Compare(transform.name, childName))
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        #endregion

        #region 删除子物体

        public static void DeleteChilds(GameObject parent)
        {
            if(null != parent)
            {
                Transform t = parent.transform;
                for(int i=0; i != t.childCount; ++i)
                {
                    GameObject.Destroy(t.GetChild(i).gameObject);
                }
            }
            else
            {
                LoggerHelper.Warning("DeleteChilds go is null");
            }
        }

        #endregion

        #region 坐标系转换

        public Vector2 WorldPosToScreenPos(Camera camera, Vector3 pos)
        {
            return new Vector2();
        }

        public Vector3 ScreenPosToWorldPos(Camera camera, Vector2 pos)
        {
            return new Vector3();
        }

        #endregion
    }
}

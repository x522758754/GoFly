using UnityEditor;
using UnityEngine;

namespace EditorUtils
{


    public static class EditorUtil
    {
        [MenuItem("UIMenu/Del/删除空组件", false, 9)]
        /// <summary>
        /// 删除物体上的空组件
        /// </summary>
        public static void DelEmptyComponent()
        {
            GameObject go = Selection.activeGameObject;
            SerializedObject so = new SerializedObject(go);
            SerializedProperty prop = so.FindProperty("m_Component");

            var cpts = go.GetComponents<Component>();
            for (int i = cpts.Length - 1; i != -1; --i)
            {
                if (null == cpts[i])
                {
                    prop.DeleteArrayElementAtIndex(i);
                }

            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
        }


        public static bool CanStaticCombine(Transform trans)
        {
            //各种动画
            if (trans.GetComponent<Animation>() != null)
            {
                return false;
            }
            if (trans.GetComponent<Animator>() != null)
            {
                return false;
            }

            //粒子系统
            if (trans.GetComponent<ParticleSystem>() != null)
            {
                return false;
            }

            //透明材质
            Renderer r = trans.GetComponent<Renderer>();
            if (r != null)
            {
                Material[] sharedMaterials = r.sharedMaterials;
                foreach (var material in sharedMaterials)
                {
                    if (material == null) return false;
                    if (material.renderQueue >= 2750 || material.shader.renderQueue >= 2750)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
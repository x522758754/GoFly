using UnityEditor;
using UnityEngine;

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
        for(int i= cpts.Length - 1; i != -1; --i)
        {
            if(null == cpts[i])
            {
                prop.DeleteArrayElementAtIndex(i);
            }

        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(go);
    }

}
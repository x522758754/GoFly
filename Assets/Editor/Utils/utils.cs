using UnityEditor;
using UnityEngine;

public static class EditorUtil
{
    [MenuItem("UIMenu/Del/ɾ�������", false, 9)]
    /// <summary>
    /// ɾ�������ϵĿ����
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
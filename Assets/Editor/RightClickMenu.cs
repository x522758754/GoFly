using UnityEngine;
using UnityEditor;
using System.IO;

public class RightClickMenu : EditorWindow
{
    [MenuItem("Assets/Create/Txt", false, 85)]
    public static void CreateTxt()
    {
        Object selected = Selection.activeObject;
        string path = string.Format(AssetDatabase.GetAssetPath(selected));
        if (null == path)
        {
            path = "Assets/";
        }
        else
            path = string.Format("{0}/", path);
        
        ScriptableObject so = new ScriptableObject();

        string name = "newTxt";
        string allPath = "";

        Object obj = null;
        do
        {
            name += 1;
            allPath = string.Format("{0}{1}.txt", path, name);
            obj = AssetDatabase.LoadAssetAtPath(allPath, typeof(TextAsset));
        }
        while (null != obj);
        AssetDatabase.CreateAsset(so, allPath);
        AssetDatabase.Refresh();
    }

    public static void CreateFile(string path, byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }
}

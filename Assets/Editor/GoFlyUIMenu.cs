using UnityEngine;
using UnityEditor;
using System.Collections;

static public class NewBehaviourScript
{
    #region Atlas 

    [MenuItem("UIMenu/Open/分离图集", false, 9)]
    public static void OpenWindow_SplitAtlas()
    {
        EditorWindow.GetWindow<SplitAtlas>(false, "SplitAtlas", true).Show();
    }

    #endregion
}

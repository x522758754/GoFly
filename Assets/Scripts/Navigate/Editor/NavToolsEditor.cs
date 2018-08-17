using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

[CustomEditor(typeof(NavTools))]
public class NavToolsEditor : Editor
{
    NavTools navTools;

    void OnEnable()
    {
        navTools = target as NavTools;
    }
    public override void OnInspectorGUI()
    {
        //EditorGUILayout.BeginHorizontal();

        //EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("load map"))
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = "All Files\0*.*\0\0";
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = UnityEngine.Application.dataPath;//默认路径
            ofn.title = "Open Project";
            ofn.defExt = "bin";//显示文件的类型
            //注意 一下项目不一定要全选 但是0x00000008项不要缺少
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
            if (OpenWindows.GetOpenFileName(ofn))
            {
                navTools.LoadMap(ofn.file);
            }
        }

        if(GUILayout.Button("load ob"))
        {

        }

        if(GUILayout.Button("clear ob"))
        {

        }

        if(GUILayout.Button("save ob"))
        {

        }

        if(GUILayout.Button("test navmesh"))
        {
            //批量生成生成机器人自动跑场景，随机设置目标点寻路
            ///寻找跑不到目标点 或者从场景中掉下去的点打印出来

        }
    }
}

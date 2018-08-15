using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MinimapMaker))]
public class MinimapMakeEditor : Editor
{
    public Vector2 texPos;
    public Vector2 minimapPos;
    public Vector2 worldPos;

    public int mapIdProp = 0;
    public int texWidthProp = 1024;
    public int texHeightProp = 576;

    MinimapMaker minimapMaker;
    UIMinimap uiMinimap;
    void OnEnable()
    {
        minimapMaker = target as MinimapMaker;
        uiMinimap = new UIMinimap();
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        minimapMaker.mapId = EditorGUILayout.IntField("地图ID", minimapMaker.mapId);
        mapIdProp = minimapMaker.mapId;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        texWidthProp = EditorGUILayout.IntField("纹理宽度", texWidthProp);
        minimapMaker.texWidth = texWidthProp;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        texHeightProp = EditorGUILayout.IntField("纹理高度", texHeightProp);
        minimapMaker.texHeight = texHeightProp;
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TagField("左上角世界坐标", minimapMaker.minimapData.ltPos.ToString("f2"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TagField("左下角世界坐标", minimapMaker.minimapData.lbPos.ToString("f2"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TagField("右下角世界坐标", minimapMaker.minimapData.rbPos.ToString("f2"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TagField("右上角世界坐标", minimapMaker.minimapData.rtPos.ToString("f2"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        texPos = EditorGUILayout.Vector2Field("纹理坐标", texPos);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        minimapPos = EditorGUILayout.Vector2Field("地图坐标", minimapPos);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        worldPos = EditorGUILayout.Vector2Field("世界坐标", worldPos);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("导出Minimap"))
        {
            if (minimapMaker)
            {
                minimapMaker.Make();
                uiMinimap.minimapData = minimapMaker.minimapData;
            }
        }

        if (GUILayout.Button("纹理 => world"))
        {
            minimapPos = uiMinimap.Tex2Minimap(texPos);
            worldPos = uiMinimap.Tex2World(texPos);
        }

        if (GUILayout.Button("minimap => Tex"))
        {
            texPos = uiMinimap.Minimap2Tex(minimapPos);
        }

        if (GUILayout.Button("world => 纹理"))
        {
            minimapPos = uiMinimap.World2Minimap(worldPos);
            texPos = uiMinimap.World2Tex(worldPos);
        }
    }
}

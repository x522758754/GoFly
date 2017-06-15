using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// NGUI图集相关 
/// </summary>
public class FindSprites
{

    public static string allSpriteFile = "allSprites.txt";
    public static string allAtlasFile = "所有Atlas图片.txt";
    //public static string allAtlasPath = Application.dataPath + "/" + "TGameResources" + "/" + "SubSys" + "/" + "AllAtlas";
    public static string allAtlasPath = Application.dataPath + "/" + "TGameResources";

    /// <summary>
    /// 寻找所有的spite
    /// </summary>
    public static void FindAllAtlasSprites()
    {
        List<UIAtlas> allAtlasList = new List<UIAtlas>();
        string[] allAtlas = AllAtlasPath(allAtlasPath);
        for (int i = 0; i < allAtlas.Length; i++)
        {
            GameObject gameObject = AssetDatabase.LoadAssetAtPath(allAtlas[i], typeof(GameObject)) as GameObject;
            if (gameObject != null)
            {
                UIAtlas atlas = gameObject.GetComponent<UIAtlas>();
                if (atlas != null)
                {
                    allAtlasList.Add(atlas);
                }
            }
        }
        GetAllAtlasSprites(allAtlasList, allAtlasFile);
    }

    public static string[] AllAtlasPath(string _resourcePath)
    {
        string[] allpath = Directory.GetFiles(allAtlasPath, "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < allpath.Length; i++)
        {
            allpath[i] = allpath[i].Replace(Application.dataPath, "Assets");
            allpath[i] = allpath[i].Replace("\\", "/");
        }
        return allpath;
    }

    /// <summary>
    /// 寻找
    /// </summary>
    /// <param name="atlasList"></param>
    /// <param name="saveFilePath"></param>
    public static void GetAllAtlasSprites(List<UIAtlas> atlasList, string saveFilePath)
    {
        for (int i = 0; i < atlasList.Count; i++)
        {
            //LogHelper.writeline(string.Format("图集名称 {0}:", atlasList[i].name));
            for (int j = 0; j < atlasList[i].spriteList.Count; j++)
            {
                Debug.Log(atlasList[i].spriteList[j].name);
                //LogHelper.OpenFile(saveFilePath, FileMode.Create);
                //LogHelper.writeline(atlasList[i].spriteList[j].name);
            }
        }
        //LogHelper.CloseFile();
    }


    [MenuItem("Custom Editor/UI/find SelectedAtlas allSprites")]
    public static void FindSelectedAtlasSprites()
    {
        List<UIAtlas> allSelectedAtlasList = new List<UIAtlas>();
        allSelectedAtlasList = GetSelectedAtlas();
        GetAllAtlasSprites(allSelectedAtlasList, allSpriteFile);
    }

    [MenuItem("Custom Editor/UI/find SelectedAtlas referencePrefabs")]
    public static void FindSelectedAtlasReference()
    {
        Dictionary<UIAtlas, string> atlasDic = new Dictionary<UIAtlas, string>();
        List<UIAtlas> allSelectedAtlasList = new List<UIAtlas>();
        GameObject[] selected = Selection.gameObjects;
        for (int i = 0; i < selected.Length; i++)
        {
            UIAtlas atlas = selected[i].GetComponent<UIAtlas>();
            if (atlas != null)
            {
                allSelectedAtlasList.Add(atlas);
                atlasDic.Add(atlas, AssetDatabase.GetAssetPath(selected[i]));
            }
        }
        for (int i = 0; i < atlasDic.Count; i++)
        {
            string[] allDependencies = AssetDatabase.GetDependencies(new string[] { atlasDic[allSelectedAtlasList[i]] });
            for (int j = 0; j < allDependencies.Length; j++)
            {
                Debug.Log(allDependencies[j]);
            }
        }
    }

    public static List<UIAtlas> GetSelectedAtlas()
    {
        List<UIAtlas> result = new List<UIAtlas>();
        GameObject[] selected = Selection.gameObjects;
        for (int i = 0; i < selected.Length; i++)
        {
            UIAtlas atlas = selected[i].GetComponent<UIAtlas>();
            if (atlas != null)
            {
                result.Add(atlas);
            }
        }

        return result;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2Int
{
    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int x;
    public int y;
}


public class GameResolution
{
    //场景相机分辨率
    public static Vector2Int[] resolutionSettings =
        {
        new Vector2Int(850, 480),
        new Vector2Int(1280, 720),
        new Vector2Int(1680,960),
        new Vector2Int(1920,1080)
    };

    private static int ResolutionWidth;
    private static int ResolutionHeight;

    public static Vector2Int GetFullResolution()
    {
        Vector2Int vec;
        if (ResolutionWidth == 0 || ResolutionHeight == 0)
        {
            Resolution r = Screen.currentResolution;
            ResolutionWidth = r.width;
            ResolutionHeight = r.height;
        }

        vec.x = ResolutionWidth;
        vec.y = ResolutionHeight;

        return vec;
    }

    public static Vector2Int CalcResoution(int lv)
    {
        Vector2Int vec;
        lv = Mathf.Clamp(lv, 0, resolutionSettings.Length - 1);
        vec = resolutionSettings[lv];

        if (vec.x < vec.y)
        {
            float aspect = vec.y / vec.x;
            vec.x = Mathf.Min(vec.x, ResolutionWidth);
            vec.y = Mathf.CeilToInt(vec.x * aspect);
        }
        else
        {
            float aspect = ResolutionWidth / (float)ResolutionHeight;
            vec.y = Mathf.Min(vec.y, ResolutionHeight);
            vec.x = Mathf.CeilToInt(vec.y * aspect);
        }

        return vec;
    }
}

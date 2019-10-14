using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;


public struct MinimapData
{
    public int id;                  //图片id
    public int texWidth;            //图片生成宽度
    public int texHeight;           //图片生成高度
    public string texName;          //图片名字
    public float realWidth;         //图片宽对应实际宽度
    public float realHeight;        //图片高对应实际高度
    public float yRotation;         //图片旋转绕y旋转角度
    public Vector2 ltPos;           //相机旋转后，相机视口左下角(即图片左上角)对应的世界坐标
    public Vector2 lbPos;           //相机旋转后，相机视口左下角(即图片左下角)对应的世界坐标
    public Vector2 rbPos;           //相机旋转后，相机视口右上角(即图片右下角)对应的世界坐标
    public Vector2 rtPos;           //相机旋转后，相机视口右上角(即图片右上角)对应的世界坐标
};

/// <summary>
/// 相机拍照的小地图需要俯视图
/// 因此需调整相机绕x轴旋转90°=> camera.eulerAngles (90, 0, 0),以此为默认情形,
/// 绕y旋转可以调整拍摄的角度
/// </summary>
[RequireComponent(typeof(Camera))]
public class MinimapMaker : MonoBehaviour
{
    public string outputDir = "";
    public int mapId = 0;
    public int texWidth = 1280;//生成的分辨率，width
    public int texHeight = 720;//生成的分辨率，height

    public MinimapData minimapData;
   

    Camera cam;

	void Start ()
    {
        cam = this.gameObject.GetComponent<Camera>();

        Debug.Log(Screen.width);
        Debug.Log(Screen.height);

        Screen.SetResolution(texWidth, texHeight, true);

    }

    Vector2 Rotate(Vector2 pos, Vector2 center, float angle)
    {
        Vector2 rotatedPos;
        angle = Mathf.Deg2Rad * angle;

        ///T(-center)
        pos = pos - center;
        ///R(angle)
        rotatedPos.x = Mathf.Cos(angle) * pos.x - Mathf.Sin(angle) * pos.y;
        rotatedPos.y = Mathf.Sin(angle) * pos.x + Mathf.Cos(angle) * pos.y;
        //T(center)
        rotatedPos = rotatedPos + center;

        return rotatedPos;
    }

    public void Make()
    {
        if (!cam || !cam.orthographic)
        {
            Debug.LogWarning("cam.orthographic false");
            return;
        }

        //图片左下角的对应的世界坐标
        
        ///计算图片对应的实际场景长宽
        float camHalfHeight = cam.orthographicSize;
        float minimapHeiht = camHalfHeight * 2;
        float minimapWidth = minimapHeiht / texHeight * texWidth;

        ///计算图片左下角的旋转 
        ///
        ///方式一：图片绕图片中心旋转矩阵，M = T(minimapCenter)R(yRotate)T(-minimapCenter)
        /// 1   0   minimapCenter.x     cos(yRotate)    -sin(yRotate)   0       1   0   -minimapCenter.x        x
        ///[0   1   minimapCenter.y] * [sin(yRotate)    cos(yRotate)    0] * [  0   1   -minimapCenter.y] * [   y   ]
        /// 0   0   1                   0               0               1       0   0   1                       1
        ///相机没旋转时,图片对应的信息对应的坐标信息
        Vector2 minimapCenter = new Vector2(cam.transform.position.x, cam.transform.position.z); //相机位置对应图片中心
        Vector2 minimapLeftTop = new Vector2(minimapCenter.x - 0.5f * minimapWidth, minimapCenter.y + 0.5f * minimapHeiht);
        Vector2 minimapLeftBottom = new Vector2(minimapCenter.x - 0.5f * minimapWidth, minimapCenter.y - 0.5f * minimapHeiht);
        Vector2 minimapRightBottom = new Vector2(minimapCenter.x + 0.5f * minimapWidth, minimapCenter.y - 0.5f * minimapHeiht);
        Vector2 minimapRightTop = new Vector2(minimapCenter.x + 0.5f * minimapWidth, minimapCenter.y + 0.5f * minimapHeiht);

        ///相机旋转后，图片对应的信息
        float yRotate = -cam.transform.eulerAngles.y; //图片的旋转与相机的旋转相反
        Vector2 leftTopNewPos = Rotate(minimapLeftTop, minimapCenter, yRotate);
        Vector2 leftBottomNewPos = Rotate(minimapLeftBottom, minimapCenter, yRotate);
        Vector2 rightBottomNewPos = Rotate(minimapRightBottom, minimapCenter, yRotate);
        Vector2 rightTopNewPos = Rotate(minimapRightTop, minimapCenter, yRotate);
        // 
        //         ///T(-minimapCenter)
        //         minimapLeftBottom = minimapLeftBottom - minimapCenter;
        //         ///R(yRotate)
        //         leftbottomNewPos.x = Mathf.Cos(yRotate * Mathf.Deg2Rad) * minimapLeftBottom.x - Mathf.Sin(yRotate * Mathf.Deg2Rad) * minimapLeftBottom.y;
        //         leftbottomNewPos.y = Mathf.Sin(yRotate * Mathf.Deg2Rad) * minimapLeftBottom.x + Mathf.Cos(yRotate * Mathf.Deg2Rad) * minimapLeftBottom.y;
        //         //T(minimapCenter)
        //         leftbottomNewPos = leftbottomNewPos + minimapCenter;

        ///方式二：普通的向量旋转
        ///...

        //生成图片
        RenderTexture rtMinimap = RenderTexture.GetTemporary(texWidth, texHeight, 0, RenderTextureFormat.ARGB32);
        cam.targetTexture = rtMinimap; // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机 
        cam.Render();
        RenderTexture.active = rtMinimap; // 激活这个rt, 并从中中读取像素。

        ///注：生成给美术用可以生成2倍大的图，让画的效果更好，最后美术再缩小回来
        Texture2D texMinimap = new Texture2D(rtMinimap.width, rtMinimap.height, TextureFormat.RGBA32, false);
        texMinimap.ReadPixels(new Rect(0, 0, rtMinimap.width, rtMinimap.height), 0, 0);
        texMinimap.Apply();

        byte[] bytes = texMinimap.EncodeToPNG();
        string fileName = Path.GetFileName(SceneManager.GetActiveScene().name) + ".png";
        string filePath = Application.dataPath + "/" + outputDir + fileName;
        System.IO.File.WriteAllBytes(fileName, bytes);

        //重置相关参数，以使用camera继续在屏幕上显示 
        cam.targetTexture = null;
        RenderTexture.active = null;
        GameObject.Destroy(rtMinimap);

        //小地图信息存储
        minimapData.id = mapId;
        minimapData.texName = fileName;
        minimapData.texWidth = texWidth;
        minimapData.texHeight = texHeight;
        minimapData.realWidth = minimapWidth;
        minimapData.realHeight = minimapHeiht;
        minimapData.yRotation = yRotate;
        minimapData.ltPos = leftTopNewPos;
        minimapData.lbPos = leftBottomNewPos;
        minimapData.rbPos = rightBottomNewPos;
        minimapData.rtPos = rightTopNewPos;
    }
}

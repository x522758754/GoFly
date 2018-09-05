using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// https://blog.csdn.net/mobilebbki399/article/details/50240515
/// https://blog.csdn.net/aceyan0718/article/details/52279594 pcf优化阴影边界
/// 结合Projector 和RT实现实时阴影
/// 本质上是一种贴花(Decal)技术：将阴影贴图投射到你想展现阴影的地方，优点：投影效果不取决与被投影区域的几何形状，即被投影区域的可以是任意凹凸面
/// </summary>
public class ProjectorSetting : MonoBehaviour
{
    public Shader projShader;
    public Camera projCam;
    public Projector proj;
    public LayerMask renderLayers; //设置渲染目标的Layer
    public LayerMask projIgnoreLayers; //设置不需要被投射阴影的层
    public float cullDistance = 32f; //层消隐距离
    public int texWidth = 1024;

    public Texture2D texFade;
    private RenderTexture rt;

    void Start()
    {
        setCamera();
        setProj();
    }

    void LateUpdate()
    {
        customeRender();
    }

    /// <summary>
    /// 相机设置
    /// </summary>
    void setCamera()
    {
        if (!projCam) return;

        projShader = Shader.Find("Unlit/ShadowCreate");

        ///canera自定义设置

        ///关闭自动渲染，改成手动渲染
        ///防止camera 发送其它渲染事件，被其它脚本捕捉，加工处理
        projCam.enabled = false;

        ///设置要渲染的Layers
        projCam.cullingMask = renderLayers.value;

        ///设置要渲染的Layer,消隐距离
        float[] distances = new float[32];
        for (int i = 0; i != 32; ++i)
        {
            distances[i] = 0;
            if (1 == ((1 << i & projCam.cullingMask) >> i))
                distances[i] = cullDistance;
        }
        projCam.layerCullDistances = distances;

        ///
        //projCam.layerCullSpherical = true;

        ///背景色黑色
        projCam.clearFlags = CameraClearFlags.SolidColor;
        projCam.backgroundColor = Color.black;

        ///正交投影
        ///正交投影矩阵并不会将场景用透视图进行变形，所有视线 / 光线都是平行的，这使它对于定向光来说是个很好的投影矩阵。
        projCam.orthographic = true;

        ///创建相机渲染纹理 https://blog.csdn.net/leonwei/article/details/54972653
        ///RenderTexture本质上一个类似一个FrameBufferObject(包含color、depth、stencil buffer等)
        ///depthBuffer为16,format为RenderTextureFormat.Depth 
        ///=》只输出depthbuffer信息，作为阴影信息；需要渲染场景的深度信息，颜色缓冲没用到，显示告诉OpenGL我们不进行任何颜色数据进行渲染
        ///待测试 使用链接的第4种方式使用RenderTexture，进一步优化

        //rt = RenderTexture.GetTemporary(texWidth, texWidth, 0, RenderTextureFormat.Depth);
        rt = new RenderTexture(texWidth, texWidth, 16, RenderTextureFormat.Depth);
        rt.name = "projectorRT";
        projCam.targetTexture = rt;

        ///不用相机生成深度图，通过自定义shader渲染一张深度图
        projCam.depthTextureMode = DepthTextureMode.None;
        ///其它设置
        projCam.renderingPath = RenderingPath.Forward;
    }

    /// <summary>
    /// 投影设置
    /// </summary>
    void setProj()
    {
        if (!proj || !projCam) return;
        projCam.farClipPlane = proj.farClipPlane;
        projCam.nearClipPlane = proj.nearClipPlane;
        projCam.orthographic = proj.orthographic;
        //projCam.fieldOfView = proj.fieldOfView;
        projCam.orthographicSize = proj.orthographicSize;

        proj.ignoreLayers = projIgnoreLayers.value;


        ///阴影渲染设置
        Shader shader = Shader.Find("Unlit/ShadowShow");
        Material m = new Material(shader);
        m.SetTexture("_DepthTex", rt);
        m.SetTexture("_FadeTex", texFade);
        //m.SetFloat("", 1);
        //m.SetFloat("", 1);
        m.name = "projectorMat";
        proj.GetComponent<Projector>().material = m;

    }

    /// <summary>
    /// 自定义渲染
    /// </summary>
    void customeRender()
    {
        if (!projCam || !projShader) return;

        //渲染的顶点做翻转操作，不会对法线，待测试
        //GL.invertCulling = true;
        ///手动渲染，渲染特定shader的物体
        ///使用replaceShader是为了避免渲染“轮廓”图的时候还是用原来复杂的shader，照成不必要的性能浪费。替换后我们只输出纯色（方便测试）或者alpha值到“轮廓”图。
        projCam.RenderWithShader(projShader, "ShadowProj");
        //GL.invertCulling = false;
    }
}

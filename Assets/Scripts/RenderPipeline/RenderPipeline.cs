/*
 * 场景渲染管线
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderPipeline
{
    private static RenderPipeline m_inst;
    public static RenderPipeline Inst
    {
        get
        {
            if (m_inst == null)
                m_inst = new RenderPipeline();

            return m_inst;
        }
    }

    public const string pipelineName = "RenderPipelineRT";

    //所有可以参与渲染的节点，排序按（Cameras with lower depth are rendered before cameras with higher depth.）
    private List<RenderNode> AllNodes = new List<RenderNode>();
    //当前帧参与渲染的节点
    private List<RenderNode> FrameNodes = new List<RenderNode>();

    //场景相机渲染RT,从第一个渲染节点的OnPreRender创建
    public RenderTexture sceneCameraRt;
    private bool renderStart = false;

    //1.准备RT，依据当前设置的分辨率准备RT
    private void PrepareRT()
    {
        if (sceneCameraRt != null)
        {
            RenderTexture.ReleaseTemporary(sceneCameraRt);
            sceneCameraRt = null;
        }

        //RT是一张特殊的texture，可以real-time将camera拍摄的画面写入texture。
        //实现：将FBO直接关联一个gpu上的texture对象，这样就等于在绘制时就直接绘制到这个texure上，这样也省去了拷贝时间，gles中一般是使用FramebufferTexture2D（）这样的接口。
        //RT作为"native engine object" 类型，不会被GC释放，所以留心RT的生存时间，及时手动释放RT就非常重要了。
        //如果用blit做一系列的后处理，最高效的方法是在每一次blit的时候都获取和释放RT（unity有一个RT pool）。而不是提前声明一个RT并且反复使用。这对于移动平台尤其有益。
        //可以使用camera的特性比如mask;整个屏幕的后处理post-processing,Unity提供方便接口直接把需要渲染的画面写入RT。高效：因为不用拷贝操作就可以获得RT，并且对texture处理后直接输出在屏幕。
        //

        //GrabPass  Unity shader中内置的特殊pass，可以实现与RT类似功能，但原理、用法和效率有不同。
        //把当前整个屏幕的back buffer复制写入一张texture，（并不包括当前shader正在渲染的物体），这张texture可以被用在接下来的pass中做一些处理。
        //如果是被命名的texture，同样可以用在其他shader中。通常用作处理屏幕部分区域特效，比如扭曲火焰，毛玻璃。,效率不如RT，但使用方便，不用手动控制grab时机。



        sceneCameraRt = RenderTexture.GetTemporary(
            1136, 640, //此处可以扩展成动态分辨率 根据机型高中低
            16, //此处可以扩展成动态深度 根据机型高中低
            RenderTextureFormat.Default, 
            RenderTextureReadWrite.Default, 
            1//采样点 抗锯齿 可以配置
            //RenderTextureMemoryless. 待测试
            );

        //newRt.filterMode = FilterMode.Point;
        sceneCameraRt.name = "RenderPipelineRT";

        //待实现
        //需要把RT关联到每一个相机上
    }

    //2.在摄像机开始渲染场景之前调用,按照渲染的优先顺序调用（unity 先调用depth小的相机）
    public void OnPreRender(RenderNode node)
    {
        Camera cam = node.camera;
        if (!renderStart)
        {
            //第一个渲染，添加所有的相机，移除渲染开始时不激活的相机
            //渲染开始后，有新的相机激活，不会触发渲染
            //渲染开始后，有相机关闭，这个相机这帧不会渲染
            //frameCameras在开始的时候，记录所有这一帧要发生渲染的相机，每个相机渲染完后，移除这个相机
            //并且删掉不激活的相机，如果frameCamears空，则说明是最后一个相机，blit到屏幕
            renderStart = true;
            FrameNodes.Clear();
            FrameNodes.AddRange(AllNodes);
            FrameNodes.RemoveAll(NotValidCamera);

            cam.clearFlags = CameraClearFlags.Skybox;
            PrepareRT();
        }
        else
        {
            //不清理
            cam.clearFlags = CameraClearFlags.Nothing;
        }
    }

    //3.在摄像机完成场景渲染之后调用
    public void OnPostRender(RenderNode node)
    {
        FrameNodes.Remove(node);
        FrameNodes.RemoveAll(NotValidCamera);
        if(FrameNodes.Count == 0)
        {
            //后续没有相机了，渲染到屏幕
            if(true)
            {
                //处理后效
                Graphics.Blit(sceneCameraRt, null as RenderTexture);
            }
            else
            {
                //
                Graphics.Blit(sceneCameraRt, null as RenderTexture);
            }

            //每帧用rt，记得释放 防止内存泄漏
            RenderTexture.ReleaseTemporary(sceneCameraRt);
            sceneCameraRt = null;

            //处理渲染完成之后的逻辑,回调可以在这个后面加

        }
    }

    //4.在场景渲染完成之后允许屏幕图像后期处理调用。Pro Only ，不在这里处理后效了
    public void OnRenderImage()
    {
        //unity，实现屏幕后处理的两种方法,本方案用的是rt第二种

        //方法一：
        //Main Camera的Target Texture保持为None。挂一个Blit脚本，在其中的OnRenderImage中调用Graphics.Blit(sourceTexture, destTexture, myMaterial)。
        //需要注意的是myMaterial中的shader一定要用ZWrite Off、ZTest Always的shader。

        //方法二：
        //为Main Camera的Target Texture指定一个RT，直接渲染到RT，同时将RT赋给一个quad并用一个正交相机对准它。另外为了实现后处理，quad的material需要选一个适当的后处理shader。
    }

    public void AddNode(RenderNode node)
    {
        AllNodes.Add(node);
    }

    public void RemoveNode(RenderNode node)
    {
        AllNodes.Remove(node);
    }

    private bool NotValidCamera(RenderNode node)
    {
        return node == null || !node.isActiveAndEnabled
            || node.camera == null
            || !node.camera.isActiveAndEnabled;
    }
}

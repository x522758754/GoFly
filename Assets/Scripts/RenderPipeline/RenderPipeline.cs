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

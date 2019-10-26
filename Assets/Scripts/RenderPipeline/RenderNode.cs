using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RenderNode : MonoBehaviour
{
    public Camera camera { get; private set; }
    private void Awake()
    {
        camera = GetComponent<Camera>();
        RenderPipeline.Inst.AddNode(this);
    }

    private void OnDestroy()
    {
        RenderPipeline.Inst.RemoveNode(this);
    }

    //2.在摄像机开始渲染场景之前调用,按照渲染的优先顺序调用（unity 先调用depth小的相机）
    public void OnPreRender()
    {
        RenderPipeline.Inst.OnPreRender(this);
    }

    //3.在摄像机完成场景渲染之后调用
    public void OnPostRender()
    {
        RenderPipeline.Inst.OnPostRender(this);
    }

    //4.在场景渲染完成之后允许屏幕图像后期处理调用。Pro Only
    public void OnRenderImage()
    {

    }
}

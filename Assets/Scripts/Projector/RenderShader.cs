using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderShader : MonoBehaviour
{
    public Shader projShader;
    public Camera projCam;
    public LayerMask setLayers; //设置渲染目标的Layer
    public float cullDistance = 32f; //层消隐距离

    private void Start()
    {
        if (projCam)
        {
            ///canera自定义设置

            ///关闭自动渲染，改成手动渲染
            ///防止camera 发送其它渲染事件，被其它脚本捕捉，加工处理
            projCam.enabled = false;

            ///设置要渲染的Layers
            projCam.cullingMask = setLayers.value;

            ///设置要渲染的Layer,消隐距离
            float[] distances = new float[32];
            for (int i=0; i != 32; ++i)
            {
                distances[i] = 0;
                if(1 == ((1 << i & projCam.cullingMask) >> i))
                    distances[i] = cullDistance;

            }
            projCam.layerCullDistances = distances;

            ///
            //projCam.layerCullSpherical = true;

            ///背景色黑色
            projCam.clearFlags = CameraClearFlags.SolidColor;
            projCam.backgroundColor = Color.black;
            
            ///其它设置
            projCam.depthTextureMode = DepthTextureMode.None;
            projCam.renderingPath = RenderingPath.Forward;
        }

    }

    private void LateUpdate()
    {
        if (projCam && projShader)
        {
            //渲染的顶点做翻转操作，不会对法线，待测试
            GL.invertCulling = true;
            ///手动渲染，渲染特定shader的物体
            projCam.RenderWithShader(projShader, "ShadowProj");
            GL.invertCulling = false;
        }
    }

}

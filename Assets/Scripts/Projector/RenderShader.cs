using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderShader : MonoBehaviour
{
    public Shader shaderProj;
    public Camera cameraProj;
    public List<LayerMask> layers; //设置渲染目标的Layer

    private void Start()
    {
        if (!cameraProj)
        {
            //关闭自动渲染，改成手动渲染
            cameraProj.enabled = false;
            //设置要渲染的Layer,消隐距离
            float[] distances = new float[32];
            for (int i=0; i !=  32; ++i)
            {
                distances[i] = 0;
            }
            for(int i=0; i != layers.Count; ++i)
            {
                int iLayer = layers[i].value;
                distances[iLayer] = 32;
            }
            cameraProj.layerCullDistances = distances;
            cameraProj.depthTextureMode = DepthTextureMode.None;
            cameraProj.backgroundColor = Color.black;
            cameraProj.clearFlags = CameraClearFlags.SolidColor;
            cameraProj.renderingPath = RenderingPath.Forward;
        }

    }

    private void LateUpdate()
    {
        if (cameraProj && shaderProj)
        {
            GL.invertCulling = true; //渲染的顶点做翻转操作，不会对法线，待测试
            cameraProj.RenderWithShader(shaderProj, "ShadowProj");
            GL.invertCulling = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMinimap : MonoBehaviour
{
    public MinimapData minimapData; //当前小地图信息
    ///坐标轴空间变换
    ///world ------> 
    ///minimapRotated ------> 
    ///minimapNotRotated ------> 
    ///tex --- 长宽改变 --->
    ///ui --- 原点改变 --->
    ///Screen
    ///至此屏幕坐标,可以转化到ui上

    public Vector2 Tex2Custom(Vector2 texPos, float w, float h)
    {
        ///scale
        ///sx   0   0
        ///0    sy  0
        ///0    0   0

        /// texPos.x texPos.y 1

        float sx = w / minimapData.texWidth;
        float sy = h / minimapData.texHeight;

        Vector2 custom;
        custom.x = sx * texPos.x;
        custom.y = sy * texPos.y;

        return custom;
    }

    public Vector2 Custom2Tex(Vector2 customPos, float w, float h)
    {

        ///scale
        ///sx   0   0
        ///0    sy  0
        ///0    0   0

        /// texPos.x texPos.y 1
        float sx = w / minimapData.texWidth;
        float sy = h / minimapData.texHeight;

        Vector2 custom;
        custom.x = 1/sx * customPos.x;
        custom.y = 1/sy * customPos.y;

        return custom;
    }
    public Vector2 Tex2Screen(Vector2 texPos)
    {
        float w = Screen.width;
        float h = Screen.height;

        return Tex2Custom(texPos, w, h);
    }

    public Vector2 Screen2Tex(Vector2 screenPos)
    {
        float w = Screen.width;
        float h = Screen.height;

        return Custom2Tex(screenPos, w, h);
    }

    public Vector2 Minimap2Tex(Vector2 minimapPos)
    {
        //minimapPos(Rotated) -> minimapPos(Not Rotated)
        ///
        /// xAsix.x yAsix.x origin.x
        /// xAsix.y yAsix.y origin.y
        /// 0       0       1
        //minimapPos(Not Rotated) -> texPos 
        /// scale
        /// s   0   0
        /// 0   s   0
        /// 0   0   1
        /// =>
        /// s*xAsix.x s*yAsix.x s*origin.x
        /// s*xAsix.y s*yAsix.y s*origin.y
        /// 0         0         1
        /// minimapPos.x minimapPos.y 1.0

        Vector2 lbPos = World2Minimap(minimapData.lbPos);
        Vector2 rtPos = World2Minimap(minimapData.rtPos);
        Vector2 rbPos = World2Minimap(minimapData.rbPos);

        Vector2 xAsix = (rbPos - lbPos).normalized;
        Vector2 yAsix = (rtPos - rbPos).normalized;
        Vector2 origin = World2Minimap(minimapData.lbPos);
        float s = minimapData.texWidth / minimapData.realWidth;

        Vector2 texPos;
        texPos.x = s * xAsix.x * minimapPos.x + s * yAsix.x * minimapPos.y + s * origin.x;
        texPos.y = s * xAsix.y * minimapPos.x + s * yAsix.y * minimapPos.y + s * origin.y;

        return texPos;
    }

    public Vector2 Tex2Minimap(Vector2 texPos)
    {
        /// texPos = Mm2t * minimapPos
        /// Mm2t-1 texPos = minimapPos

        /// Mm2t-1
        /// 1/sx    0       0
        /// 0       1/sy    0
        /// 0       0       1

        float s = minimapData.texWidth / minimapData.realWidth;

        Vector2 minimapPos;
        minimapPos.x = 1 / s * texPos.x;
        minimapPos.y = 1 / s * texPos.y;

        return minimapPos;
    }

    public Vector2 Minimap2World(Vector2 minimapPos)
    {
        /// worldPos = Mm2w * minimapPos
        /// Mm2w-1 * worldPos = minimapPos
        
        /// Mm2w: minimap -> world
        /// xAsix.x yAsix.x origin.x
        /// xAsix.y yAsix.y origin.y
        /// 0       0       1
        
        Vector2 lb_worldPos = minimapData.lbPos;
        Vector2 rt_worldPos = minimapData.rtPos;
        Vector2 rb_worldPos = minimapData.rbPos;

        Vector2 xAsix = (rb_worldPos - lb_worldPos).normalized;
        Vector2 yAsix = (rt_worldPos - rb_worldPos).normalized;
        Vector2 origin = minimapData.lbPos;
        ///minimapPos.x minimapPos.y 1.0

        Vector2 worldPos;
        worldPos.x = xAsix.x * minimapPos.x + yAsix.x * minimapPos.y + origin.x;
        worldPos.y = xAsix.y * minimapPos.x + yAsix.y * minimapPos.y + origin.y;

        return worldPos;
    }

    public Vector2 World2Minimap(Vector2 worldPos)
    {
        ///Mm2w-1
        /// xAsix.x xAsix.y -dot(origin, xAsix)
        /// yAsix.x yAsix.y -dot(origin, yAsix)
        /// 0       0       1

        Vector2 lb_worldPos = minimapData.lbPos;
        Vector2 rt_worldPos = minimapData.rtPos;
        Vector2 rb_worldPos = minimapData.rbPos;

        Vector2 xAsix = (rb_worldPos - lb_worldPos).normalized;
        Vector2 yAsix = (rt_worldPos - rb_worldPos).normalized;
        Vector2 origin = minimapData.lbPos;
        /// worldPos.x worldPos.y 1.0

        Vector2 minimapPos;
        minimapPos.x = xAsix.x * worldPos.x + xAsix.y * worldPos.y - Vector2.Dot(origin, xAsix);
        minimapPos.y = yAsix.x * worldPos.x + yAsix.y * worldPos.y - Vector2.Dot(origin, yAsix);

        return minimapPos;
    }

    public Vector2 World2Tex(Vector2 worldPos)
    {
        Vector2 minimapPos = World2Minimap(worldPos);
        Vector2 texPos = Minimap2Tex(minimapPos);

        return texPos;
    }

    public Vector2 Tex2World(Vector2 texPos)
    {
        Vector2 minimapPos = Tex2Minimap(texPos);
        Vector2 worldPos = Minimap2World(minimapPos);

        return worldPos;
    }
}

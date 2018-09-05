Shader "Unlit/Test"
{
	Properties{
		_Color("Color Tint", Color) = (1, 1, 1, 1)
		_AlphaScale("Alpha Scale", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry"  "RenderType" = "Transparent" }
		Pass
		{
			//关闭深度写入
			ZWrite Off
			//开启透明度混合
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			fixed4 _Color;
			fixed _AlphaScale;

			struct a2v {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 pos : SV_POSITION;
			};

			v2f vert(a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 albedo = _Color.rgb;

				//在这里设置透明度通道，必须使用Blend命令，设置透明度通道才有意义
				return fixed4(albedo,  _AlphaScale);
			}
			ENDCG
		}
	}
}
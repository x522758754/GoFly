///阴影创建-获取渲染阴影所需的深度信息
Shader "Unlit/ShadowCreate"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "ShadowProj" = "Projector" }
		LOD 100

		Pass
		{
			//剔除背面,渲染阴影只需一半即可
			Cull Front
			//开启Z buffer
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				return o;
			}
			
			//fragment shader一般不会覆盖z buffer值，采用光栅化的默认值
			//自己重写z buffer值，会让GPUs关闭对depth buffer的优化。
			//成本类似于alpha testing中使用内置的clip()
			//fixed frag (v2f i) : sv_depth
			//{	
			//	return 1.0;
			//}

			//使用渲染物体默认的depth值
			fixed4 frag(v2f i):SV_Target
			{
				return fixed4(0, 0, 0, 0);
			}

			ENDCG
		}
	}
}

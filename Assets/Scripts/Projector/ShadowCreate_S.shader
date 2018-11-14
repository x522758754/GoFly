///阴影创建-获取渲染阴影所需的深度信息
Shader "Unlit/ShadowCreate_S"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" "ShadowProj" = "Projector" }
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

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);

		return o;
	}

	//使用渲染物体默认的depth值
	fixed4 frag(v2f i) :SV_Target
	{
		fixed4 col = EncodeFloatRGBA(i.vertex.z/ i.vertex.w); // i.vertex.z/ i.vertex.w 这个可以移到vs下做 节省性能
		return col;
	}

		ENDCG
	}
	}
}

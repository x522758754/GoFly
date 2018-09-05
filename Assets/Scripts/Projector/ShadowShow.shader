///阴影显示 -- 调控阴影的渲染
Shader "Unlit/ShadowShow"
{
	Properties
	{
		_DepthTex("DepthTex", 2D) = "black" {}
		_FadeTex ("FadeTex", 2D) = "black" {}
		_AlphaScale("Alpha Scale", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent"}
		LOD 100

		Pass
		{
			// don't change depths
			//保证我们不会改变depth buffer，因为我们添加投影到目标物上，而该目标物早已光栅化完了
			ZWrite Off
			//混合时注意目标物的shader执行顺序，应在当前shader之前执行
			//Blending就是控制透明的。处于光栅化的最后阶段。
			//Blend SrcFactor DstFactor,SrcFactorA DstFactorA
			//混合颜色 = 当前片元颜色*SrcFactor  + 存在颜色缓冲里面的颜色*DstFactor
			Blend DstColor Zero
			//ColorMask RGB

			// avoid depth fighting
			//轻微的改变投影深度值,使我们当前的投影在目标物之前，确保不会被目标物挡到
			Offset -1, -1

			//AlphaTest Greater 0
			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
			};
	 
			struct v2f
			{
				half4 pos : SV_POSITION;
				half4 posProj:TEXCOORD0; // position in projector space
				half4 fposProj:TEXCOORD1; //
			};

			//transformation matrix, from object space to projector space 
			//projector space类似light space,参见opengl中shadowMapping.hpp
			half4x4 unity_Projector; 
			
			half4x4 unity_ProjectorClip;

			sampler2D _DepthTex;
			sampler2D _FadeTex;
			fixed _AlphaScale;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.posProj = mul(unity_Projector, v.vertex);
				o.fposProj = mul(unity_ProjectorClip, v.vertex);
				return o;
			}
			
			fixed4 frag(v2f i):SV_Target
			{
				fixed4 projTex = tex2D(_DepthTex, i.posProj.xy / i.posProj.w); //透视除法:i.posProj.xy / i.posProj.w
				//alternative: fixed4 tex = tex2Dproj(_DepthTex, i.posProj);
				
				fixed fade = tex2D(_FadeTex, i.posProj.xy / i.posProj.w).r; //阴影衰减方式1：边缘衰减
				fixed fadeout = tex2D(_FadeTex, i.fposProj.xy / i.fposProj.w).g; ////阴影衰减方式1：远近衰减
				
				fixed4 result;
				result.rgb = lerp(fixed3(1, 1, 1), 1 - projTex.r, fadeout)*1.3;
				result.rgb += 1 - fade;
				result.a = 1;
				return result;
			}

			ENDCG
		}
	}
}

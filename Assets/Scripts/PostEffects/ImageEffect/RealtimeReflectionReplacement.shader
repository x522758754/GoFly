/* 

replacement shaders for ...

cheaper/better rendering of reflections for low end / mobile platforms. we will 
only render the types tagged here into the reflection buffer, thus freeing up
many CPU and GPU cycles.

*/

Shader "Fairy Tails/RealtimeReflectionReplacement" {
	
Properties {
	_MainColor("MainColor", Color) = (1,1,1,1)
	_MainTex("MainTex", 2D) = "white" {}
	_Normal("Normal", 2D) = "bump" {}
	_ColorR("ColorR", Color) = (1,1,1,1)
	_MaskMap("MaskMap", 2D) = "black" {}


/////////////////////////////////////////////////
	//_FadeTex("FadeTex", 2D) = "white" {}
	//_MaskIntensity("Mask Intensity", Range(1,25)) = 25
	//_ReflectionIntensity("Reflection Intensity", Range(0,0.5)) = 0.4
}

CGINCLUDE		

struct v2f 
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	//half4 sproj:TEXCOORD1;
	//half4 fproj:TEXCOORD2;
};

struct v2f_full
{
	half4 pos : SV_POSITION;
	half4 uv : TEXCOORD0;
	half3 tsBase0 : TEXCOORD2;
	half3 tsBase1 : TEXCOORD3;
	half3 tsBase2 : TEXCOORD4;	
	half3 viewDirNotNormalized : TEXCOORD5;
};
	
#include "UnityCG.cginc"	
uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;
//sampler2D _MainTex;
sampler2D _Normal;		
uniform fixed4 _MainColor;
uniform fixed4 _ColorR;
uniform sampler2D _MaskMap; 
uniform float4 _MaskMap_ST;

//sampler2D _FadeTex;
//half _MaskIntensity;
//half _ReflectionIntensity;
//
//half4x4 unity_Projector;
//half4x4 unity_ProjectorClip;

ENDCG 


SubShader {
	Tags { "Reflection" = "RenderReflectionOpaque" }
	LOD 200 
    Fog { Mode Off }
	
	Pass {
		
		CGPROGRAM

				
		v2f vert (appdata_full v) 
		{
			v2f o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv = v.texcoord;
			//o.sproj = mul(unity_Projector, v.vertex);
			//o.fproj = mul(unity_ProjectorClip, v.vertex);
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{ 
			//
			float4 col = tex2D(_MainTex,TRANSFORM_TEX(i.uv, _MainTex));
			float4 m = tex2D(_MaskMap,TRANSFORM_TEX(i.uv, _MaskMap));
			float3 Diff = lerp(col.rgb,(col.rgb*_ColorR.rgb),m.r);
			return float4(Diff.rgb, col.a);
			//half fade = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.sproj)).r;
			//half fadeout = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.fproj)).g;
			//half4 result;
			//result.rgb = lerp(fixed3(26 - _MaskIntensity, 26 - _MaskIntensity, 26 - _MaskIntensity), Diff.rgb, fadeout);
			//
			//result.rgb = lerp(10, Diff.rgb, fadeout);
			//result.rgb += 1 - fade;
			//return float4(result.rgb, col.a);
		}	
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
	
		ENDCG
	}
} 

SubShader {
	Tags { "Reflection" = "RenderReflectionTransparentAdd" }
	
	LOD 200 
    Fog { Mode Off }
	Blend One One
	Cull Off
	ZWrite Off
	ZTest Always
	
	Pass {
        Cull Off
		
		CGPROGRAM

				
		v2f vert (appdata_full v) 
		{
			v2f o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv = v.texcoord;

			//o.sproj = mul(unity_Projector, v.vertex);
			//o.fproj = mul(unity_ProjectorClip, v.vertex);
			return o; 
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			//
			float4 col = tex2D(_MainTex,TRANSFORM_TEX(i.uv, _MainTex));
			float4 m = tex2D(_MaskMap,TRANSFORM_TEX(i.uv, _MaskMap));
			float3 Diff = lerp(col.rgb,(col.rgb*_ColorR.rgb),m.r);
			return float4(Diff.rgb, col.a);
			//half fade = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.sproj)).r;
			//half fadeout = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.fproj)).g;
			//half4 result;
			//result.rgb = lerp(fixed3(26 - _MaskIntensity, 26 - _MaskIntensity, 26 - _MaskIntensity), Diff.rgb, fadeout);
			//
			//result.rgb = lerp(10, Diff.rgb, fadeout);
			//result.rgb += 1 - fade;
			//return float4(result.rgb, col.a);
		}	
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
	
		ENDCG
	}
} 

SubShader {
	Tags { "Reflection" = "RenderReflectionTransparentBlend" }
	
	LOD 200 
    Fog { Mode Off }
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Off
	ZWrite Off
	
	Pass {
        Cull Off
		
		CGPROGRAM

		v2f vert (appdata_full v) 
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv = v.texcoord;
			//o.sproj = mul(unity_Projector, v.vertex);
			//o.fproj = mul(unity_ProjectorClip, v.vertex);
			return o;
		}		
		
		fixed4 frag (v2f i) : COLOR0 
		{
			//
			float4 col = tex2D(_MainTex,TRANSFORM_TEX(i.uv, _MainTex));
			float4 m = tex2D(_MaskMap,TRANSFORM_TEX(i.uv, _MaskMap));
			float3 Diff = lerp(col.rgb,(col.rgb*_ColorR.rgb),m.r);
			return float4(Diff.rgb, col.a);
			//half fade = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.sproj)).r;
			//half fadeout = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.fproj)).g;
			//half4 result;
			//result.rgb = lerp(fixed3(26 - _MaskIntensity, 26 - _MaskIntensity, 26 - _MaskIntensity), Diff.rgb, fadeout);
			//
			//result.rgb = lerp(10, Diff.rgb, fadeout);
			//result.rgb += 1 - fade;
			//return float4(result.rgb, col.a);
		}	
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
	
		ENDCG
	}
} 
FallBack Off
}


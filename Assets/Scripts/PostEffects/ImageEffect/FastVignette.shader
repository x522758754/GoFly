// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

Shader "Fairy Tails/Fast Vignette"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Params ("Center (XY) Sharpness (Z) Darkness (W)", Vector) = (0.5, 0.5, 0.1, 0.3)
		_Color ("Vignette Color (RGB)", Color) = (0, 0, 0, 0)
	}

	CGINCLUDE

		#include "UnityCG.cginc"
		sampler2D _MainTex;
		half4 _Params;
		half4 _Color;

		half4 frag_rgb(v2f_img i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			half d = distance(i.uv, _Params.xy);
			half multiplier = smoothstep(0.8, _Params.z * 0.799, d * (_Params.w + _Params.z));
			multiplier+=0.01;
			half3 c = lerp(_Color, color.rgb, (1/_Color.a)*multiplier);
			return half4(c, color.a);
		}

		half4 frag_rgb1(v2f_img i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			half d = distance(i.uv, _Params.xy);
			half multiplier = smoothstep(0.8, _Params.z * 0.799, d * (_Params.w + _Params.z));
			multiplier += 0.01;
			half3 c = lerp(_Color, color.rgb, (1 / _Color.a)*(1-multiplier));
			return half4(c, color.a);
		}

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		Pass
		{
			CGPROGRAM
		
				#pragma vertex vert_img
				#pragma fragment frag_rgb
				#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag_rgb1
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
	}

	FallBack off
}


Shader "Fairy Tails/EasyDofDebug" {
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_BlurSpread("Blur Spread", float) = 1
		_BlurDistance("Blur Distance", float) = 0.001
    }

 SubShader
 {
     CGINCLUDE
#include "UnityCG.cginc"
     sampler2D _MainTex;
     float4 _MainTex_TexelSize;
     uniform float _BlurSpread;
	 uniform float _BlurDistance;

	 sampler2D_float _CameraDepthTexture;
	 half4 _CameraDepthTexture_ST;

	 //sampler2D_float  _DepthBuffer;
	 //half4 _DepthBuffer_ST;

	 sampler2D _BlurTexture;
	 half4 _BlurTexture_ST;
     struct appdata
     {
         float4 vertex : POSITION;
         float2 uv : TEXCOORD0;
     };

	struct v2f
	{
	    float4 pos : SV_POSITION;
	    float2 uv[5] : TEXCOORD0;
	};

   v2f vertHorizontal(appdata v)
   {
       v2f o;
       o.pos = UnityObjectToClipPos(v.vertex);
       float tsx = _MainTex_TexelSize.x * _BlurSpread;
       o.uv[0] = v.uv + float2(tsx * -2, 0);
       o.uv[1] = v.uv + float2(tsx * -1, 0);
       o.uv[2] = v.uv;
       o.uv[3] = v.uv + float2(tsx * 1, 0);
       o.uv[4] = v.uv + float2(tsx * 2, 0);
       return o;
   }

	v2f vertVertical(appdata v)
	{
	    v2f o;
	    o.pos = UnityObjectToClipPos(v.vertex);
	    float tsy = _MainTex_TexelSize.y * _BlurSpread;
	    o.uv[0] = v.uv + float2(0, tsy * -2);
	    o.uv[1] = v.uv + float2(0, tsy * -1);
	    o.uv[2] = v.uv;
	    o.uv[3] = v.uv + float2(0, tsy * 1);
	    o.uv[4] = v.uv + float2(0, tsy * 2);
	    return o;
	}

   fixed4 frag(v2f i) : SV_TARGET
   {
       float g[3] = { 0.0545, 0.2442, 0.4026 };
       fixed4 col = tex2D(_MainTex, i.uv[2]) * g[2];
       for (int k = 0; k < 2; k++)
           {
               col += tex2D(_MainTex, i.uv[k]) * g[k];
               col += tex2D(_MainTex, i.uv[4 - k]) * g[k];
           }
       return col;
   }

	fixed4 fragdof(v2f_img  i) : SV_TARGET
	{
	   fixed4 renderTex = tex2D(_MainTex, i.uv);
	   fixed2 _XY = i.uv;
#if SHADER_API_D3D9
   if (_MainTex_TexelSize.y < 0.0 )
	   _XY.y = 1 - _XY.y;
#endif

	
		
	   fixed4 blurTex = tex2D(_BlurTexture, _XY);
	   //return blurTex;
   
	   float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(_XY, _MainTex_ST));
	   d = Linear01Depth(d);
	   float v = saturate(abs(d - _BlurDistance) *0.6);
	   //return fixed4(d, d, d, 1);
	   return lerp(renderTex, blurTex, v);
    }

   ENDCG

   Pass
   {
       Name "HORIZONTAL"
           ZTest Always
           ZWrite Off
           Cull Off

           CGPROGRAM
		   #include "UnityCG.cginc"
           #pragma vertex vertHorizontal
           #pragma fragment frag
           ENDCG
   }

   Pass
   {
       Name "VERTICAL"
           ZTest Always
           ZWrite Off
           Cull Off

           CGPROGRAM
		   #include "UnityCG.cginc"
           #pragma vertex vertVertical
           #pragma fragment frag
           ENDCG
     }
   Pass
   {
	   Name "DOF"
		   ZTest Always
		   ZWrite Off
		   Cull Off

		   CGPROGRAM
		   #include "UnityCG.cginc"
		   #pragma vertex vert_img
		   #pragma fragment fragdof
		   ENDCG
   }
}

   Fallback Off

}

﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UnlitInvertedAlpha" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "black" {}
		_Alpha("Alpha", Range(0,1)) = 1
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Cull front   
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		half2 texcoord : TEXCOORD0;
	};


	sampler2D _MainTex;

	float4 _MainTex_ST;
	float _Alpha;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		v.texcoord.x = 1 - v.texcoord.x;
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.texcoord);
		col.a =  _Alpha;
	return col;
	}
		ENDCG
	}
	}

}
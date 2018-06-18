// Copyright (C) Itseez3D, Inc. - All Rights Reserved
// You may not use this file except in compliance with an authorized license
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
// See the License for the specific language governing permissions and limitations under the License.
// Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017

// Shader that draws haircuts with transparent texture parts.
Shader "AvatarUnlitHairShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorTarget("ColorTarget", Vector) = (1, 1, 1, 0)
		_ColorTint ("ColorTint", Vector) = (0, 0, 0, 0)
		_TintCoeff ("TintCoeff", Float) = 0.8
		_MultiplicationTintThreshold("MultiplicationTintThreshold", Float) = 0.2
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
		AlphaToMask On
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Input 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _ColorTarget;
			fixed4 _ColorTint;
			float _TintCoeff;
			float _MultiplicationTintThreshold;

			Input vert(appdata v)
			{
				Input o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (Input i) : COLOR
			{
				fixed4 colorTexture = tex2D(_MainTex, i.uv);
				float threshold = _MultiplicationTintThreshold;
				fixed4 tinted = colorTexture + _TintCoeff * _ColorTint;

				float maxTargetChannel = max(_ColorTarget.r, max(_ColorTarget.g, _ColorTarget.b));
				if (maxTargetChannel < threshold)
				{
					float darkeningCoeff = min(0.85, (threshold - maxTargetChannel) / threshold);
					tinted = (1.0 - darkeningCoeff) * tinted + darkeningCoeff * (_ColorTarget * colorTexture);
				}

				return fixed4(tinted.rgb, colorTexture.a);
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}

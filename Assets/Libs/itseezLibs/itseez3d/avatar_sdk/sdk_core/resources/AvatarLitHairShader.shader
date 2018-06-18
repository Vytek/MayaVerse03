// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Copyright (C) Itseez3D, Inc. - All Rights Reserved
// You may not use this file except in compliance with an authorized license
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
// See the License for the specific language governing permissions and limitations under the License.
// Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017

Shader "AvatarLitHairShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Cull ("Culling: 0-Off, 1-Front, 2-Back", Int) = 2
		_ColorTarget("ColorTarget", Vector) = (1, 1, 1, 0)
		_ColorTint ("ColorTint", Vector) = (0, 0, 0, 0)
		_TintCoeff ("TintCoeff", Float) = 0.8
		_MultiplicationTintThreshold("MultiplicationTintThreshold", Float) = 0.2
	}

	SubShader
	{
		Pass
		{
			Tags {"LightMode"="ForwardBase" "Queue"="Transparent" "RenderType"="TransparentCutout" "IgnoreProjector"="True"}
			AlphaToMask On
			Cull [_Cull]

			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 lightDir : TEXCOORD1;
				float3 normal : TEXCOORD2;
				fixed3 ambientColor : COLOR0;
				LIGHTING_COORDS(3,4)
				float3 vertexLighting : TEXCOORD5;
			};


			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Cutoff;

			// recoloring
			fixed4 _ColorTarget;
			fixed4 _ColorTint;
			float _TintCoeff;
			float _MultiplicationTintThreshold;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.normal = v.normal;

				TRANSFER_VERTEX_TO_FRAGMENT(o);

				o.vertexLighting = float3(0, 0, 0);

				float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL));
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

				o.ambientColor = ShadeSH9(half4(worldNormal, 1));

				return o;
			}

			fixed4 frag (v2f i) : SV_TARGET
			{
				i.lightDir = normalize(i.lightDir);
				fixed atten = LIGHT_ATTENUATION(i); 

				fixed4 tex = tex2D(_MainTex, i.uv);

				float threshold = _MultiplicationTintThreshold;
				fixed4 tinted = tex + _TintCoeff * _ColorTint;

				float maxTargetChannel = max(_ColorTarget.r, max(_ColorTarget.g, _ColorTarget.b));
				if (maxTargetChannel < threshold)
				{
					float darkeningCoeff = min(0.85, (threshold - maxTargetChannel) / threshold);
					tinted = (1.0 - darkeningCoeff) * tinted + darkeningCoeff * (_ColorTarget * tex);
				}

				fixed diff = saturate(dot(i.normal, i.lightDir));

				fixed4 c = tinted;
				c.rgb *= max(diff * atten, 0.4) + c.rgb * (i.vertexLighting +  i.ambientColor);
				return fixed4(c.rgb, tex.a);
			}

			ENDCG
		}

		Pass
		{
			Tags {"LightMode"="ForwardAdd" "Queue"="Transparent" "RenderType"="TransparentCutout" "IgnoreProjector"="True"}
			Blend One One
			AlphaToMask On
			Cull [_Cull]

			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 lightDir : TEXCOORD1;
				float3 normal : TEXCOORD2;
				LIGHTING_COORDS(3,4)
			};


			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.lightDir = ObjSpaceLightDir(v.vertex);

				o.normal = v.normal;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			fixed4 frag (v2f i) : SV_TARGET
			{
				i.lightDir = normalize(i.lightDir);
				fixed atten = LIGHT_ATTENUATION(i);
				fixed4 tex = tex2D(_MainTex, i.uv);

				fixed3 normal = i.normal;
				fixed diff = saturate(dot(normal, i.lightDir));

				fixed4 c;
				c.rgb = (tex.rgb * _LightColor0.rgb * diff * atten); 
				c.a = tex.a;
				return c;
			}
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True"}

			ZWrite On ZTest Less

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				V2F_SHADOW_CASTER; 
				float2 uv : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Cutoff;

			v2f vert(appdata v )
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag( v2f i ) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				if (col.a < 0.5f)
					discard;
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}

	Fallback "VertexLit"
	//FallBack "Diffuse"
}

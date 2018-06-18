// Upgrade NOTE: replaced 'defined VRWORKS_MRS' with 'defined (VRWORKS_MRS)'
// Upgrade NOTE: replaced 'defined VRWORKS_NONE' with 'defined (VRWORKS_NONE)'

#ifndef _VRWORKS_CGINC
#define _VRWORKS_CGINC

sampler2D _CameraDepthNormalsTexture;
sampler2D _CameraDepthTexture;
float4 _CameraDepthTexture_ST;

#ifdef VRWORKS_LMS

sampler2D _PluginUVRemapToLinearTexture;
sampler2D _PluginUVRemapFromLinearTexture;
float2 VRWorksUVToLinear(float2 uv) { return tex2D(_PluginUVRemapToLinearTexture, uv); }
float2 VRWorksUVFromLinear(float2 uv) { return tex2D(_PluginUVRemapFromLinearTexture, uv); }

sampler2D VRWorksGetDepthSampler()
{
  return _CameraDepthTexture;
}
sampler2D VRWorksGetDepthNormalsSampler()
{
  return _CameraDepthNormalsTexture;
}
float2 VRWorksRemapUV(float2 uv)
{
  float2 ruv = tex2D(_PluginUVRemapToLinearTexture, uv).xy;
  return TRANSFORM_TEX(ruv, _CameraDepthTexture); // needed in VR projects with double-wide render target
}
float2 VRWorksClampUV(float2 uv)
{
  return uv;// clamp(uv, _PluginUVClamp.xy, _PluginUVClamp.zw);
}
#ifdef SAMPLE_DEPTH_TEXTURE
#undef SAMPLE_DEPTH_TEXTURE
#	define SAMPLE_DEPTH_TEXTURE(sam, uv) (tex2D(sam, RemapUV(uv)).r)
#endif

#elif defined (VRWORKS_MRS)

float4 _PluginUVClamp;
sampler2D _PluginUVRemapToLinearTexture;
sampler2D _PluginUVRemapFromLinearTexture;
float2 VRWorksUVToLinear(float2 uv) { return tex2D(_PluginUVRemapToLinearTexture, uv); }
float2 VRWorksUVFromLinear(float2 uv) { return tex2D(_PluginUVRemapFromLinearTexture, uv); }

// Use low-res depth buffer since _MainTex is low-res in this case
sampler2D _PluginDepthNormalsTexture;
sampler2D _PluginDepthTexture;
sampler2D VRWorksGetDepthSampler()
{
  return _PluginDepthTexture;
}
sampler2D VRWorksGetDepthNormalsSampler()
{
  return _CameraDepthNormalsTexture;
}
float2 VRWorksClampUV(float2 uv)
{
  return clamp(uv, _PluginUVClamp.xy, _PluginUVClamp.zw);
}
float2 VRWorksRemapUV(float2 uv)
{
  return VRWorksClampUV(uv);
}

#ifdef SAMPLE_DEPTH_TEXTURE
#undef SAMPLE_DEPTH_TEXTURE
#	define SAMPLE_DEPTH_TEXTURE(sam, uv) (tex2D(_PluginDepthTexture, uv).r)
#endif

#else

// default implementation
sampler2D VRWorksGetDepthSampler()
{
  return _CameraDepthTexture;
}
sampler2D VRWorksGetDepthNormalsSampler()
{
  return _CameraDepthNormalsTexture;
}
float2 VRWorksRemapUV(float2 uv)
{
  return uv;
}
float2 VRWorksClampUV(float2 uv)
{
  return uv;
}
float2 VRWorksUVToLinear(float2 uv) { return uv; }
float2 VRWorksUVFromLinear(float2 uv) { return uv; }
#endif

#endif // _VRWORKS_CGINC
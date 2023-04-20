Shader "Custom/HightMapShader"
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        //3가지 텍스쳐
        _LowTex ("Low Texture", 2D) = "white" {}
        _MidTex ("Mid Texture", 2D) = "white" {}
        _HighTex ("High Texture", 2D) = "white" {}
        //높이 맵
        _HeightMap ("Height Map", 2D) = "white" {}
        //높이 제한 값
        _BlendLowMid ("Blend Low-Mid", Range(0, 1)) = 0.5
        _BlendMidHigh ("Blend Mid-High", Range(0, 1)) = 0.5
        //블렌드 값
        _BlendScale ("Blend Scale", Range(0, 1)) = 0.1
    }

    SubShader 
    {
        //터레인에서 사용할 것이기에 "TerrainCompatible"="true" 추가
        Tags { "RenderType"="Opaque" "TerrainCompatible"="true" }
        //Level of Detail, 가까울수록 자세하게 표현
        LOD 100
        
        //구조체, Material에 대한 쉐이더 정의
        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _LowTex;
        sampler2D _MidTex;
        sampler2D _HighTex;
        sampler2D _HeightMap;
        float _BlendLowMid;
        float _BlendMidHigh;
        float _BlendScale;

        struct Input 
        {
            float2 uv_MainTex;
            float2 uv_HeightMap;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float height = tex2D(_HeightMap, IN.uv_HeightMap).r;
            
             o.Albedo = tex2D(_HighTex, IN.uv_MainTex).rgb;
             if(height < _BlendMidHigh + _BlendScale)
                 o.Albedo = lerp(tex2D(_MidTex, IN.uv_MainTex).rgb, tex2D(_HighTex, IN.uv_MainTex).rgb,
                 (height - _BlendMidHigh + _BlendScale) / (_BlendScale*2));
             if(height < _BlendMidHigh - _BlendScale)
                 o.Albedo = tex2D(_MidTex, IN.uv_MainTex).rgb;
             if(height < _BlendLowMid + _BlendScale)
                 o.Albedo = lerp(tex2D(_LowTex, IN.uv_MainTex).rgb, tex2D(_MidTex, IN.uv_MainTex).rgb,
                 (height - _BlendLowMid + _BlendScale) / (_BlendScale*2));
             if(height < _BlendLowMid - _BlendScale)
                 o.Albedo = tex2D(_LowTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

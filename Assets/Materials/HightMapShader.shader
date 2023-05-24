Shader "Custom/HightMapShader"
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        //3가지 텍스쳐
        _LowTex ("Low Texture", 2D) = "white" {}
        _MidTex ("Mid Texture", 2D) = "white" {}
        _HighTex ("High Texture", 2D) = "white" {}
        _RiverTex ("River Texture", 2D) = "white" {}
        //높이 맵
        _HeightMap ("Height Map", 2D) = "white" {}
        //강 맵
        _RiverMap ("River Map", 2D) = "white" {}
        //높이 제한 값
        _BlendLowMid ("Blend Low-Mid", Range(0, 1)) = 0.5
        _BlendMidHigh ("Blend Mid-High", Range(0, 1)) = 0.5
        //블렌드 값
        _BlendScale ("Blend Scale", Range(0, 1)) = 0.1
        //강 높이 변경 값
        _RiverHeight ("River Height", Range(0, 1000)) = 0
    }

    SubShader 
    {
        //터레인에서 사용할 것이기에 "TerrainCompatible"="true" 추가
        Tags { "RenderType"="Opaque" "TerrainCompatible"="true" }
        //Level of Detail, 가까울수록 자세하게 표현
        LOD 100
        
        //구조체, Material에 대한 쉐이더 정의
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert target 3.0

        sampler2D _MainTex;
        sampler2D _LowTex;
        sampler2D _MidTex;
        sampler2D _HighTex;
        sampler2D _RiverTex;
        sampler2D _HeightMap;
        sampler2D _RiverMap;
        float _BlendLowMid;
        float _BlendMidHigh;
        float _BlendScale;
        float _RiverHeight;

        struct Input 
        {
            float2 uv_MainTex;
            float2 uv_HeightMap;
            float2 uv_RiverMap;
            float3 vertex;
        };

        void vert (inout appdata_full v)
        {
            float heightValue = 1 - tex2Dlod(_RiverMap, v.texcoord).r;
            v.vertex += float4(0, heightValue * _RiverHeight, 0, 0);
        }
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            float height = tex2D(_HeightMap, IN.uv_HeightMap).r;
            float river = tex2D(_RiverMap, IN.uv_RiverMap).r;

            //pixel not error
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

            if(river < 0.8f)
            {
                o.Albedo = tex2D(_RiverTex, IN.uv_MainTex).rgb;
            }
            if(river >= 0.8f && river < 1)
                o.Albedo = lerp(tex2D(_RiverTex, IN.uv_MainTex).rgb, o.Albedo,
                (river - 0.8f) / 0.2f);
        }
        ENDCG
    }
    FallBack "Diffuse"
}

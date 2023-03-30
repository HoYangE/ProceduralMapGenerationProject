Shader "Custom/HightMapShader"
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LowTex ("Low Texture", 2D) = "white" {}
        _MidTex ("Mid Texture", 2D) = "white" {}
        _HighTex ("High Texture", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "white" {}
        _BlendLowMid ("Blend Low-Mid", Range(0, 1)) = 0.5
        _BlendMidHigh ("Blend Mid-High", Range(0, 1)) = 0.5
    }

    SubShader 
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _LowTex;
        sampler2D _MidTex;
        sampler2D _HighTex;
        sampler2D _HeightMap;
        float _BlendLowMid;
        float _BlendMidHigh;

        struct Input 
        {
            float2 uv_MainTex;
            float2 uv_HeightMap;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float height = tex2D(_HeightMap, IN.uv_HeightMap).r;

            if (height < _BlendLowMid) 
                o.Albedo = tex2D(_LowTex, IN.uv_MainTex).rgb;
            else if (height < _BlendMidHigh) 
                o.Albedo = tex2D(_MidTex, IN.uv_MainTex).rgb;
            else 
                o.Albedo = tex2D(_HighTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

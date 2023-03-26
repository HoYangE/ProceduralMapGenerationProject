Shader "Custom/HightMapShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _LowTex("Low Texture", 2D) = "white" {}
        _MidTex("Mid Texture", 2D) = "white" {}
        _HighTex("High Texture", 2D) = "white" {}
        _HeightMap("Height Map", 2D) = "white" {}
        _LowThreshold("Low Threshold", Range(0, 1)) = 0.3
        _HighThreshold("High Threshold", Range(0, 1)) = 0.7
    }
    SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _LowTex;
        sampler2D _MidTex;
        sampler2D _HighTex;
        sampler2D _HeightMap;
        float _LowThreshold;
        float _HighThreshold;

        struct Input 
        {
            float2 uv_MainTex;
            float2 uv_HeightMap;
        };

        void surf(Input IN, inout SurfaceOutput o) 
        {
            float height = tex2D(_HeightMap, IN.uv_HeightMap).r;
            if (height < _LowThreshold) 
            {
                o.Albedo = tex2D(_LowTex, IN.uv_MainTex).rgb;
            }
            else if (height > _HighThreshold) 
            {
                o.Albedo = tex2D(_HighTex, IN.uv_MainTex).rgb;
            }
            else 
            {
                o.Albedo = tex2D(_MidTex, IN.uv_MainTex).rgb;
            }
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

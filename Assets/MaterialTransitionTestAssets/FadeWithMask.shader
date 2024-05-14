Shader "Custom/FadeWithMask"
{
    Properties
    {
        _Blend("Blend", Range(0, 1)) = 4.5
        _Color("Main Color", Color) = (1, 1, 1, 1)
        _MainTex("Main Texture", 2D) = "white" {}
        _TransitionTex("Transition Texture", 2D) = ""
        _MaskTex("Mask Texture", 2D) = ""
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            SetTexture[_MainTex]
            SetTexture[_TransitionTex]
            SetTexture[_MaskTex]

            {
                ConstantColor(0, 0, 0, [_Blend])
                Combine texture Lerp(constant) previous, texture * previous
            }
        }

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _TransitionTex;
        sampler2D _MaskTex;
        fixed4 _Color;
        float _Blend;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_TransitionTex;
            float2 uv_MaskTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 t1 = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 t2 = tex2D(_TransitionTex, IN.uv_TransitionTex) * _Color;
            float maskValue = tex2D(_MaskTex, IN.uv_MaskTex).r; // Assuming mask texture is grayscale
            float maskedBlend = _Blend * maskValue; // Applying mask to blend value
            o.Albedo = lerp(t1, t2, maskedBlend);
        }
        ENDCG
    }
    FallBack "Diffuse"
}

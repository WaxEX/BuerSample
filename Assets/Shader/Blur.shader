Shader "Unlit/Blur"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Dirction("_Dirction", Vector) = (1,0,0,0)
        _Ratio("_Ratio", Range(0,1)) = 0
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" }

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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _Dirction;
            float _Ratio;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                static const int samplingCount = 20;
                static const float blurOffset= 0.06 / samplingCount;

                float4 col = float4(0, 0, 0, 0);
                float weight_total = 0;

                [loop]
                for (float x = -samplingCount; x <= samplingCount; x += 1)
                {
                    float r = _Ratio * blurOffset * x;
                    float weight = exp(-0.5 * r * r * 5.0);

                    weight_total += weight;
                    col += tex2D(_MainTex, i.uv + _Dirction * r) * weight;
                }

                col /= weight_total;
                return col;
            }
            ENDCG
        }
    }
}
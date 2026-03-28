Shader "Unlit/ChromaKeyScope"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorThreshold ("Threshold", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
            float _ColorThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 체크무늬의 밝은 회색/하얀색을 타겟으로 함
                // 하얀색에 가까울수록 luminosity가 1에 가까움
                float luminosity = 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;

                // 임계값보다 밝으면 투명하게 만듦 (Alpha = 0)
                if (luminosity > 1.0 - _ColorThreshold)
                {
                    col.a = 0;
                }

                return col;
            }
            ENDCG
        }
    }
}

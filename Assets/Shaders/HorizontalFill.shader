Shader "StudioByStorm/Sprites/HorizontalFill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        [Enum(Left, 0, Right, 1)] _FillDirection ("Direction", Int) = 0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            fixed4 _Color;
            float _FillAmount;
            int _FillDirection;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 tintedCol = col * _Color;
                
                if(_FillDirection == 0)
                {
                    clip(i.uv.x - _FillAmount);
                }
                else
                {
                    clip(_FillAmount - i.uv.x); 
                }
                
                return fixed4(tintedCol.rgb, col.a);
            }

            ENDCG
        }
    }

}

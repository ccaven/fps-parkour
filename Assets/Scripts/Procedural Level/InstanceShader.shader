Shader "Custom/InstancedIndirectColor" {
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color: COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;

                LIGHTING_COORDS(1,2)

                fixed4 color : COLOR;
            }; 

            struct MeshProperties {
                // Transform properties
                float3 position;
                float4x4 rotation; // quaternion
                float3 scale;

                // 
                float4 color;
            };

            StructuredBuffer<MeshProperties> _Properties;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;

                o.uv = i.uv;

                float4 pos = mul(_Properties[instanceID].rotation, i.vertex * _Properties[instanceID].scale) + float4(_Properties[instanceID].position, 0.0);
                
                o.vertex = UnityObjectToClipPos(pos);

                o.color = _Properties[instanceID].color;

                TRANSFER_VERTEX_TO_FRAGMENT(o);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target {

                float atten = LIGHT_ATTENUATION(i);

                return i.color * atten;
            }

            ENDCG
        }
    }
}

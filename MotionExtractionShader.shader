Shader "Unlit/MotionExtractionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        //Declares buffer of 10 frames for use in storing
        [HideInInspector] Frame0 ("Frame 0", 2D) = "white" {}
        [HideInInspector] Frame1 ("Frame 1", 2D) = "white" {}
        [HideInInspector] Frame2 ("Frame 2", 2D) = "white" {}
        [HideInInspector] Frame3 ("Frame 3", 2D) = "white" {}
        [HideInInspector] Frame4 ("Frame 4", 2D) = "white" {}
        [HideInInspector] Frame5 ("Frame 5", 2D) = "white" {}
        [HideInInspector] Frame6 ("Frame 6", 2D) = "white" {}
        [HideInInspector] Frame7 ("Frame 7", 2D) = "white" {}
        [HideInInspector] Frame8 ("Frame 8", 2D) = "white" {}
        [HideInInspector] Frame9 ("Frame 9", 2D) = "white" {}

        [HideInInspector]_NumberOfDelayFrames ("Number Of Delay Frames", int) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            //Initializes frame buffer
            sampler2D _Frame0;            
            sampler2D _Frame1;            
            sampler2D _Frame2;            
            sampler2D _Frame3;            
            sampler2D _Frame4;            
            sampler2D _Frame5;            
            sampler2D _Frame6;            
            sampler2D _Frame7;            
            sampler2D _Frame8;            
            sampler2D _Frame9;

            int _NumberOfDelayFrames;

            fixed4 _MaxColor;
            fixed4 _Colors[10];
            float _Thresholds[10];
            int _NumberOfSections = 10;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainColor = tex2D(_MainTex, i.uv);

                fixed4 frame0 = tex2D(_Frame0, i.uv);
                fixed4 frame1 = tex2D(_Frame1, i.uv);
                fixed4 frame2 = tex2D(_Frame2, i.uv);
                fixed4 frame3 = tex2D(_Frame3, i.uv);
                fixed4 frame4 = tex2D(_Frame4, i.uv);
                fixed4 frame5 = tex2D(_Frame5, i.uv);
                fixed4 frame6 = tex2D(_Frame6, i.uv);
                fixed4 frame7 = tex2D(_Frame7, i.uv);
                fixed4 frame8 = tex2D(_Frame8, i.uv);
                fixed4 frame9 = tex2D(_Frame9, i.uv);

                fixed4 delayedColor;
                for(int frameIndex = 0; frameIndex < _NumberOfDelayFrames; frameIndex++){
                    if(frameIndex == 0) delayedColor += tex2D(_Frame0, i.uv);
                    else if(frameIndex == 1) delayedColor += tex2D(_Frame1, i.uv);
                    else if(frameIndex == 2) delayedColor += tex2D(_Frame2, i.uv);
                    else if(frameIndex == 3) delayedColor += tex2D(_Frame3, i.uv);
                    else if(frameIndex == 4) delayedColor += tex2D(_Frame4, i.uv);
                    else if(frameIndex == 5) delayedColor += tex2D(_Frame5, i.uv);
                    else if(frameIndex == 6) delayedColor += tex2D(_Frame6, i.uv);
                    else if(frameIndex == 7) delayedColor += tex2D(_Frame7, i.uv);
                    else if(frameIndex == 8) delayedColor += tex2D(_Frame8, i.uv);
                    else if(frameIndex == 9) delayedColor += tex2D(_Frame9, i.uv);
                }

                delayedColor = delayedColor / _NumberOfDelayFrames;

                float delta = distance(mainColor, delayedColor);

                if(_NumberOfDelayFrames > 0){
                    [unroll(10)]
                    for(int x = 0; x < 10; x++){
                        if(x < _NumberOfSections){
                            if(delta < _Thresholds[x]){
                                return _Colors[x];
                            }
                        }
                    }
                }
                else{
                    return mainColor;
                }
                return _MaxColor;
            }
            ENDCG
        }
    }
}

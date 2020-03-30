Shader "Billboard/BillboardParticl" 
    {
        Properties 
        {
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _Disappear( "Disappear", Range(0, 20) ) = 8
            _Life("Life", float) = 1.5
            _Speed("Speed", float) = 2
            _Acce("Acce", float) = -0.9
            _B("Scale time", float) = 0.125
            _C("Scale size", float) = 1
        }

        Subshader 
        {
            Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
			Fog { Mode Off }
			Cull Off
			Blend SrcAlpha  OneMinusSrcAlpha
			ZTest Always
			ZWrite Off
			Lighting Off
            Pass 
            {
				CGPROGRAM
				#pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma glsl_no_auto_normalization
                #include "UnityCG.cginc"                




                struct v2f 
                { 
                    fixed4   pos : SV_POSITION;
                    fixed2   uv : TEXCOORD0;
                    fixed4   clr : COLOR;

                };

                fixed4 _MainTex_ST;
                float _Disappear;
                
                float _Life;
                float _Speed;
                float _Acce;
                float _B;
                float _C;

				float _Delaytime;
				float _Scaletime1;
				float _Scaletime2;
				float _Maxsize;
				float _Endsize;

                v2f vert (appdata_full v)
                {
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					float XinitSpeed = v.texcoord1.x;
					float YinitSpeed = v.texcoord1.y;

					float normaltime = v.normal.x;
					float fadetime = v.normal.y;
					float acceleration = v.normal.z;

					//About scaling process
					float delaytime = _Delaytime;
					float scaletime1 = _Scaletime1;
					float scaletime2 = _Scaletime2;
					float maxsize = _Maxsize;
					float endsize = _Endsize;



					float time = _Time.y - v.tangent.z;
					float fLifeSpan = normaltime + fadetime;

					if( time < fLifeSpan )
					{		
						float scale;

						int phasetime1 = (int)(time > delaytime);
						phasetime1 *=  (int)(time <(delaytime + scaletime1));

						int phasetime2 = (int)(time > (delaytime+scaletime1));
						phasetime2 *=  (int)(time <normaltime);

						int phasetime3 = (int)(time > (delaytime+scaletime1 + scaletime2));
						phasetime3 *=  (int)(time <fLifeSpan);
							
						float scaleP1;
						{
							float b = min(1.0/scaletime1,1000);					
							scaleP1 = maxsize * b * (time - delaytime);
						}

						float scaleP2;
						{
							float k = min((endsize - maxsize)/scaletime2,1000);
							scaleP2 = k*(time-delaytime-scaletime1)+maxsize;
						}
							
						scale = scaleP1 * phasetime1 + scaleP2 * phasetime2 + endsize * phasetime3;
						v.tangent.xy += v.tangent.xy * scale;
						
                    	float4 camspacePos = mul (UNITY_MATRIX_V, v.vertex);
                    	camspacePos.xy += v.texcoord1.xy * time + time * time * acceleration;


                    	camspacePos = float4( v.tangent.xy + camspacePos.xy, camspacePos.z, 1);
                    	o.pos = mul (UNITY_MATRIX_P, camspacePos);
                    	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    	o.clr = v.color;
                    	//o.clr.a = 1.0 - pow( time / _Life, _Disappear );

						float alpha = 1.0 - pow( time / fLifeSpan, fadetime );
						
						int phasealpha = (int)(time>normaltime);
						o.clr.a = 1.0 * (1-phasealpha) + phasealpha*alpha ;
                    }
                    else
                    {
                    	o.pos = float4( 0,0,0,0 );
                    }
                    return o;
                }

                sampler2D _MainTex;
				
                fixed4 frag (v2f i) : COLOR
                {
                    fixed4 c = tex2D(_MainTex,i.uv);
                    c *= i.clr;
                    return c;
                  //return i.clr;
                }
                ENDCG
            }
        }
		fallback "Mobile/Unlit (Supports Lightmap)"
    }
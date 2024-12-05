Shader "Custom/GrassComputeSurface" 
{
	Properties 
	{
		[Toggle(BLEND)] _BlendFloor("Blend with floor", Float) = 0
		_Fade("Top Fade Offset", Range(-1,10)) = 1
		_Stretch("Top Fade Stretch", Range(-1,10)) = 1
		_AmbientAdjustmentColor("Ambient Adjustment Color", Color) = (0.5,0.5,0.5,1)
		_Metallic("Metallic", Range(0,1)) = 1
		_Glossiness("Specular", Range(0,1)) = 1
		_Edge("Edge", Range(0,1)) = 1
	}
	SubShader 
	{
		Tags { "Queue" = "Geometry" "IgnoreProjector" = "True"
			"RenderType" = "Opaque"
			
		}
		LOD 200	
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma surface surf Standard addshadow fullforwardshadows vertex:vert 
		#pragma target 4.0
		#pragma shader_feature BLEND

		struct DrawVertex
		{
			float3 positionWS; // The position in world space 
			float2 uv;
			
		};
		
		// A triangle on the generated mesh
		struct DrawTriangle
		{
			float3 normalOS;
			float3 diffuseColor;
			DrawVertex vertices[3]; // The three points on the triangle
		};
		
		#ifdef SHADER_API_D3D11	
			StructuredBuffer<DrawTriangle> _DrawTriangles;
		#endif

		// Properties
		float4 _TopTint;
		float4 _BottomTint;
		float _Fade, _Stretch;
		float _OrthographicCamSizeTerrain;
		float3 _OrthographicCamPosTerrain;
		uniform sampler2D _TerrainDiffuse;

		struct Input 
		{
			float2 uv_MainTex;
			float3 diffuseColor;			
			float3 worldPos;
			float2 texcoord;
			
		};
		struct appdata_id
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
			float3 color : COLOR;
			uint vertexID : SV_VertexID;
		};

		void vert(inout appdata_id v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);			
			#ifdef SHADER_API_D3D11			
				// Get the vertex from the buffer
				DrawTriangle tri = _DrawTriangles[v.vertexID / 3];
				DrawVertex input = tri.vertices[v.vertexID % 3];				
				v.vertex = mul(unity_WorldToObject,float4(input.positionWS,1));
				o.worldPos = input.positionWS;
				v.normal = tri.normalOS;			
				v.texcoord = input.uv;
				v.texcoord2 = input.positionWS;
				o.uv_MainTex = input.uv;
				o.texcoord = input.uv;
				
				o.diffuseColor = tri.diffuseColor;   
			#endif	
			
			v.vertex = v.vertex;
			
		}

		half _Glossiness;
		half _Metallic;
		half _Edge;
		fixed4 _Color;
		float4 _AmbientAdjustmentColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			
			// rendertexture UV for terrain blending
			float2 uv = IN.worldPos.xz - _OrthographicCamPosTerrain.xz;
			uv = uv / (_OrthographicCamSizeTerrain * 2);
			uv += 0.5;
			
			// fade over the length of the grass
			float verticalFade = saturate((IN.texcoord.y + _Fade) * _Stretch);
			// colors from the tool with tinting from the grass script
			float4 baseColor = lerp(_BottomTint , _TopTint * _AmbientAdjustmentColor,verticalFade) * float4(IN.diffuseColor, 1);
			// get the floor map
			float4 terrainForBlending = tex2D(_TerrainDiffuse, uv);
			
			float4 final = baseColor;
			#if BLEND             
				final = lerp(terrainForBlending,terrainForBlending+ ( _TopTint* float4(IN.diffuseColor, 1) * _AmbientAdjustmentColor) , verticalFade);
			#endif			
			float outside = saturate(abs(IN.texcoord.x - 0.5) + _Edge)* IN.texcoord.y;
			
			o.Albedo = final;
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic * outside;
			o.Smoothness = _Glossiness * outside;
			o.Alpha = 1;
		}	
		ENDCG		
	}	
	Fallback "VertexLit"
}
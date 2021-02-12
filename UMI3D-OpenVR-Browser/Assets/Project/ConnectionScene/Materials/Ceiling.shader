// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Ceiling"
{
	Properties
	{
		_Tiling("Tiling", Vector) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_AlbedoColor1("AlbedoColor1", Color) = (1,1,1,1)
		_AlbedoColor2("AlbedoColor2", Color) = (1,1,1,1)
		_Speed("Speed", Float) = 0
		_Threeshold("Threeshold", Range( 0 , 1)) = 0.91
		_Scale("Scale", Float) = 6.76
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _AlbedoColor1;
		uniform float4 _AlbedoColor2;
		uniform float _Scale;
		uniform float _Speed;
		uniform float _Threeshold;
		uniform sampler2D _Albedo;
		uniform float2 _Tiling;


		float2 voronoihash8( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi8( float2 v, float time, inout float2 id, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mr = 0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash8( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = g - f + o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			
F1 = 8.0;
for ( int j = -2; j <= 2; j++ )
{
for ( int i = -2; i <= 2; i++ )
{
float2 g = mg + float2( i, j );
float2 o = voronoihash8( n + g );
		o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = g - f + o;
float d = dot( 0.5 * ( mr + r ), normalize( r - mr ) );
F1 = min( F1, d );
}
}
return F1;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float mulTime4 = _Time.y * _Speed;
			float time8 = mulTime4;
			float2 coords8 = i.uv_texcoord * _Scale;
			float2 id8 = 0;
			float voroi8 = voronoi8( coords8, time8,id8, 0 );
			float4 lerpResult9 = lerp( _AlbedoColor1 , _AlbedoColor2 , (( voroi8 < _Threeshold ) ? 0.0 :  1.0 ));
			float2 temp_cast_0 = (( mulTime4 * 0.01 )).xx;
			float2 uv_TexCoord3 = i.uv_texcoord * _Tiling + temp_cast_0;
			o.Emission = ( lerpResult9 * tex2D( _Albedo, uv_TexCoord3 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
239;73;1298;652;650.8033;474.1828;1.3;True;False
Node;AmplifyShaderEditor.RangedFloatNode;5;-1572.906,67.99057;Inherit;False;Property;_Speed;Speed;4;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;14;-1421.155,-607.2505;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-1355.574,-389.3127;Inherit;False;Property;_Scale;Scale;6;0;Create;True;0;0;False;0;6.76;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;4;-1383.095,65.32279;Inherit;False;1;0;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;8;-1149.727,-497.5882;Inherit;True;0;0;1;4;1;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;2;FLOAT;0;FLOAT;1
Node;AmplifyShaderEditor.RangedFloatNode;21;-1168.99,-225.2229;Inherit;False;Property;_Threeshold;Threeshold;5;0;Create;True;0;0;False;0;0.91;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1000.763,17.37813;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;23;-804.2025,-45.18266;Inherit;False;Property;_Tiling;Tiling;0;0;Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TFHCCompareLower;20;-788.4183,-308.7338;Inherit;True;4;0;FLOAT;0;False;1;FLOAT;0.05;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-589.5632,15.41724;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;10;-349.141,-409.7509;Inherit;False;Property;_AlbedoColor2;AlbedoColor2;3;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-372.3365,-597.251;Inherit;False;Property;_AlbedoColor1;AlbedoColor1;2;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-337.1206,-26.71387;Inherit;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;9;-62.09369,-327.3159;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;177.7455,-29.09705;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;454.0775,-2.054649;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Ceiling;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;5;0
WireConnection;8;0;14;0
WireConnection;8;1;4;0
WireConnection;8;2;12;0
WireConnection;22;0;4;0
WireConnection;20;0;8;0
WireConnection;20;1;21;0
WireConnection;3;0;23;0
WireConnection;3;1;22;0
WireConnection;2;1;3;0
WireConnection;9;0;7;0
WireConnection;9;1;10;0
WireConnection;9;2;20;0
WireConnection;6;0;9;0
WireConnection;6;1;2;0
WireConnection;0;2;6;0
ASEEND*/
//CHKSM=4D6DC74C709DB905A33B437F533FEB68BC808114
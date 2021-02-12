// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "QuestFloor"
{
	Properties
	{
		_Tiling("Tiling", Float) = 1
		_TileTexture("Tile Texture", 2D) = "white" {}
		_TileAlbedo("TileAlbedo", Color) = (1,1,1,1)
		_TileEmissionColor("Tile Emission Color", Color) = (1,1,1,1)
		_TileMinDistance("TileMinDistance", Float) = 0
		_TileMaxDistance("TileMaxDistance", Float) = 1
		_Center("Center", Vector) = (0,0,0,0)
		_HaloCenterColor("Halo Center Color", Color) = (1,1,1,1)
		_HaloOutsideColor("Halo Outside Color", Color) = (1,1,1,1)
		_HaloMinRadius("HaloMinRadius", Float) = 0
		_HaloMaxRadius("HaloMaxRadius", Float) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float3 _Center;
		uniform float _TileMinDistance;
		uniform float _TileMaxDistance;
		uniform float4 _TileAlbedo;
		uniform sampler2D _TileTexture;
		uniform float _Tiling;
		uniform float4 _HaloOutsideColor;
		uniform float4 _HaloCenterColor;
		uniform float _HaloMinRadius;
		uniform float _HaloMaxRadius;
		uniform float4 _TileEmissionColor;
		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float Distance26 = distance( _Center , mul( unity_ObjectToWorld, float4( ase_vertex3Pos , 0.0 ) ).xyz );
			float clampResult30 = clamp( (1.0 + (Distance26 - _TileMinDistance) * (0.0 - 1.0) / (_TileMaxDistance - _TileMinDistance)) , 0.0 , 1.0 );
			float4 appendResult35 = (float4(ase_vertex3Pos.x , ase_vertex3Pos.z , 0.0 , 0.0));
			float4 tex2DNode1 = tex2D( _TileTexture, ( _Tiling * appendResult35 ).xy );
			float clampResult18 = clamp( (1.0 + (Distance26 - _HaloMinRadius) * (0.0 - 1.0) / (_HaloMaxRadius - _HaloMinRadius)) , 0.0 , 1.0 );
			float4 lerpResult20 = lerp( _HaloOutsideColor , _HaloCenterColor , clampResult18);
			o.Albedo = ( ( clampResult30 * ( _TileAlbedo * ( tex2DNode1 * tex2DNode1.a ) ) ) + lerpResult20 ).rgb;
			o.Emission = ( clampResult30 * ( tex2DNode1.a * _TileEmissionColor ) ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
1157;81;483;645;1810.631;473.7507;1;False;False
Node;AmplifyShaderEditor.CommentaryNode;27;-2398.296,398.6474;Inherit;False;913.8701;520.39;Distance from center;6;9;8;10;11;7;26;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;9;-2346.797,619.0435;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.PosVertexDataNode;8;-2348.296,694.0377;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;7;-2133.892,448.6474;Inherit;False;Property;_Center;Center;6;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-2114.315,625.0433;Inherit;True;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;34;-1752.951,-218.5564;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;35;-1546.951,-213.5564;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-1527.951,-307.5564;Inherit;False;Property;_Tiling;Tiling;0;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;11;-1890.832,506.5522;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1727.427,503.9433;Inherit;False;Distance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1301.951,-217.5564;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-727.9318,1032.533;Inherit;False;Property;_HaloMinRadius;HaloMinRadius;9;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1114.333,-195.0836;Inherit;True;Property;_TileTexture;Tile Texture;1;0;Create;True;0;0;False;0;-1;1b817d4ab75df7945b751d7a1af2f780;1b817d4ab75df7945b751d7a1af2f780;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-1271.324,-588.5478;Inherit;False;Property;_TileMinDistance;TileMinDistance;4;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-721.9326,1124.026;Inherit;False;Property;_HaloMaxRadius;HaloMaxRadius;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-1260.516,-667.5139;Inherit;False;26;Distance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1271.607,-513.5969;Inherit;False;Property;_TileMaxDistance;TileMaxDistance;5;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;28;-635.3146,620.8442;Inherit;False;26;Distance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-799.9181,-192.1469;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;31;-1004.914,-542.9607;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;12;-381.4587,636.564;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-804.303,-370.6536;Inherit;False;Property;_TileAlbedo;TileAlbedo;2;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;18;-168.2643,631.6488;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;21;-346.1101,229.4174;Inherit;False;Property;_HaloOutsideColor;Halo Outside Color;8;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;30;-797.1578,-542.8758;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-1067.661,80.92014;Inherit;False;Property;_TileEmissionColor;Tile Emission Color;3;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-334.1232,406.9891;Inherit;False;Property;_HaloCenterColor;Halo Center Color;7;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-524.6454,-275.8914;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;20;8.559891,397.1376;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-279.294,-302.7834;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-794.0798,23.22981;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;23;413.3958,188.3268;Inherit;False;Property;_Smoothness;Smoothness;12;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;399.3958,121.3268;Inherit;False;Property;_Metallic;Metallic;11;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-339.4608,12.62292;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;258.7588,-202.6503;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;697.6267,-15.42061;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;QuestFloor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;9;0
WireConnection;10;1;8;0
WireConnection;35;0;34;1
WireConnection;35;1;34;3
WireConnection;11;0;7;0
WireConnection;11;1;10;0
WireConnection;26;0;11;0
WireConnection;36;0;37;0
WireConnection;36;1;35;0
WireConnection;1;1;36;0
WireConnection;19;0;1;0
WireConnection;19;1;1;4
WireConnection;31;0;29;0
WireConnection;31;1;25;0
WireConnection;31;2;24;0
WireConnection;12;0;28;0
WireConnection;12;1;13;0
WireConnection;12;2;14;0
WireConnection;18;0;12;0
WireConnection;30;0;31;0
WireConnection;4;0;2;0
WireConnection;4;1;19;0
WireConnection;20;0;21;0
WireConnection;20;1;15;0
WireConnection;20;2;18;0
WireConnection;32;0;30;0
WireConnection;32;1;4;0
WireConnection;5;0;1;4
WireConnection;5;1;3;0
WireConnection;33;0;30;0
WireConnection;33;1;5;0
WireConnection;17;0;32;0
WireConnection;17;1;20;0
WireConnection;0;0;17;0
WireConnection;0;2;33;0
WireConnection;0;3;22;0
WireConnection;0;4;23;0
ASEEND*/
//CHKSM=914970E6C3676CF0823C1C466F95C203142292E0
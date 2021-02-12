// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MenuItem"
{
	Properties
	{
		_LineWidth("LineWidth", Float) = 0
		_TimeScale("TimeScale", Float) = 1
		_ColorA("Color A", Color) = (0.227451,0.9294118,0.9450981,0.09019608)
		_ColorB("Color B", Color) = (0.2269491,0.9281093,0.9433962,0.5647059)
		_ColorC("Color C", Color) = (0.227451,0.9294118,0.9450981,0.09019608)
		[Toggle]_Animate("Animate", Float) = 0
		_CompletionMin("CompletionMin", Float) = 0
		_CompletionMax("CompletionMax", Float) = 0
		_CompletionValue("CompletionValue", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
		};

		uniform float _CompletionMin;
		uniform float _CompletionMax;
		uniform float _CompletionValue;
		uniform float4 _ColorA;
		uniform float4 _ColorB;
		uniform float _LineWidth;
		uniform float _Animate;
		uniform float _TimeScale;
		uniform float4 _ColorC;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float lerpResult29 = lerp( _CompletionMin , _CompletionMax , _CompletionValue);
			float mulTime17 = _Time.y * _TimeScale;
			float clampResult14 = clamp( ( ( sin( ( _LineWidth * ( distance( ase_vertex3Pos , float3( 0,0,0 ) ) + (( _Animate )?( mulTime17 ):( 0.0 )) ) ) ) * 2.0 ) + 1.9 ) , 0.0 , 1.0 );
			float4 lerpResult9 = lerp( _ColorA , _ColorB , clampResult14);
			float4 ifLocalVar23 = 0;
			if( distance( ase_vertex3Pos , float3( 0,0,0 ) ) >= lerpResult29 )
				ifLocalVar23 = lerpResult9;
			else
				ifLocalVar23 = _ColorC;
			float4 FinalColor21 = ifLocalVar23;
			o.Emission = FinalColor21.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
2187;73;1099;573;3442.089;1064.384;4.400386;True;False
Node;AmplifyShaderEditor.CommentaryNode;22;-2489.932,-373.7344;Inherit;False;1915.163;979.6752;Comment;15;18;17;1;20;2;16;5;4;3;13;15;14;11;10;9;AnimatedBase;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2439.932,490.9408;Inherit;False;Property;_TimeScale;TimeScale;1;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;1;-2351.146,130.3588;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;17;-2288.505,486.2557;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;2;-2117.772,128.5011;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;20;-2111.773,462.5197;Inherit;False;Property;_Animate;Animate;5;0;Create;True;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-1832.33,178.0575;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1847.374,12.83331;Inherit;False;Property;_LineWidth;LineWidth;0;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-1652.195,98.56322;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;3;-1496.676,101.2988;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1288.388,185.4663;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-1136.448,184.3036;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1007.047,-635.8151;Inherit;False;Property;_CompletionMin;CompletionMin;6;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;24;-997.1815,-884.3072;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-1376.96,-136.301;Inherit;False;Property;_ColorB;Color B;3;0;Create;True;0;0;False;0;0.2269491,0.9281093,0.9433962,0.5647059;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;14;-944.8989,176.3381;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;10;-1386.871,-323.7344;Inherit;False;Property;_ColorA;Color A;2;0;Create;True;0;0;False;0;0.227451,0.9294118,0.9450981,0.09019608;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;28;-1084.218,-471.8262;Inherit;False;Property;_CompletionValue;CompletionValue;8;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-1011.181,-554.5096;Inherit;False;Property;_CompletionMax;CompletionMax;7;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-758.771,82.64065;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceOpNode;25;-756.9167,-891.6773;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;29;-742.4601,-584.8269;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;-971.3553,896.8886;Inherit;False;Property;_ColorC;Color C;4;0;Create;True;0;0;False;0;0.227451,0.9294118,0.9450981,0.09019608;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;23;-402.3595,19.94743;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;12;91.82869,-4.404609;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;-183.8548,-124.3293;Inherit;False;FinalColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;380.7732,-143.3588;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;MenuItem;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;18;0
WireConnection;2;0;1;0
WireConnection;20;1;17;0
WireConnection;16;0;2;0
WireConnection;16;1;20;0
WireConnection;4;0;5;0
WireConnection;4;1;16;0
WireConnection;3;0;4;0
WireConnection;13;0;3;0
WireConnection;15;0;13;0
WireConnection;14;0;15;0
WireConnection;9;0;10;0
WireConnection;9;1;11;0
WireConnection;9;2;14;0
WireConnection;25;0;24;0
WireConnection;29;0;26;0
WireConnection;29;1;27;0
WireConnection;29;2;28;0
WireConnection;23;0;25;0
WireConnection;23;1;29;0
WireConnection;23;2;9;0
WireConnection;23;3;9;0
WireConnection;23;4;30;0
WireConnection;12;0;21;0
WireConnection;21;0;23;0
WireConnection;0;2;21;0
ASEEND*/
//CHKSM=2A188C3A77795347FAF601FA23D4DBA7441C8CD6
// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "browserQuest_icon"
{
	Properties
	{
		[HDR][Gamma]_ButtonColor("Button Color", Color) = (0,0,0,0)
		_Iconmap("Icon map", 2D) = "white" {}
		_Emisiveintensity("Emisive intensity", Range( 0 , 1)) = 0
		[HDR]_IconColor("Icon Color", Color) = (0,0,0,0)
		_Alpha_map("Alpha_map", 2D) = "white" {}
		_Opacity("Opacity", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _ButtonColor;
		uniform sampler2D _Iconmap;
		uniform float4 _Iconmap_ST;
		uniform float4 _IconColor;
		SamplerState sampler_Iconmap;
		uniform float _Emisiveintensity;
		uniform float _Opacity;
		uniform sampler2D _Alpha_map;
		SamplerState sampler_Alpha_map;
		uniform float4 _Alpha_map_ST;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Iconmap = i.uv_texcoord * _Iconmap_ST.xy + _Iconmap_ST.zw;
			float4 tex2DNode12 = tex2D( _Iconmap, uv_Iconmap );
			float4 lerpResult25 = lerp( _ButtonColor , ( tex2DNode12 * _IconColor ) , tex2DNode12.a);
			o.Emission = ( lerpResult25 * _Emisiveintensity ).rgb;
			float2 uv_Alpha_map = i.uv_texcoord * _Alpha_map_ST.xy + _Alpha_map_ST.zw;
			float lerpResult6 = lerp( 0.0 , _Opacity , tex2D( _Alpha_map, uv_Alpha_map ).a);
			o.Alpha = lerpResult6;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18600
0;0;1536;803;1210.96;729.3681;1.52635;True;False
Node;AmplifyShaderEditor.SamplerNode;12;-585.9665,-415.1974;Inherit;True;Property;_Iconmap;Icon map;2;0;Create;True;0;0;False;0;False;-1;None;ab1a12963d5d82846b3a27aab753bc83;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;26;-559.9615,-217.1023;Inherit;False;Property;_IconColor;Icon Color;4;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;0.3764706,0.3764706,0.3764706,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-178.5324,-257.0957;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;5;-514.917,-590.6414;Inherit;False;Property;_ButtonColor;Button Color;1;2;[HDR];[Gamma];Create;True;0;0;False;0;False;0,0,0,0;0.3773585,0.3773585,0.3773585,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;25;22.73281,-384.524;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-88.02811,-3.175945;Inherit;False;Property;_Opacity;Opacity;6;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;12.072,-254.1818;Float;False;Property;_Emisiveintensity;Emisive intensity;3;0;Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;11;-104.3523,90.6695;Inherit;True;Property;_Alpha_map;Alpha_map;5;0;Create;True;0;0;False;0;False;-1;None;56f6307cf3f25fd439a330ad118dbe5b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;238.692,-24.47672;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;336.6346,-273.6501;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;669.4382,-320.3453;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;browserQuest_icon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;AlphaTest;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;0;12;0
WireConnection;27;1;26;0
WireConnection;25;0;5;0
WireConnection;25;1;27;0
WireConnection;25;2;12;4
WireConnection;6;1;3;0
WireConnection;6;2;11;4
WireConnection;7;0;25;0
WireConnection;7;1;4;0
WireConnection;0;2;7;0
WireConnection;0;9;6;0
ASEEND*/
//CHKSM=F379491191E54F192035C38396991FFBEE9EB38A
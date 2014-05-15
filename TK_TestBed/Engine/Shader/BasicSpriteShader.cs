using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK_TestBed.Engine.Utility;

namespace TK_TestBed.Engine.Shader
{
    class BasicSpriteShader : Shader
    {
        public BasicSpriteShader() : base()
        {
            VertexShader = Utils.J(
                ShaderChunk.version_header,
                "in vec4 position;",
                "uniform float voxelSize;",
                "uniform mat4 MV;",
                "uniform mat4 P; //rojection",
                "uniform vec2 screenSize;",
                "uniform float maxDistance;",
                "uniform float pctAlpha;",
                "out float depthAlpha;",
                "void main() {",
                    "vec4 eyePos = MV * position;",
                    "vec4 projVoxel = P * vec4(voxelSize, voxelSize, eyePos.z, eyePos.w);",
                    "vec2 projSize = screenSize * projVoxel.xy / projVoxel.w;",
                    "float posSize = projSize.x+projSize.y;",
                    "depthAlpha = clamp(1.0f - distance(eyePos,projVoxel)/maxDistance,0.1f,1.0f) * pctAlpha;",
                    "gl_PointSize = max(posSize, 2.0f);",
                    "gl_Position = P * eyePos;",
                "}");

            FragmentShader = Utils.J(
                ShaderChunk.version_header,
                "uniform sampler2D tex;",
                "out vec4 color;",
                "in float depthAlpha;",
                "void main() {",
                    "color = texture(tex, gl_PointCoord);",
                    "if (color.a<0.01f)",
	                    "discard;",
                    "color.a = depthAlpha;",
                    "color.r *= clamp(depthAlpha,0.43f,1.0f);",
                    "color.g *= clamp(depthAlpha,0.30f,1.0f);",
                    "color.b *= clamp(depthAlpha,0.63f,1.0f);",
                "}");

            Build(this);
        }
        // Must match variables in shader code in order to be properly bound via reflection in the shader builder
        //
#pragma warning disable 0649
        public int MV, P, screenSize, voxelSize, position, tex, maxDistance, pctAlpha;//, alpha;
#pragma warning restore 0649
    }
}

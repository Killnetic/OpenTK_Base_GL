/* Thanks three.js for this pattern!
 * The MIT License

Copyright &copy; 2010-2014 three.js authors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 * */
using TK_TestBed.Engine.Utility;

namespace TK_TestBed.Engine.Shader
{
    internal class ShaderChunk
    {
        public static readonly string version_header = "#version 330\n";

        public static readonly string alphatest_fragment = Utils.J(
            "#ifdef ALPHATEST",

            "if ( gl_FragColor.a < ALPHATEST ) discard;",

            "#endif");

        public static readonly string color_fragment = Utils.J(
            "#ifdef USE_COLOR",

            "gl_FragColor = gl_FragColor * vec4( vColor, 1.0 );",

            "#endif");

        public static readonly string color_pars_fragment = Utils.J(
            "#ifdef USE_COLOR",
            "varying vec3 vColor;",
            "#endif");

        public static readonly string color_pars_vertex = Utils.J(
            "#ifdef USE_COLOR",
            "varying vec3 vColor;",
            "#endif");

        public static readonly string color_vertex = Utils.J(
            "#ifdef USE_COLOR",
            "#ifdef GAMMA_INPUT",
            "vColor = color * color;",
            "#else",
            "vColor = color;",
            "#endif",
            "#endif");

        public static readonly string fog_fragment = Utils.J(
            "#ifdef USE_FOG",

            "float depth = gl_FragCoord.z / gl_FragCoord.w;",

            "#ifdef FOG_EXP2",

            "const float LOG2 = 1.442695;",
            "float fogFactor = exp2( - fogDensity * fogDensity * depth * depth * LOG2 );",
            "fogFactor = 1.0 - clamp( fogFactor, 0.0, 1.0 );",

            "#else",

            "float fogFactor = smoothstep( fogNear, fogFar, depth );",

            "#endif",

            "gl_FragColor = mix( gl_FragColor, vec4( fogColor, gl_FragColor.w ), fogFactor );",

            "#endif");

        public static readonly string fog_pars_fragment = Utils.J(
            "#ifdef USE_FOG",
            "uniform vec3 fogColor;",
            "#ifdef FOG_EXP2",
            "uniform float fogDensity;",
            "#else",
            "uniform float fogNear;",
            "uniform float fogFar;",
            "#endif",
            "#endif");

        public static readonly string map_particle_fragment = Utils.J(
            "#ifdef USE_MAP",
            "gl_FragColor = gl_FragColor * texture2D( map, vec2( gl_PointCoord.x, 1.0 - gl_PointCoord.y ) );",
            "#endif");

        public static readonly string map_particle_pars_fragment = Utils.J(
            "#ifdef USE_MAP",
            "uniform sampler2D map;",
            "#endif");

        public static readonly string shadowmap_fragment = Utils.J(
            "#ifdef USE_SHADOWMAP",

            "#ifdef SHADOWMAP_DEBUG",

            "vec3 frustumColors[3];",
            "frustumColors[0] = vec3( 1.0, 0.5, 0.0 );",
            "frustumColors[1] = vec3( 0.0, 1.0, 0.8 );",
            "frustumColors[2] = vec3( 0.0, 0.5, 1.0 );",

            "#endif",

            "#ifdef SHADOWMAP_CASCADE",

            "int inFrustumCount = 0;",

            "#endif",

            "float fDepth;",
            "vec3 shadowColor = vec3( 1.0 );",

            "for( int i = 0; i < MAX_SHADOWS; i ++ ) {",

            "vec3 shadowCoord = vShadowCoord[ i ].xyz / vShadowCoord[ i ].w;",

            // "if ( something && something )" breaks ATI OpenGL shader compiler
            // "if ( all( something, something ) )" using this instead

            "bvec4 inFrustumVec = bvec4 ( shadowCoord.x >= 0.0, shadowCoord.x <= 1.0, shadowCoord.y >= 0.0, shadowCoord.y <= 1.0 );",
            "bool inFrustum = all( inFrustumVec );",

            // don't shadow pixels outside of light frustum
            // use just first frustum (for cascades)
            // don't shadow pixels behind far plane of light frustum

            "#ifdef SHADOWMAP_CASCADE",

            "inFrustumCount += int( inFrustum );",
            "bvec3 frustumTestVec = bvec3( inFrustum, inFrustumCount == 1, shadowCoord.z <= 1.0 );",

            "#else",

            "bvec2 frustumTestVec = bvec2( inFrustum, shadowCoord.z <= 1.0 );",

            "#endif",

            "bool frustumTest = all( frustumTestVec );",

            "if ( frustumTest ) {",

            "shadowCoord.z += shadowBias[ i ];",

            "#if defined( SHADOWMAP_TYPE_PCF )",

            // Percentage-close filtering
            // (9 pixel kernel)
            // http://fabiensanglard.net/shadowmappingPCF/

            "float shadow = 0.0;",

            /*
                            // nested loops breaks shader compiler / validator on some ATI cards when using OpenGL
                            // must enroll loop manually

                            "for ( float y = -1.25; y <= 1.25; y += 1.25 )",
                            "for ( float x = -1.25; x <= 1.25; x += 1.25 ) {",

                            "vec4 rgbaDepth = texture2D( shadowMap[ i ], vec2( x * xPixelOffset, y * yPixelOffset ) + shadowCoord.xy );",

                            // doesn't seem to produce any noticeable visual difference compared to simple "texture2D" lookup
                            //"vec4 rgbaDepth = texture2DProj( shadowMap[ i ], vec4( vShadowCoord[ i ].w * ( vec2( x * xPixelOffset, y * yPixelOffset ) + shadowCoord.xy ), 0.05, vShadowCoord[ i ].w ) );",

                            "float fDepth = unpackDepth( rgbaDepth );",

                            "if ( fDepth < shadowCoord.z )",
                            "shadow += 1.0;",

                            "}",

                            "shadow /= 9.0;",

                            */

            "const float shadowDelta = 1.0 / 9.0;",

            "float xPixelOffset = 1.0 / shadowMapSize[ i ].x;",
            "float yPixelOffset = 1.0 / shadowMapSize[ i ].y;",

            "float dx0 = -1.25 * xPixelOffset;",
            "float dy0 = -1.25 * yPixelOffset;",
            "float dx1 = 1.25 * xPixelOffset;",
            "float dy1 = 1.25 * yPixelOffset;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx0, dy0 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( 0.0, dy0 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx1, dy0 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx0, 0.0 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx1, 0.0 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx0, dy1 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( 0.0, dy1 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "fDepth = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx1, dy1 ) ) );",
            "if ( fDepth < shadowCoord.z ) shadow += shadowDelta;",

            "shadowColor = shadowColor * vec3( ( 1.0 - shadowDarkness[ i ] * shadow ) );",

            "#elif defined( SHADOWMAP_TYPE_PCF_SOFT )",

            // Percentage-close filtering
            // (9 pixel kernel)
            // http://fabiensanglard.net/shadowmappingPCF/

            "float shadow = 0.0;",

            "float xPixelOffset = 1.0 / shadowMapSize[ i ].x;",
            "float yPixelOffset = 1.0 / shadowMapSize[ i ].y;",

            "float dx0 = -1.0 * xPixelOffset;",
            "float dy0 = -1.0 * yPixelOffset;",
            "float dx1 = 1.0 * xPixelOffset;",
            "float dy1 = 1.0 * yPixelOffset;",

            "mat3 shadowKernel;",
            "mat3 depthKernel;",

            "depthKernel[0][0] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx0, dy0 ) ) );",
            "depthKernel[0][1] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx0, 0.0 ) ) );",
            "depthKernel[0][2] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx0, dy1 ) ) );",
            "depthKernel[1][0] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( 0.0, dy0 ) ) );",
            "depthKernel[1][1] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy ) );",
            "depthKernel[1][2] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( 0.0, dy1 ) ) );",
            "depthKernel[2][0] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx1, dy0 ) ) );",
            "depthKernel[2][1] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx1, 0.0 ) ) );",
            "depthKernel[2][2] = unpackDepth( texture2D( shadowMap[ i ], shadowCoord.xy + vec2( dx1, dy1 ) ) );",

            "vec3 shadowZ = vec3( shadowCoord.z );",
            "shadowKernel[0] = vec3(lessThan(depthKernel[0], shadowZ ));",
            "shadowKernel[0] *= vec3(0.25);",

            "shadowKernel[1] = vec3(lessThan(depthKernel[1], shadowZ ));",
            "shadowKernel[1] *= vec3(0.25);",

            "shadowKernel[2] = vec3(lessThan(depthKernel[2], shadowZ ));",
            "shadowKernel[2] *= vec3(0.25);",

            "vec2 fractionalCoord = 1.0 - fract( shadowCoord.xy * shadowMapSize[i].xy );",

            "shadowKernel[0] = mix( shadowKernel[1], shadowKernel[0], fractionalCoord.x );",
            "shadowKernel[1] = mix( shadowKernel[2], shadowKernel[1], fractionalCoord.x );",

            "vec4 shadowValues;",
            "shadowValues.x = mix( shadowKernel[0][1], shadowKernel[0][0], fractionalCoord.y );",
            "shadowValues.y = mix( shadowKernel[0][2], shadowKernel[0][1], fractionalCoord.y );",
            "shadowValues.z = mix( shadowKernel[1][1], shadowKernel[1][0], fractionalCoord.y );",
            "shadowValues.w = mix( shadowKernel[1][2], shadowKernel[1][1], fractionalCoord.y );",

            "shadow = dot( shadowValues, vec4( 1.0 ) );",

            "shadowColor = shadowColor * vec3( ( 1.0 - shadowDarkness[ i ] * shadow ) );",

            "#else",

            "vec4 rgbaDepth = texture2D( shadowMap[ i ], shadowCoord.xy );",
            "float fDepth = unpackDepth( rgbaDepth );",

            "if ( fDepth < shadowCoord.z )",

            // spot with multiple shadows is darker

            "shadowColor = shadowColor * vec3( 1.0 - shadowDarkness[ i ] );",

            // spot with multiple shadows has the same color as single shadow spot

            //"shadowColor = min( shadowColor, vec3( shadowDarkness[ i ] ) );",

            "#endif",

            "}",


            "#ifdef SHADOWMAP_DEBUG",

            "#ifdef SHADOWMAP_CASCADE",

            "if ( inFrustum && inFrustumCount == 1 ) gl_FragColor.xyz *= frustumColors[ i ];",

            "#else",

            "if ( inFrustum ) gl_FragColor.xyz *= frustumColors[ i ];",

            "#endif",

            "#endif",

            "}",

            "#ifdef GAMMA_OUTPUT",

            "shadowColor *= shadowColor;",

            "#endif",

            "gl_FragColor.xyz = gl_FragColor.xyz * shadowColor;",

            "#endif");

        public static readonly string shadowmap_pars_fragment = Utils.J(
            "#ifdef USE_SHADOWMAP",

            "uniform sampler2D shadowMap[ MAX_SHADOWS ];",
            "uniform vec2 shadowMapSize[ MAX_SHADOWS ];",

            "uniform float shadowDarkness[ MAX_SHADOWS ];",
            "uniform float shadowBias[ MAX_SHADOWS ];",

            "varying vec4 vShadowCoord[ MAX_SHADOWS ];",

            "float unpackDepth( const in vec4 rgba_depth ) {",

            "const vec4 bit_shift = vec4( 1.0 / ( 256.0 * 256.0 * 256.0 ), 1.0 / ( 256.0 * 256.0 ), 1.0 / 256.0, 1.0 );",
            "float depth = dot( rgba_depth, bit_shift );",
            "return depth;",

            "}",

            "#endif");

        public static readonly string shadowmap_pars_vertex = Utils.J(
            "#ifdef USE_SHADOWMAP",
            "varying vec4 vShadowCoord[ MAX_SHADOWS ];",
            "uniform mat4 shadowMatrix[ MAX_SHADOWS ];",
            "#endif");

        public static readonly string shadowmap_vertex = Utils.J(
            "#ifdef USE_SHADOWMAP",
            "for( int i = 0; i < MAX_SHADOWS; i ++ ) {",
            "vShadowCoord[ i ] = shadowMatrix[ i ] * worldPosition;",
            "}",
            "#endif");

        public static readonly string worldpos_vertex = Utils.J(
            "#if defined( USE_ENVMAP ) || defined( PHONG ) || defined( LAMBERT ) || defined ( USE_SHADOWMAP )",
            "#ifdef USE_SKINNING",
            "vec4 worldPosition = modelMatrix * skinned;",
            "#endif",
            "#if defined( USE_MORPHTARGETS ) && ! defined( USE_SKINNING )",
            "vec4 worldPosition = modelMatrix * vec4( morphed, 1.0 );",
            "#endif",
            "#if ! defined( USE_MORPHTARGETS ) && ! defined( USE_SKINNING )",
            "vec4 worldPosition = modelMatrix * vec4( position, 1.0 );",
            "#endif",
            "#endif");

        
    }
}

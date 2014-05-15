using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TK_TestBed.Engine.GPU;

namespace TK_TestBed.Engine._3DObjects
{
    class ParticleCloud : BaseObject3D
    {
        // cloud fields
        //
        public StaticVBO staticVbo;

        // sprite fields
        //
        private PointSpriteImage pointSpriteImage;

        private float pointMaxSize;
        private int pointCount;

        public int Diameter { get; private set; }
        


        public ParticleCloud(int pointCount, int pointSize)
        {

            this.pointCount = pointCount;

            // generate points
            //
            Vector3[] generatedPoints = new Vector3[pointCount];

            Random random = new Random();
            double radius, latitude, longitude;
            float x, y, z;
            Diameter = pointCount;
            for (int i = 0; i < pointCount; i++)
            {
                radius = 1 + (pointCount - (random.NextDouble() * pointCount));
                longitude = Math.PI - (random.NextDouble() * (2 * Math.PI));
                latitude = (random.NextDouble() * Math.PI);

                x = (float)(radius * 2 * Math.Cos(longitude) * Math.Sin(latitude));
                z = (float)(radius * 2 * Math.Sin(longitude) * Math.Sin(latitude));
                y = (float)(radius * 2 * Math.Cos(latitude) + 10);
                generatedPoints[i] = new Vector3(x, y, z);
            }

            // create VBOs
            //
            staticVbo = new StaticVBO(generatedPoints, new Vector2[4]); // 0 set for tex coords

            // create image
            //
            Bitmap bmp = new Bitmap(Assembly.GetEntryAssembly().GetManifestResourceStream("TK_TestBed.Resources.circle.png"));
            pointSpriteImage = new PointSpriteImage(bmp);

            // init GL
            //
            GL.GetFloat(GetPName.PointSizeMax, out pointMaxSize);
            // Do not allow point sprites to exceed max size allowed by graphics card
            if (pointSize > (int)pointMaxSize) pointSize = (int)pointMaxSize;
        }

        public override void Draw()
        {
            // Specify point sprite texture coordinate replacement mode for each 
            // texture unit
            GL.TexEnv(TextureEnvTarget.PointSprite, TextureEnvParameter.CoordReplace, 1);
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.PointSprite);
            GL.Translate(0, -10, 0);
            // Draw VBO
            staticVbo.Draw(pointCount, PrimitiveType.Points);

            GL.Disable(EnableCap.PointSprite);
            GL.Disable(EnableCap.ProgramPointSize);
            
        }

        public override void Free()
        {
            staticVbo.Free();
            pointSpriteImage.Free();
        }
    }
}

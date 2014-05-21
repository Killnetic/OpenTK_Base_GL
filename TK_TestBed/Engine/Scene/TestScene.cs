using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TK_TestBed.Engine;
using TK_TestBed.Engine.Shader;
using TK_TestBed.Engine._3DObjects;

namespace TK_TestBed.Engine.Scene
{
    public class TestScene : BaseScene
    {
        private List<BaseObject3D> objects3d;
        private BasicSpriteShader spriteShader;
        private ParticleCloud particleCloud;
        private TextOverlay textOverlay;
        private float sceneDepth;

        public TestScene(string sceneName, Vector2 screenSize) : base(sceneName, screenSize)
        {
            // make some text
            //
            textOverlay = new TextOverlay((int)screenSize.X,(int)screenSize.Y);
            TextOverlay.TextEntry entry = new TextOverlay.TextEntry("hello", Brushes.WhiteSmoke, new Font(FontFamily.GenericSansSerif, 12.0f),new PointF(10,10));
            textOverlay.SubmitEntry("hello", entry);

            spriteShader = new BasicSpriteShader();
            objects3d = new List<BaseObject3D>();
            particleCloud = new ParticleCloud(1000000, 6);
            
            AddObject(particleCloud);

            sceneDepth = particleCloud.Diameter;
            camera = new Camera(70f, 0f, sceneDepth);

            modelView = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FovY), aspectRatio, camera.ZNear, camera.ZFar);
            GL.LoadMatrix(ref projectionMatrix);
            
            camera.position = new Vector3(0f,0f,sceneDepth/2);
            camera.targetPosition = particleCloud.position;

            /* TODO: build a scene
            var sprite = new AnimatedSprite("TK_TestBed.Resources.circle.png");
            AddObject(sprite);
            */
        }

        public override void AddObject(BaseObject3D baseObject3D)
        {
            objects3d.Add(baseObject3D);
        }

        public override void FreeObjects()
        {
            for (int i = 0; i < objects3d.Count; i++)
            {
                objects3d[i].Free();
            }
            objects3d = null;
            GC.Collect();
            objects3d = new List<BaseObject3D>();
        }

        private Matrix4 rotation;
        private DateTime actualTimeDelta = DateTime.Now;
        private float delta, cameraRotation;
        public override void DrawObjects()
        {
            textOverlay.Draw();
            
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            delta = (float)(DateTime.Now - actualTimeDelta).TotalSeconds;
            actualTimeDelta = DateTime.Now;
            cameraRotation = ((cameraRotation < 360f) ? (cameraRotation + delta * 0.1f) : 0f);
            rotation = Matrix4.CreateFromAxisAngle(Vector3.UnitY, cameraRotation);

            modelView = Matrix4.LookAt(camera.position, camera.targetPosition, Vector3.UnitY);
            modelView = Matrix4.Mult(rotation, modelView);
            GL.LoadMatrix(ref modelView);

            GL.UseProgram(spriteShader.ProgramID);
            GL.UniformMatrix4(spriteShader.MV, false, ref modelView);
            GL.UniformMatrix4(spriteShader.P, false, ref projectionMatrix);
            GL.Uniform2(spriteShader.screenSize, ref screenSize);
            GL.Uniform1(spriteShader.voxelSize, 24.0f);
            GL.Uniform1(spriteShader.maxDistance, Math.Max(sceneDepth, 1.0f));
            GL.Uniform1(spriteShader.pctAlpha, 0.8f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            GL.EnableVertexAttribArray(spriteShader.position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, particleCloud.staticVbo.vboID);
            GL.VertexAttribPointer(spriteShader.position, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            particleCloud.Draw();
            GL.DisableVertexAttribArray(spriteShader.position);

            GL.Disable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.UseProgram(0); // disable program

            
        }
    }
}

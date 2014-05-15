using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TK_TestBed.Engine.Scene
{
    public abstract class BaseScene : IEquatable<BaseScene>
    {
        protected Vector2 screenSize;
        protected Camera camera;
        protected Matrix4 projectionMatrix;
        protected Matrix4 modelView;
        protected float aspectRatio;
        public string SceneName { get; private set; }
        public BaseScene( string sceneName, Vector2 screenSize )
        {
            this.screenSize = screenSize;
            aspectRatio = screenSize.X/screenSize.Y;
            SceneName = sceneName;
        }

        public abstract void AddObject( BaseObject3D baseObject3D );
        public abstract void FreeObjects();
        public abstract void DrawObjects();

        public bool Equals( BaseScene other )
        {
            return SceneName.Equals(other.SceneName);
        }

        public void UpdateScreenSize(int width, int height)
        {
            screenSize = new Vector2(width, height);
            GL.Viewport(0, 0, width, height);
            aspectRatio = (float)width / height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FovY), aspectRatio, camera.ZNear, camera.ZFar);
            GL.LoadMatrix(ref projectionMatrix);
        }
    }
}

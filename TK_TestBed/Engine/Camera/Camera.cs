using OpenTK;

namespace TK_TestBed.Engine
{
    public class Camera
    {
        // camera should automatically clamp znear&zfar as close to the scene as possible
        // this will help make the depth buffer be more precise
        // zfar worth less than znear in depth buffer accuracy
        // thereforce znear changes could possibly make stereoscopic rendering disorienting if it changes too much
        //
        public Vector3 position, targetPosition;

        // Properties subject to optimization pass changes
        //
        public float FovY { get; private set; }
        public float ZFar { get; private set; }
        public float ZNear { get; private set; }

        /// <summary>
        /// creates a scene camera
        /// </summary>
        /// <param name="fovY"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        /// 
        public Camera(float fovY, float zNear, float zFar)
        {
            FovY = fovY;
            ZFar = zFar;
            ZNear = zNear + 0.4f;
            Reset();
            position = targetPosition = Vector3.Zero;
        }

        public void Reset()
        {
            targetPosition = Vector3.Zero;
            position = Vector3.Zero;
            position.Z = ZNear + 0.4f;
        }

    }
}

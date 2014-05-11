using OpenTK;

namespace TK_TestBed.Engine
{
    public abstract class BaseObject3D
    {
        /// <summary>
        ///  Draws object to device
        ///  </summary>
        public abstract void Draw();

        /// <summary>
        ///  Frees all resources associated with object
        ///  </summary>
        public abstract void Free();

        public Vector3 position;
    }
}

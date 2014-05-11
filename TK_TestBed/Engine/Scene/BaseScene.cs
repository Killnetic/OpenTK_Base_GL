using System;

namespace TK_TestBed.Engine.Scene
{
    public abstract class BaseScene : IEquatable<BaseScene>
    {
        public string SceneName { get; private set; }
        public BaseScene( string sceneName )
        {
            SceneName = sceneName;
        }

        public abstract void AddObject( BaseObject3D baseObject3D );
        public abstract void FreeObjects();
        public abstract void DrawObjects();

        public bool Equals( BaseScene other )
        {
            return SceneName.Equals(other.SceneName);
        }
    }
}

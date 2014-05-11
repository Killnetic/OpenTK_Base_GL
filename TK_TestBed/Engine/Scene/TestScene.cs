using System;
using System.Collections.Generic;
using TK_TestBed.Engine.Shader;
using TK_TestBed.Engine._3DObjects;

namespace TK_TestBed.Engine.Scene
{
    public class TestScene : BaseScene
    {
        private List<BaseObject3D> objects3d;

        public TestScene(string sceneName) : base(sceneName)
        {
            var shader = new BasicSpriteShader();

            objects3d = new List<BaseObject3D>();
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

        public override void DrawObjects()
        {
            for (int i = 0; i < objects3d.Count; i++)
            {
                objects3d[i].Draw();
            }
        }
    }
}

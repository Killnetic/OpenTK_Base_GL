using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace TK_TestBed.Engine.GPU
{
    class PointSpriteImage
    {
        public StaticVBO StaticVbo;
        public int texture;
        public int w, h;

        public PointSpriteImage(Bitmap bitmap)
        {
            StaticVbo = new StaticVBO(new Vector3[4], new Vector2[4]); // quad + tex coords
            Load(bitmap);
        }

        public void Load(Bitmap bitmap)
        {

            // Generate texture
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Store texture size
            w = bitmap.Width;
            h = bitmap.Height;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            // Setup filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        /// <summary>
        /// Deletes texture from GPU memory
        /// </summary>
        public void Free()
        {
            GL.DeleteTextures(1, ref texture);
        }
    }
}

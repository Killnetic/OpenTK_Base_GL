using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace TK_TestBed.Engine._3DObjects
{
    class AnimatedSprite : BaseObject3D
    {

        public int texture;            // Holds image data
        public int width, height;
        // GL assigned memory pointers
        //
        private int vboID, texID;

        public override void Draw()
        {
            throw new NotImplementedException();
        }

        public override void Free()
        {
            FreeTexture();
        }

        public override void Resize(int height, int width)
        {

        }

        public AnimatedSprite()
        {
            position = Vector3.Zero;
        }

        public AnimatedSprite(string embeddedResourceName)
            : base()
        {
            LoadFromEmbed(embeddedResourceName);
        }

        public AnimatedSprite(FileInfo file)
            : base()
        {
            Load(file);
        }

        public AnimatedSprite(Bitmap bitmap)
            : base()
        {
            Load(bitmap);
        }

        /// <summary>
        /// Loads image from embedded resource
        /// </summary>
        /// <param name="resourceName">Embedded resource path and file name ex: AssemblyName.Resources.image.png</param>
        public void LoadFromEmbed(string resourceName)
        {
            Stream bitmapStream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName);
            if (bitmapStream != null)
                Load(new Bitmap(bitmapStream));
            else
                throw new NullReferenceException("Unable to load image from embedded resource");

        }

        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="file">The image file to be loaded</param>
        public void Load(FileInfo file)
        {
            using (var reader = new BinaryReader(File.OpenRead(file.FullName)))
            {
                var bitmapStream = new MemoryStream();
                reader.BaseStream.CopyTo(bitmapStream);
                Load(new Bitmap(bitmapStream));
            }
        }

        /// <summary>
        /// Buffers bitmap to GPU,
        /// </summary>
        /// <param name="bitmap"></param>
        public void Load(Bitmap bitmap)
        {
            // Free any existing texture
            //
            FreeTexture();

            // Generate texture
            //
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Store texture size
            //
            width = bitmap.Width;
            height = bitmap.Height;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            // Setup filtering
            //
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private void FreeTexture()
        {
            /* khronos.org
             * 
             * glDeleteTextures deletes n textures named by the elements of the array textures.
            After a texture is deleted, it has no contents or dimensionality,
            and its name is free for reuse (for example by glGenTextures).
            If a texture that is currently bound is deleted, the binding reverts
            to 0 (the default texture).
        
            glDeleteTextures silently ignores 0's and names that do not correspond to
            existing textures.
            */

            if (texture != 0) GL.DeleteTextures(1, ref texture);

            // Delete old VBOs
            if (texID != 0) GL.DeleteBuffers(1, ref texID);
            if (vboID != 0) GL.DeleteBuffers(1, ref vboID);

            // TODO investigate texture value after success or failed deletions
            // there may be a need to set texture's value to 0 or -1 manually indicating it has been freed
            //
            //
        }
    }
}

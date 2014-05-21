using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace TK_TestBed.Engine._3DObjects
{
    class TextOverlay : BaseObject3D
    {
        // TODO: add font sizing

        private Dictionary<string, TextEntry> overlayStrings;
        public int Count {
            get { return overlayStrings.Count; }
        }

        public struct TextEntry
        {
            public Brush brush;
            public Font font;
            public PointF point;
            public string text;
            public TextEntry(string text, Brush brush, Font font, PointF point)
            {
                this.text = text;
                this.brush = brush;
                this.font = font;
                this.point = point;
            }
        }
        Font sans = new Font(FontFamily.GenericSansSerif, 24);
        Bitmap bmp;
        Graphics gfx;
        int texture;
        Rectangle dirty_region;
        bool disposed;

        int width, height;
        private bool invalidateTexture;

        public TextOverlay(int width, int height)
        {
            this.width = width;
            this.height = height;
            overlayStrings = new Dictionary<string, TextEntry>();

            // this is a bitmap overlay pattern and needs to be the size of the screen
            // having a single bitmap overlay will allow us to draw multiple strings on
            // one texture and produce only one draw call
            //

            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height ");
            if (GraphicsContext.CurrentContext == null)
                throw new InvalidOperationException("No GraphicsContext is current on the calling thread.");

            bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfx = Graphics.FromImage(bmp);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            EC();
        }

        /// <summary>
        /// Clears the backing store to the specified color.
        /// </summary>
        /// <param name="color">A <see cref="System.Drawing.Color"/>.</param>
        public void Clear(Color color)
        {
            gfx.Clear(color);
            dirty_region = new Rectangle(0, 0, bmp.Width, bmp.Height);
        }

        /// <summary>
        /// Draws the specified string to the backing store.
        /// </summary>
        /// <param name="text">The <see cref="System.String"/> to draw.</param>
        /// <param name="font">The <see cref="System.Drawing.Font"/> that will be used.</param>
        /// <param name="brush">The <see cref="System.Drawing.Brush"/> that will be used.</param>
        /// <param name="point">The location of the text on the backing store, in 2d pixel coordinates.
        /// The origin (0, 0) lies at the top-left corner of the backing store.</param>
        public void DrawString(string text, Font font, Brush brush, PointF point)
        {
            gfx.DrawString(text, font, brush, point);

            SizeF size = gfx.MeasureString(text, font);
            dirty_region = Rectangle.Round(RectangleF.Union(dirty_region, new RectangleF(point, size)));
            dirty_region = Rectangle.Intersect(dirty_region, new Rectangle(0, 0, bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Gets a <see cref="System.Int32"/> that represents an OpenGL 2d texture handle.
        /// The texture contains a copy of the backing store. Bind this texture to TextureTarget.Texture2d
        /// in order to render the drawn text on screen.
        /// </summary>
        public int Texture
        {
            get
            {
                UploadBitmap();
                return texture;
            }
        }

        // Uploads the dirty regions of the backing store to the OpenGL texture.
        void UploadBitmap()
        {
            if (dirty_region != RectangleF.Empty)
            {
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(dirty_region,
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    dirty_region.X, dirty_region.Y, dirty_region.Width, dirty_region.Height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);

                dirty_region = Rectangle.Empty;
            }
        }

        void Dispose(bool manual)
            {
                if (!disposed)
                {
                    if (manual)
                    {
                        bmp.Dispose();
                        gfx.Dispose();
                        if (GraphicsContext.CurrentContext != null)
                            GL.DeleteTexture(texture);
                    }

                    disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~TextOverlay()
            {
                Console.WriteLine("[Warning] Resource leaked: {0}.", typeof(TextOverlay));
            }


        private void EC()
        {
            var error = GL.GetError();
            if (error != 0)
            {
                string problem = error.ToString();
                Console.WriteLine("\r\n!!! ERROR Build Text !!!");
                Console.WriteLine("\r\n" + problem);
                Console.ReadKey(true);
                throw new Exception(problem);
            }
        }

        public override void Resize(int height, int width)
        {
            this.width = width;
            this.height = height;
            
        }

        /// <summary>
        /// Update existing entry or add new entry
        /// </summary>
        /// <param name="key">the unique identifier of the text entry</param>
        /// <param name="entry">the value</param>
        public void SubmitEntry(string key, TextEntry entry)
        {
            if (overlayStrings.ContainsKey(key))
            { // probably an update call
                if (! entry.text.Equals(overlayStrings[key].text))
                { // text has been updated, update bitmap
                    invalidateTexture = true;
                }
                overlayStrings[key] = entry;
            }
            else
            {
                overlayStrings.Add(key, entry);
                invalidateTexture = true;
            }
        }

        /// <summary>
        /// Remove entry if it exists
        /// </summary>
        /// <param name="key">the unique identifier of the text entry to remove</param>
        public void RemoveEntry(string key)
        {
            if (overlayStrings.ContainsKey(key))
            {
                overlayStrings.Remove(key);
            }
        }

        public override void Draw()
        {
            if (invalidateTexture)
            {
                invalidateTexture = false;
                foreach (var entry in overlayStrings.Values)
                {
                    DrawString(entry.text, entry.font, entry.brush, entry.point);
                }
                bmp.Save("C:\\whatever2.png", ImageFormat.Png);
            }
            GL.PushMatrix();
            GL.UseProgram(0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1f, -1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1f, -1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1f, 1f);
            
            GL.End();

            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
        }

        public override void Free()
        {
            Dispose();
        }
    }
}

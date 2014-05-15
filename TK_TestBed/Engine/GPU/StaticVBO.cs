using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TK_TestBed.Engine.GPU
{
    class StaticVBO
    {
        public Vector3[] vertices;
        public Vector2[] texcoords;
        public int vboID, texID;

        public StaticVBO(Vector3[] vertices, Vector2[] texcoords)
        {
            this.vertices = vertices;
            this.texcoords = texcoords;
            GL.GenBuffers(1, out vboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vector3.SizeInBytes), vertices, BufferUsageHint.StaticDraw);
            GL.GenBuffers(1, out texID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, texID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texcoords.Length * Vector2.SizeInBytes), texcoords, BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Draws StaticVBO.
        /// </summary>
        /// <param name="length">Number of vertices to be drawn from array.</param>
        /// <param name="mode">Mode used for drawing.</param>
        public void Draw(int length, PrimitiveType mode)
        {
            // Use VBOs if they are supported
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, texID);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, IntPtr.Zero);

            GL.DrawArrays(mode, 0, length);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);

        }

        /// <summary>
        /// Deletes arrays from GPU memory
        /// </summary>
        public void Free()
        {
            if (vboID != 0) GL.DeleteBuffers(1, ref vboID);
            if (texID != 0) GL.DeleteBuffers(1, ref texID);
        }
    }
}

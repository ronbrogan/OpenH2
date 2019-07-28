using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Rendering.Abstractions
{
    /// <summary>
    /// Provides an abstraction over the underlying graphics driver
    /// </summary>
    public interface IGraphicsAdapter
    {
        void UseMatricies(MatriciesUniform matricies);
        void DrawMesh(Mesh mesh, IMaterial<Bitmap> material);
    }
}

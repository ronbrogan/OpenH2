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
        void UploadMesh(object mesh);
        void DrawMesh(object mesh);
    }
}

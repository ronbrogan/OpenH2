using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using System.Numerics;

namespace OpenH2.Rendering.Abstractions
{
    /// <summary>
    /// Provides an abstraction over the underlying graphics driver
    /// </summary>
    public interface IGraphicsAdapter
    {
        void BeginFrame(GlobalUniform matricies);

        void UseShader(Shader shader);
        void SetSunLight(Vector3 sunDirection);
        void AddLight(PointLight light);
        void DrawMesh(Mesh<BitmapTag> mesh, Matrix4x4 transform);
        void EndFrame();
    }
}

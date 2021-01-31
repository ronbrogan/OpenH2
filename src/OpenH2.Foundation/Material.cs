using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Foundation
{
    public interface IMaterial
    {
        Vector4 DiffuseColor { get; set; }

        Vector3 SpecularColor { get; set; }
    }

    public interface IMaterial<TTexture> : IMaterial
    {
        TTexture DiffuseMap { get; set; }

        TTexture AlphaMap { get; set; }
        bool AlphaFromRed { get; set; }

        TTexture AnimationMap { get; set; }

        TTexture SpecularMap { get; set; }

        TTexture EmissiveMap { get; set; }
        EmissiveType EmissiveType { get; set; }
        Vector4 EmissiveArguments { get; set; }

        TTexture NormalMap { get; set; }
        Vector4 NormalMapScale { get; set; }

        TTexture DetailMap1 { get; set; }
        Vector4 Detail1Scale { get; set; }

        TTexture DetailMap2 { get; set; }
        Vector4 Detail2Scale { get; set; }

        TTexture ColorChangeMask { get; set; }
    }

    public record Material<TTexture> : IMaterial<TTexture>
    {

        public Vector4 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        public TTexture DiffuseMap { get; set; }

        public TTexture AlphaMap { get; set; }
        public bool AlphaFromRed { get; set; }

        public TTexture AnimationMap { get; set; }

        public TTexture EmissiveMap { get; set; }
        public EmissiveType EmissiveType { get; set; }
        public Vector4 EmissiveArguments { get; set; }

        public TTexture NormalMap { get; set; }
        public Vector4 NormalMapScale { get; set; } = new Vector4(1, 1, 0, 0);

        public TTexture DetailMap1 { get; set; }
        public Vector4 Detail1Scale { get; set; } = new Vector4(1, 1, 0, 0);

        public TTexture DetailMap2 { get; set; }
        public Vector4 Detail2Scale { get; set; } = new Vector4(1, 1, 0, 0);

        public TTexture ColorChangeMask { get; set; }
        public Vector4 ColorChangeColor { get; set; }

        public TTexture SpecularMap { get; set; }
    }
}

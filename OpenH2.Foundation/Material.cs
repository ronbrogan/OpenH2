using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation
{
    public interface IMaterial
    {
        Vector3 DiffuseColor { get; set; }
    }

    public interface IMaterial<TTexture> : IMaterial
    {
        TTexture DiffuseMap { get; set; }

        TTexture EmissiveMap { get; set; }

        TTexture NormalMap { get; set; }

        TTexture DetailMap1 { get; set; }

        TTexture DetailMap2 { get; set; }
    }

    public class Material<TTexture> : IMaterial<TTexture>
    {

        public Vector3 DiffuseColor { get; set; }

        public TTexture DiffuseMap { get; set; }
        public TTexture EmissiveMap { get; set; }
        public TTexture NormalMap { get; set; }
        public TTexture DetailMap1 { get; set; }
        public TTexture DetailMap2 { get; set; }
    }
}

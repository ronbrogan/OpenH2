using System.Numerics;

namespace OpenH2.Foundation
{
    public interface IMaterial
    {
        Vector3 DiffuseColor { get; set; }

        Vector3 SpecularColor { get; set; }
    }

    public interface IMaterial<TTexture> : IMaterial
    {
        TTexture DiffuseMap { get; set; }
        long DiffuseHandle { get; set; }

        TTexture SpecularMap { get; set; }
        long SpecularHandle { get; set; }

        TTexture EmissiveMap { get; set; }
        long EmissiveHandle { get; set; }

        TTexture NormalMap { get; set; }
        long NormalHandle { get; set; }

        TTexture DetailMap1 { get; set; }
        long Detail1Handle { get; set; }

        TTexture DetailMap2 { get; set; }
        long Detail2Handle { get; set; }
    }

    public class Material<TTexture> : IMaterial<TTexture>
    {

        public Vector3 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        public TTexture DiffuseMap { get; set; }
        public long DiffuseHandle { get; set; }
        public TTexture EmissiveMap { get; set; }
        public long EmissiveHandle { get; set; }
        public TTexture NormalMap { get; set; }
        public long NormalHandle { get; set; }
        public TTexture DetailMap1 { get; set; }
        public long Detail1Handle { get; set; }
        public TTexture DetailMap2 { get; set; }
        public long Detail2Handle { get; set; }
        public TTexture SpecularMap { get; set; }
        public long SpecularHandle { get; set; }
    }
}

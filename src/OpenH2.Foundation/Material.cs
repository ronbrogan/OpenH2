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
    }

    public record Material<TTexture> : IMaterial<TTexture>
    {

        public Vector4 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        public TTexture DiffuseMap { get; set; }

        public TTexture AlphaMap { get; set; }

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

        public TTexture SpecularMap { get; set; }

        //public override bool Equals(object obj)
        //{
        //    return obj is Material<TTexture> material &&
        //           EqualityComparer<TTexture>.Default.Equals(this.DiffuseMap, material.DiffuseMap) &&
        //           EqualityComparer<TTexture>.Default.Equals(this.AlphaMap, material.AlphaMap) &&
        //           EqualityComparer<TTexture>.Default.Equals(this.EmissiveMap, material.EmissiveMap) &&
        //           EqualityComparer<TTexture>.Default.Equals(this.NormalMap, material.NormalMap) &&
        //           EqualityComparer<TTexture>.Default.Equals(this.DetailMap1, material.DetailMap1) &&
        //           this.Detail1Scale.Equals(material.Detail1Scale) &&
        //           EqualityComparer<TTexture>.Default.Equals(this.DetailMap2, material.DetailMap2) &&
        //           this.Detail2Scale.Equals(material.Detail2Scale) &&
        //           EqualityComparer<TTexture>.Default.Equals(this.SpecularMap, material.SpecularMap);
        //}

        //public override int GetHashCode()
        //{
        //    var hashCode = 284558436;
        //    hashCode = hashCode * -1521134295 + EqualityComparer<TTexture>.Default.GetHashCode(this.DiffuseMap);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<TTexture>.Default.GetHashCode(this.AlphaMap);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<TTexture>.Default.GetHashCode(this.EmissiveMap);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<TTexture>.Default.GetHashCode(this.NormalMap);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<TTexture>.Default.GetHashCode(this.DetailMap1);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(this.Detail1Scale);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<TTexture>.Default.GetHashCode(this.DetailMap2);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(this.Detail2Scale);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<TTexture>.Default.GetHashCode(this.SpecularMap);
        //    return hashCode;
        //}
    }
}

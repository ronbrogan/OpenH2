using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Foundation;
using System;

namespace OpenH2.Core.Factories
{
    public interface IMaterialFactory
    {
        void AddListener(Action callback);
        Material<BitmapTag> CreateMaterial(H2vMap map, ModelMesh mesh);
    }

    public sealed class NullMaterialFactory : IMaterialFactory
    {
        public static NullMaterialFactory Instance = new NullMaterialFactory();

        public void AddListener(Action callback) { }

        public Material<BitmapTag> CreateMaterial(H2vMap map, ModelMesh mesh) => new Material<BitmapTag>();
    }
}
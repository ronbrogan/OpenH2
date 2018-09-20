using OpenH2.Core.Factories;
using System;
using System.IO;
using Xunit;

namespace OpenH2.Core.Tests
{
    public class SceneFactoryTests
    {
        [Fact]
        public void Load_scene_from_file()
        {
            var mapStream = new FileStream("ascension.map", FileMode.Open, FileAccess.Read, FileShare.Read);

            var factory = new SceneFactory();

            var scene = factory.FromFile(mapStream);

            Assert.NotNull(scene);
            Assert.Equal("ascension", scene.Name);
            Assert.Equal(8, scene.Header.Version);
            Assert.Equal(16059904, scene.Header.TotalBytes);
            Assert.Equal(14503424, scene.Header.IndexOffset.Value);
            Assert.Equal(245760, scene.Header.MetaOffset.OriginalValue);
            Assert.Equal(14503424, scene.PrimaryMagic);
            Assert.Equal(-6468096, scene.SecondaryMagic);
        }

        [Fact]
        public void Calculated_signature_matches_stored_signature()
        {

            var mapStream = new FileStream("ascension.map", FileMode.Open, FileAccess.Read, FileShare.Read);

            var factory = new SceneFactory();

            var scene = factory.FromFile(mapStream);

            var sig = scene.CalculateSignature();

            Assert.Equal(scene.Header.StoredSignature, sig);
        }

        

    }
}

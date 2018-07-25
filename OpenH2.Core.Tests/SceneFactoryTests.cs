using OpenH2.Core.Factories;
using System;
using System.IO;
using Xunit;

namespace OpenH2.Core.Tests
{
    public class SceneFactoryTests
    {
        [Fact]
        public void LoadMapTest()
        {
            var mapStream = new FileStream("ascension.map", FileMode.Open);

            var factory = new SceneFactory();

            var scene = factory.FromFile(mapStream);

            Assert.NotNull(scene);
            Assert.Equal("ascension", scene.Name);
            Assert.Equal(8, scene.Metadata.Version);
        }
    }
}

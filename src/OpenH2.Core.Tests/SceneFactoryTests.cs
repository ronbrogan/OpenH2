using OpenH2.Core.Factories;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Core.Maps.Vista;

namespace OpenH2.Core.Tests
{
    public class SceneFactoryTests
    {
        private readonly ITestOutputHelper output;
        private readonly string ascensionPath = @"D:\H2vMaps\ascension.map";

        public SceneFactoryTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact, Trait("skip", "true")]
        public void Load_scene_from_file()
        {
            var factory = new MapFactory(Path.GetDirectoryName(ascensionPath));

            var sw = new Stopwatch();
            sw.Restart();

            var h2map = factory.Load(Path.GetFileName(ascensionPath));

            if (h2map is not H2vMap scene)
            {
                throw new NotSupportedException("Only Vista maps are supported");
            }

            sw.Stop();

            Assert.NotNull(scene);
            Assert.Equal("ascension", scene.Name);
            Assert.Equal(8, scene.Header.Version);
            Assert.Equal(14503424, scene.Header.IndexOffset.Value);
            Assert.Equal(245760, scene.Header.SecondaryOffset.OriginalValue);
            Assert.Equal(14503424, scene.PrimaryMagic);
            Assert.Equal(6468096, scene.SecondaryMagic);

            //Assert.Equal(scene.IndexHeader.ObjectCount, scene.TagIndex.Count);

            var scnr = scene.GetLocalTagsOfType<ScenarioTag>().First();

            output.WriteLine($"Scene parsing took: {sw.ElapsedMilliseconds}ms");
        }

        
        [Fact, Trait("skip", "true")]
        public void Calculated_signature_matches_stored_signature()
        {
            var factory = new MapFactory(Path.GetDirectoryName(ascensionPath));
            var h2map = factory.Load(Path.GetFileName(ascensionPath));

            if (h2map is not H2vMap scene)
            {
                throw new NotSupportedException("Only Vista maps are supported");
            }

            var sig = scene.CalculateSignature();

            Assert.Equal(scene.Header.StoredSignature, sig);
        }
    }
}
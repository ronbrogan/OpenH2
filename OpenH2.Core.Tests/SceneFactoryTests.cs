using OpenH2.Core.Factories;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenH2.Core.Tags;
using Xunit;
using Xunit.Abstractions;

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
            var mapStream = new FileStream(ascensionPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var factory = new MapFactory(Path.GetDirectoryName(ascensionPath));

            var sw = new Stopwatch();
            sw.Restart();

            var scene = factory.FromFile(mapStream, out var coverage);
            mapStream.Dispose();
            sw.Stop();

            Assert.NotNull(scene);
            Assert.Equal("ascension", scene.Name);
            Assert.Equal(8, scene.Header.Version);
            Assert.Equal(16059904, scene.Header.TotalBytes);
            Assert.Equal(14503424, scene.Header.IndexOffset.Value);
            Assert.Equal(245760, scene.Header.MetaOffset.OriginalValue);
            Assert.Equal(14503424, scene.PrimaryMagic);
            Assert.Equal(6468096, scene.SecondaryMagic);

            //Assert.Equal(scene.IndexHeader.ObjectCount, scene.TagIndex.Count);

            var scnr = scene.Tags.Values.First(v => v.GetType() == typeof(ScenarioTag)) as ScenarioTag;

            output.WriteLine($"Scene parsing took: {sw.ElapsedMilliseconds}ms and covered: {coverage.PercentCovered.ToString("0.00")}%");
        }

        
        [Fact, Trait("skip", "true")]
        public void Calculated_signature_matches_stored_signature()
        {
            var mapStream = new FileStream(ascensionPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var factory = new MapFactory(Path.GetDirectoryName(ascensionPath));

            var scene = factory.FromFile(mapStream);

            mapStream.Dispose();

            var sig = scene.CalculateSignature();

            Assert.Equal(scene.Header.StoredSignature, sig);
        }

        
    }
}
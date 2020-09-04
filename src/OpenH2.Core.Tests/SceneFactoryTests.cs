using OpenH2.Core.Factories;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenH2.Core.Extensions;
using Xunit;
using Xunit.Abstractions;
using OpenH2.Core.Representations;
using System;
using OpenH2.Core.Tags.Scenario;

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

            var materialFactory = new MaterialFactory(Path.Combine(Environment.CurrentDirectory, "Configs"));
            var factory = new MapFactory(Path.GetDirectoryName(ascensionPath), materialFactory);

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

            var scnr = scene.GetLocalTagsOfType<ScenarioTag>().First();

            output.WriteLine($"Scene parsing took: {sw.ElapsedMilliseconds}ms and covered: {coverage.PercentCovered.ToString("0.00")}%");
        }

        
        [Fact, Trait("skip", "true")]
        public void Calculated_signature_matches_stored_signature()
        {
            var mapStream = new FileStream(ascensionPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var raw = mapStream.ToMemory();
            mapStream.Seek(0, SeekOrigin.Begin);


            var materialFactory = new MaterialFactory(Path.Combine(Environment.CurrentDirectory, "Configs"));
            var factory = new MapFactory(Path.GetDirectoryName(ascensionPath), materialFactory);

            var scene = factory.FromFile(mapStream);

            mapStream.Dispose();

            var sig = H2vMap.CalculateSignature(raw);

            Assert.Equal(scene.Header.StoredSignature, sig);
        }
    }
}
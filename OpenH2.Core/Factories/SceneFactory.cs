using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using System;
using System.IO;
using System.Text;

namespace OpenH2.Core.Factories
{
    public class SceneFactory
    {

        public SceneFactory()
        {

        }

        public Scene FromFile(Stream fileStream)
        {
            var data = new byte[fileStream.Length];

            fileStream.Read(data, 0, (int)fileStream.Length);

            var header = new Span<byte>(data, 0, 2048);

            // get header span from fileStream, pass to GetMetadata method
            var meta = this.GetMetadata(header);




            var scene = new Scene();
            scene.Metadata = meta;
            

            return scene;
        }

        private SceneMetadata GetMetadata(Span<byte> data)
        {
            var factory = new MetadataFactory();
            return factory.Create(data);
        }

    }
}
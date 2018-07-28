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

            var memory = new Memory<byte>(data);

            var headerData = memory.Slice(0, 2048).Span;

            var head = this.GetMetadata(headerData);

            var scene = new Scene();
            scene.RawData = memory;
            scene.Header = head;
            
            return scene;
        }

        private SceneHeader GetMetadata(Span<byte> data)
        {
            var factory = new HeaderFactory();
            return factory.Create(data);
        }

    }
}
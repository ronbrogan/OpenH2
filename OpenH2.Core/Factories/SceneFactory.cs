using OpenH2.Core.Representations;
using System.IO;

namespace OpenH2.Core.Factories
{
    public class SceneFactory
    {

        public SceneFactory()
        {

        }

        public Scene FromFile(Stream fileStream)
        {
            // get header span from fileStream, pass to GetMetadata method
            var meta = this.GetMetadata();




            var scene = new Scene();
            scene.Metadata = meta;


            return scene;
        }

        private SceneMetadata GetMetadata(Span<byte> data)
        {


            var meta = new SceneMetadata();

            return meta;
        }

    }
}
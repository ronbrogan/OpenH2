using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags.Processors
{
    public class UniversalTagProcessor
    { 
        public T Process<T>(uint id, string name, TagIndexEntry index, TrackingChunk chunk, TrackingReader sceneReader)
            where T: BaseTag, new()
        {
            var props = TagTypeMetadataProvider.GetProperties(typeof(T));

            var tag = new T();

            foreach(var prop in props)
            {
                switch (prop.LayoutAttribute)
                {
                    case PrimitiveValueAttribute prim:
                    {
                       
                        break;
                    }


                        
                }



            }

            return tag;
        }
    }
}

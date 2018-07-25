using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Factories
{
    public class MetadataFactory
    {
        public SceneMetadata Create(Span<byte> data)
        {
            var meta = new SceneMetadata();

            meta.FileHead = GetFileHeadValue(data);
            meta.Version = GetVersion(data);
            meta.Name = GetName(data);


            return meta;
        }

        public string GetFileHeadValue(Span<byte> data)
        {
            var offset = 0;
            var length = 4;

            return data.StringFromSlice(offset, length);
        }

        public int GetVersion(Span<byte> data)
        {
            return data.IntFromSlice(4);
        }

        public string GetName(Span<byte> data)
        {
            var offset = 420;
            var length = 32;

            return data.StringFromSlice(offset, length);
        }

        
    }
}

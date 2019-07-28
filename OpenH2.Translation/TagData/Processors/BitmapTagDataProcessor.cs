using OpenH2.Core.Enums;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using OpenH2.Core.Utilities;
using System;
using System.IO;
using System.IO.Compression;

namespace OpenH2.Translation.TagData.Processors
{
    public static class BitmapTagDataProcessor
    {
        public static BitmapTagData ProcessTag(BaseTag tag)
        {
            var bitmap = tag as Bitmap;

            if (bitmap == null)
                throw new ArgumentException("Tag must be a Bitmap", nameof(tag));
            
            var tagData = new BitmapTagData(bitmap);

            // Decompress and synthesize texture headers
            for (var i = 0; i < bitmap.LevelsOfDetail.Length; i++)
            {
                var lod = bitmap.LevelsOfDetail[i];

                if (lod.Data.IsEmpty)
                    continue;

                var ms = new MemoryStream();
                BitmUtils.WriteTextureHeader(bitmap, ms);
                ms.Write(lod.Data.ToArray(), 0, lod.Data.Length);

                tagData.Levels[i] = ms.ToArray();
            }

            return tagData;
        }
    }
}

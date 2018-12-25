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
            var bitmap = tag as BitmapTag;

            if (bitmap == null)
                throw new ArgumentException("Tag must be a BitmapTag", nameof(tag));
            
            var tagData = new BitmapTagData(bitmap);

            // Decompress and synthesize texture headers
            for (var i = 0; i < bitmap.LevelsOfDetail.Length; i++)
            {
                var lod = bitmap.LevelsOfDetail[i];

                // TODO: Implement shared map retrieval
                if (lod.Offset.Location != DataFile.Local)
                    continue;
                else if (lod.Data.IsEmpty)
                    continue;



                // Need to offset 2 bytes into data to bypass zlib header for compatibility with DeflateStream
                using (var inputStream = new MemoryStream(lod.Data.Span.Slice(2).ToArray()))
                using (var decompress = new DeflateStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream((int)inputStream.Length))
                {
                    BitmUtils.WriteTextureHeader(bitmap, outputStream);
                    decompress.CopyTo(outputStream);

                    tagData.Levels[i] = new Memory<byte>(outputStream.GetBuffer()).Slice(0, (int)outputStream.Length);
                }
            }

            return tagData;
        }
    }
}

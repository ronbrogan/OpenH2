using OpenH2.Core.Offsets;
using OpenH2.Core.Maps;
using System.IO;
using OpenBlam.Core.MapLoading;
using OpenH2.Core.Enums;

namespace OpenH2.Core.Extensions
{
    public static class MapExtensions
    {
        public static PrimaryOffset PrimaryOffset(this IH2Map scene, int value)
        {
            return new PrimaryOffset(scene, value);
        }

        public static Stream GetStream(this MapStream mapStream, DataFile location)
        {
            return mapStream.GetStream((byte)location);
        }

        public static Stream GetStream(this MapStream mapStream, NormalOffset offset)
        {
            return mapStream.GetStream((byte)offset.Location);
        }
    }
}
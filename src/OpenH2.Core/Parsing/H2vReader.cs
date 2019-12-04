using OpenH2.Core.Enums;
using OpenH2.Core.Offsets;
using System;

namespace OpenH2.Core.Parsing
{
    public class H2vReader : IDisposable
    {
        public TrackingReader MapReader { get; }
        public TrackingReader MainMenu { get; }
        public TrackingReader MpShared { get; }
        public TrackingReader SpShared { get; }

        public H2vReader(TrackingReader mainMenu, TrackingReader mpShared, TrackingReader spShared)
        {
            this.MainMenu = mainMenu;
            this.MpShared = mpShared;
            this.SpShared = spShared;
        }

        public H2vReader(TrackingReader mapReader, H2vReader baseReader)
        {
            this.MapReader = mapReader;
            this.MainMenu = baseReader.MainMenu;
            this.MpShared = baseReader.MpShared;
            this.SpShared = baseReader.SpShared;
        }

        public TrackingReader GetReader(NormalOffset offset)
        {
            switch (offset.Location)
            {
                case DataFile.Local:
                    return MapReader;
                case DataFile.MainMenu:
                    return MainMenu;
                case DataFile.Shared:
                    return MpShared;
                case DataFile.SinglePlayerShared:
                    return SpShared;
                default:
                    return MapReader;
            }
        }

        public DataFile GetPrimaryDataFile()
        {
            if(this.MapReader == this.SpShared)
            {
                return DataFile.SinglePlayerShared;
            } 

            if(this.MapReader == this.MpShared)
            {
                return DataFile.Shared;
            }

            if(this.MapReader == this.MainMenu)
            {
                return DataFile.MainMenu;
            }

            return DataFile.Local;
        }

        public TrackingChunk Chunk(NormalOffset offset, int size, string label = null)
        {
            TrackingReader reader = GetReader(offset);

            return reader.Chunk(offset.Value, size, label);
        }

        public void Dispose()
        {
            this.MainMenu?.Dispose();
            this.SpShared?.Dispose();
            this.MpShared?.Dispose();
            this.MapReader?.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    public class TagViewModel
    {
        public TagViewModel(uint id, string tag)
        {
            this.Id = id;
            this.Name = tag;
        }

        public uint Id { get; set; }

        public string Name { get; set; }

        public byte[] Data { get; set; }

        public int InternalOffsetStart { get; set; }

        public int InternalOffsetEnd { get; set; }

    }
}

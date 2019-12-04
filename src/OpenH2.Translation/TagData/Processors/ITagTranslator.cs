using System;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Tags;

namespace OpenH2.Translation.TagData.Processors
{
    public interface ITagTranslator
    {
        BaseTagData ToTagData(BaseTag tag);
    }
}

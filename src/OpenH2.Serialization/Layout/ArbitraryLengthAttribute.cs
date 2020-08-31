using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Serialization.Layout
{
    /// <summary>
    /// Designates a type as serializable, but of unknown length. 
    /// As a consequence it can not be used as the target type of an array
    /// </summary>
    public sealed class ArbitraryLengthAttribute : SerializableTypeAttribute
    {
    }
}

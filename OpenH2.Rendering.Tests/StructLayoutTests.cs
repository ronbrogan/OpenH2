using OpenH2.Rendering.Shaders.Generic;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace OpenH2.Rendering.Tests
{
    public class StructLayoutTests
    {
        [Fact]
        public void GenericUniformSanityCheck()
        {
            var targetSize = 0;

            foreach (var field in typeof(GenericUniform).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                targetSize += Marshal.SizeOf(field.FieldType);
            }

            Assert.Equal(targetSize, GenericUniform.Size);
        }
    }
}

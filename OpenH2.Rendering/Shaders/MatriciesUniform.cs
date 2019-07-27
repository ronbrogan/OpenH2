﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenH2.Rendering.Shaders
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MatriciesUniform
    {
        [FieldOffset(0)]
        public Matrix4x4 ViewMatrix;

        [FieldOffset(64)]
        public Matrix4x4 ProjectionMatrix;

        [FieldOffset(128)]
        public System.Numerics.Vector3 ViewPosition;

        public static readonly int Size = BlittableValueType<MatriciesUniform>.Stride;

    }
}
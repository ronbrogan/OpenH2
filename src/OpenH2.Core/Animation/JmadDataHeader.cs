namespace OpenH2.Core.Animation
{
    public enum JmadDataType
    {
        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present
        /// Typically (always?) comes as separate section before animation data
        /// </summary>
        Flat = 1,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values in groups of FrameCount items
        /// Has 'Flat' preamble
        /// </summary>
        Three = 3,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index
        /// Has 'Flat' preamble
        /// </summary>
        Four = 4,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index.
        /// This format uses shorts for the FrameMapping, as this is commonly used for
        /// very long animations.
        /// Has 'Flat' preamble
        /// </summary>
        Five = 5,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index
        /// Has 'Flat' preamble
        /// </summary>
        Six = 6,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index.
        /// This format uses shorts for the FrameMapping, as this is commonly used for
        /// very long animations.
        /// Has 'Flat' preamble
        /// </summary>
        Seven = 7,

        /// <summary>
        /// Quaternions and Vector3 are present in the animation data as single-precision
        /// floating point values in groups of FrameCount items
        /// No pre-header data
        /// </summary>
        UncompressedFlat = 8
    }

    public struct JmadDataHeader
    {
        public JmadDataType Type;
        public int OrientationCount;
        public int TranslationCount;
        public int ScaleCount;

        public int UnknownOrientationItemOffset;
        public int UnknownTranslationItemOffset;
        public int UnknownScaleItemOffset;

        public bool ShortsForFrameMapping;
        public int OrientationFrameMapping;
        public int TranslationFrameMapping;
        public int ScaleFrameMapping;

        public int OrientationOffset;
        public int TranslationOffset;
        public int ScaleOffset;

        public int OrientationSize;
        public int TranslationSize;
        public int ScaleSize;

        public JmadDataHeader(JmadDataType type, int orientationCount, int translationCount, int scaleCount)
        {
            this.Type = type;
            this.OrientationCount = orientationCount;
            this.TranslationCount = translationCount;
            this.ScaleCount = scaleCount;

            this.OrientationOffset = -1;
            this.TranslationOffset = -1;
            this.ScaleOffset = -1;
            this.OrientationSize = -1;
            this.TranslationSize = -1;
            this.ScaleSize = -1;

            this.UnknownOrientationItemOffset = -1;
            this.UnknownTranslationItemOffset = -1;
            this.UnknownScaleItemOffset = -1;
            this.OrientationFrameMapping = -1;
            this.TranslationFrameMapping = -1;
            this.ScaleFrameMapping = -1;
            this.ShortsForFrameMapping = false;
        }

        public void SetComponentOffsets(int orientation, int translation, int scale)
        {
            this.OrientationOffset = orientation;
            this.TranslationOffset = translation;
            this.ScaleOffset = scale;
        }

        public void SetComponentSizes(int orientation, int translation, int scale)
        {
            this.OrientationSize = orientation;
            this.TranslationSize = translation;
            this.ScaleSize = scale;
        }

        public void SetFrameMappingOffsets(int orientation, int translation, int scale, bool usesShorts = false)
        {
            this.OrientationFrameMapping = orientation;
            this.TranslationFrameMapping = translation;
            this.ScaleFrameMapping = scale;
            this.ShortsForFrameMapping = usesShorts;
        }

        public void SetUnknownComponentOffsets(int orientation, int translation, int scale)
        {
            this.UnknownOrientationItemOffset = orientation;
            this.UnknownTranslationItemOffset = translation;
            this.UnknownScaleItemOffset = scale;
        }

        public int TotalLength()
        {
            return ScaleOffset + ScaleCount * sizeof(float);
        }
    }
}

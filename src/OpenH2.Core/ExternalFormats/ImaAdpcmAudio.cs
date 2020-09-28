using System;

namespace OpenH2.Core.ExternalFormats
{
    public static class ImaAdpcmAudio
    {
        public static short[] Decode(bool stereo, byte[] source)
        {
            try
            {
                return DecodeImplementation(stereo, source);
            }
            catch
            {
                return Array.Empty<short>();
            }
        }

        private static short[] DecodeImplementation(bool stereo, byte[] source)
        {
            var blocks = source.Length / (stereo ? 72 : 36);
            var samples = blocks * 65 * (stereo ? 2 : 1);

            var dest = new short[samples];

            var destIndex = 0;
            var blockIndex = 0;

            bool lastSampleLeft = true;

            int leftSample = 0;
            int leftIndex = 0;
            short leftStepsize = ima_step_table[0];

            int rightSample = 0;
            int rightIndex = 0;
            short rightStepsize = ima_step_table[0];

            for (var i = 0; i < source.Length; i++)
            {
                if (i % (stereo ? 72 : 36) == 0)
                {
                    leftSample = BitConverter.ToInt16(source, i);
                    dest[destIndex++] = (short)leftSample;
                    leftIndex = source[i + 2];
                    i += 3;
                    blockIndex = 4;

                    if (stereo)
                    {
                        rightSample = BitConverter.ToInt16(source, i + 1);
                        dest[destIndex++] = (short)rightSample;
                        rightIndex = source[i + 3];
                        i += 4;
                        blockIndex = 8;
                    }
                }
                else
                {
                    var decodingRight = stereo && blockIndex > 7 && (blockIndex & 4) == 4;
                    var b = source[i];
                    var sample1 = nibbleDecode((byte)(b & 0x0F), decodingRight);
                    var sample2 = nibbleDecode((byte)(b >> 4), decodingRight);

                    // TODO: find a cleaner way to interleave the samples
                    if (stereo)
                    {
                        if (decodingRight)
                        {
                            if (lastSampleLeft)
                            {
                                destIndex -= 15;
                            }

                            lastSampleLeft = false;
                        }
                        else
                        {
                            if (lastSampleLeft == false)
                            {
                                destIndex -= 3;
                            }

                            lastSampleLeft = true;
                        }

                        dest[destIndex + 0] = sample1;
                        dest[destIndex + 2] = sample2;

                        destIndex += 4;
                    }
                    else
                    {
                        dest[destIndex + 0] = sample1;
                        dest[destIndex + 1] = sample2;
                        destIndex += 2;
                    }

                    blockIndex++;
                }
            }

            return dest;

            short nibbleDecode(byte originalSample, bool decodingRight)
            {
                ref int sample = ref leftSample;
                ref int idx = ref leftIndex;
                ref short step = ref leftStepsize;

                if (decodingRight)
                {
                    sample = ref rightSample;
                    idx = ref rightIndex;
                    step = ref rightStepsize;
                }

                // compute predicted sample estimate newSample
                int difference = 0;

                if ((originalSample & 4) == 4)
                    difference += step;

                if ((originalSample & 2) == 2)
                    difference += step >> 1;

                if ((originalSample & 1) == 1)
                    difference += step >> 2;

                difference += step >> 3;

                if ((originalSample & 8) == 8)
                    difference = -difference;

                sample += difference;
                sample = Math.Clamp(sample, short.MinValue, short.MaxValue);

                // compute new stepsize
                idx += ima_index_table[originalSample];
                idx = Math.Clamp(idx, 0, 88);

                // find new quantizer stepsize
                step = ima_step_table[idx];

                return (short)sample;
            }
        }

        private static int[] ima_index_table = new[] {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8
        };

        private static short[] ima_step_table = new short[] {
          7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
          19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
          50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
          130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
          337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
          876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
          2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
          5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
          15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };
    }
}

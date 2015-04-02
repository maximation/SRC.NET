﻿/*
** Copyright (C) 2011, 2015 Mario Guggenberger <mg@protyposis.net>
**
** This program is free software; you can redistribute it and/or modify
** it under the terms of the GNU General Public License as published by
** the Free Software Foundation; either version 2 of the License, or
** (at your option) any later version.
**
** This program is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** You should have received a copy of the GNU General Public License
** along with this program; if not, write to the Free Software
** Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307, USA.
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace LibSampleRate {
    public class SampleRateConverter : IDisposable {

        private bool disposed = false;
        private IntPtr srcState = IntPtr.Zero;
        private SRC_DATA srcData;
        private int error;
        private int channels;
        private double ratio;
        private double bufferedSamples;

        public SampleRateConverter(ConverterType type, int channels) {
            srcState = InteropWrapper.src_new(type, channels, out error);
            ThrowExceptionForError(error);
            srcData = new SRC_DATA();

            SetRatio(1d);

            this.channels = channels;
            this.bufferedSamples = 0;
        }

        /// <summary>
        /// Gets the number of bytes buffered by the SRC. Buffering may happen since the SRC may read more
        /// data than it outputs during one #Process call.
        /// </summary>
        public int BufferedBytes {
            get { return (int)(bufferedSamples * 4); }
        }

        public void Reset() {
            error = InteropWrapper.src_reset(srcState);
            ThrowExceptionForError(error);
            bufferedSamples = 0;
        }

        public void SetRatio(double ratio) {
            SetRatio(ratio, true);
        }

        public void SetRatio(double ratio, bool step) {
            if (step) {
                // force the ratio for the next #Process call instead of linearly interpolating from the previous
                // ratio to the current ratio
                error = InteropWrapper.src_set_ratio(srcState, ratio);
                ThrowExceptionForError(error);
            }
            this.ratio = ratio;
        }

        public static bool CheckRatio(double ratio) {
            return InteropWrapper.src_is_valid_ratio(ratio) == 1;
        }

        public void Process(byte[] input, int inputOffset, int inputLength,
            byte[] output, int outputOffset, int outputLength,
            bool endOfInput, out int inputLengthUsed, out int outputLengthGenerated) {
            unsafe {
                fixed (byte* inputBytes = &input[inputOffset], outputBytes = &output[outputOffset]) {
                    Process((float*)inputBytes, inputLength / 4, (float*)outputBytes, outputLength / 4, endOfInput,
                        out inputLengthUsed, out outputLengthGenerated);
                    inputLengthUsed *= 4;
                    outputLengthGenerated *= 4;
                }
            }
        }

        public void Process(float[] input, int inputOffset, int inputLength,
            float[] output, int outputOffset, int outputLength,
            bool endOfInput, out int inputLengthUsed, out int outputLengthGenerated) {
            unsafe {
                fixed (float* inputFloats = &input[inputOffset], outputFloats = &output[outputOffset]) {
                    Process(inputFloats, inputLength, outputFloats, outputLength, endOfInput, 
                        out inputLengthUsed, out outputLengthGenerated);
                }
            }
        }

        private unsafe void Process(float* input, int inputLength, float* output, int outputLength,
            bool endOfInput, out int inputLengthUsed, out int outputLengthGenerated) {
            srcData.data_in = input;
            srcData.data_out = output;
            srcData.end_of_input = endOfInput ? 1 : 0;
            srcData.input_frames = inputLength / channels;
            srcData.output_frames = outputLength / channels;
            srcData.src_ratio = ratio;

            error = InteropWrapper.src_process(srcState, ref srcData);
            ThrowExceptionForError(error);

            inputLengthUsed = srcData.input_frames_used * channels;
            outputLengthGenerated = srcData.output_frames_gen * channels;

            bufferedSamples += inputLengthUsed - (outputLengthGenerated / ratio);
        }

        private void ThrowExceptionForError(int error) {
            if (error != 0) {
                throw new Exception(InteropWrapper.src_strerror(error));
            }
        }

        public void Dispose() {
            if (srcState != IntPtr.Zero) {
                srcState = InteropWrapper.src_delete(srcState);
                if (srcState != IntPtr.Zero) {
                    throw new Exception("could not delete the sample rate converter");
                }
            }
        }

        ~SampleRateConverter() {
            Dispose();
        }
    }
}

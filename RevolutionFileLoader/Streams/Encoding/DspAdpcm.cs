using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGAudio.Codecs.GcAdpcm;

namespace RevolutionFileLoader {

    /// <summary>
    /// DspAdpcm Encoding.
    /// </summary>
    public class DspAdpcm : AudioEncoding {

        /// <summary>
        /// Sample data.
        /// </summary>
        private byte[] samples;

        /// <summary>
        /// PCM16 samples for retrieving SEEK info.
        /// </summary>
        private short[] pcmSamples;

        /// <summary>
        /// DspAdpcm info.
        /// </summary>
        public DspAdpcmInfo DspAdpcmInfo;

        /// <summary>
        /// Loop start.
        /// </summary>
        /// <returns>Loop start info.</returns>
        public uint LoopStart;

        /// <summary>
        /// Get the PCM16 samples.
        /// </summary>
        /// <returns>The PCM16 samples.</returns>
        public override short[] ToPCM16() {
            short[] pcm16 = new short[NumSamples];
            DspAdpcmDecoder.Decode(samples, ref pcm16, ref DspAdpcmInfo, (uint)NumSamples);
            return pcm16;
        }

        /// <summary>
        /// Set the PCM16 samples.
        /// </summary>
        public override void FromPCM16(short[] samples) {
            this.samples = DspAdpcmEncoder.EncodeSamples(samples, out DspAdpcmInfo, LoopStart);
            DataSize = this.samples.Length;
            NumSamples = samples.Length;
        }

        /// <summary>
        /// Read the encoding.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            if (NumSamples == -1) {
                samples = r.ReadBytes(DataSize);
                NumSamples = DataSize;
            } else {
                samples = r.ReadBytes(NumSamples * 8 / 7);
                DataSize = NumSamples * 8 / 7;
            }
        }

        /// <summary>
        /// Write the encoding.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            w.Write(samples);
        }

        /// <summary>
        /// Init samples from a number of blocks.
        /// </summary>
        /// <param name="blockCount">The number of blocks.</param>
        /// <param name="blockSize">Size of each block.</param>
        /// <param name="blockSamples">Samples per block.</param>
        /// <param name="lastBlockSize">The size of the last block.</param>
        /// <param name="lastBlockSamples">Samples in the last block.</param>
        public override void InitFromBlocks(uint blockCount, uint blockSize, uint blockSamples, uint lastBlockSize, uint lastBlockSamples) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the number of blocks.
        /// </summary>
        /// <param name="blockSize">Size of each block.</param>
        /// <param name="blockSamples">Samples per block.</param>
        /// <returns>The number of blocks.</returns>
        public override uint NumBlocks(uint blockSize, uint blockSamples) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the size of the last block.
        /// </summary>
        /// <param name="blockSize">Size of each block.</param>
        /// <param name="blockSamples">Samples per block.</param>
        /// <returns>The size of the last block.</returns>
        public override uint LastBlockSize(uint blockSize, uint blockSamples) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the samples in the last block.
        /// </summary>
        /// <param name="blockSize">Size of each block.</param>
        /// <param name="blockSamples">Samples per block.</param>
        /// <returns>The amount of samples in the last block.</returns>
        public override uint LastBlockSamples(uint blockSize, uint blockSamples) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a block.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="blockNum"></param>
        /// <param name="blockSize"></param>
        /// <param name="blockSamples"></param>
        public override void ReadBlock(FileReader r, int blockNum, uint blockSize, uint blockSamples) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write a block.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="blockNum">The number of blocks.</param>
        /// <param name="blockSize">Size of each block.</param>
        /// <param name="blockSamples">Samples per block.</param>
        public override void WriteBlock(FileWriter w, int blockNum, uint blockSize, uint blockSamples) {
            throw new NotImplementedException();
        }

    }

    public static class DspAdpcmDecoder {

        static sbyte[] NibbleToSbyte = { 0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1 };

        static uint DivideByRoundUp(uint dividend, uint divisor) {
            return (dividend + divisor - 1) / divisor;
        }

        static sbyte GetHighNibble(byte value) {
            return NibbleToSbyte[(value >> 4) & 0xF];
        }

        static sbyte GetLowNibble(byte value) {
            return NibbleToSbyte[value & 0xF];
        }

        static short Clamp16(int value) {
            if (value > 32767) {
                return 32767;
            }
            if (value < -32678) {
                return -32678;
            }
            return (short)value;
        }


        /// <summary>
        /// Decode DSP-ADPCM data.
        /// </summary>
        /// <param name="src">DSP-ADPCM source.</param>
        /// <param name="dst">Destination array of samples.</param>
        /// <param name="cxt">DSP-APCM context.</param>
        /// <param name="samples">Number of samples.</param>
        public static void Decode(byte[] src, ref Int16[] dst, ref DspAdpcmInfo cxt, UInt32 samples) {

            //Each DSP-APCM frame is 8 bytes long. It contains 1 header byte, and 7 sample bytes.

            //Set initial values.
            short hist1 = cxt.yn1;
            short hist2 = cxt.yn2;
            int dstIndex = 0;
            int srcIndex = 0;

            //Until all samples decoded.
            while (dstIndex < samples) {

                //Get the header.
                byte header = src[srcIndex++];

                //Get scale and co-efficient index.
                UInt16 scale = (UInt16)(1 << (header & 0xF));
                byte coef_index = (byte)(header >> 4);
                short coef1 = cxt.coefs[coef_index][0];
                short coef2 = cxt.coefs[coef_index][1];

                //7 sample bytes per frame.
                for (UInt32 b = 0; b < 7; b++) {

                    //Get byte.
                    byte byt = src[srcIndex++];

                    //2 samples per byte.
                    for (UInt32 s = 0; s < 2; s++) {
                        sbyte adpcm_nibble = ((s == 0) ? GetHighNibble(byt) : GetLowNibble(byt));
                        short sample = Clamp16(((adpcm_nibble * scale) << 11) + 1024 + ((coef1 * hist1) + (coef2 * hist2)) >> 11);

                        hist2 = hist1;
                        hist1 = sample;
                        dst[dstIndex++] = sample;

                        if (dstIndex >= samples) break;
                    }
                    if (dstIndex >= samples) break;

                }

            }

        }

    }

    /// <summary>
    /// The encoder.
    /// </summary>
    public class DspAdpcmEncoder {

        /// <summary>
        /// Encodes the samples.
        /// </summary>
        /// <returns>The samples.</returns>
        /// <param name="samples">Samples.</param>
		public static byte[] EncodeSamples(short[] samples, out DspAdpcmInfo info, uint loopStart) {

            //Encode data.
            short[] coeffs = GcAdpcmCoefficients.CalculateCoefficients(samples);
            byte[] dspAdpcm = GcAdpcmEncoder.Encode(samples, coeffs);

            info = new DspAdpcmInfo();
            info.LoadCoeffs(coeffs);

            //Loop stuff.
            if (loopStart > 0) info.loop_yn1 = samples[loopStart - 1];
            if (loopStart > 1) info.loop_yn2 = samples[loopStart - 2];

            return dspAdpcm;

        }

    }

}

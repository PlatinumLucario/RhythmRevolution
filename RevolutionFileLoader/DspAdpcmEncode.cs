using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CitraFileLoader
{

    /// <summary>
    /// The encoder.
    /// </summary>
    public class DspAdpcmEncoder
    {


        /*
       /// <summary>
       /// Correlate coeffiencts.
       /// </summary>
       /// <param name="source">Source of the audio.</param>
       /// <param name="samples">Number of samples.</param>
       /// <param name="coefsOut">The output coefficients.</param>
       public void CorrelateCoefs(short[] source, int samples, ref short[][] coefsOut) {

           int numFrames = (samples + 13) / 14;
           int frameSamples;

           //Block buffer. [2][0x3800]
           short[][] blockBuffer = new short[2][];
           blockBuffer[0] = new short[0x3800];
           blockBuffer[1] = new short[0x3800];

           //[2][14]
           short[][] pcmHistBuffer = new short[2][];
           pcmHistBuffer[0] = new short[14];
           pcmHistBuffer[1] = new short[14];

           short[] vec1 = new short[3];
           short[] vec2 = new short[3];

           short[][] mtx = new short[3][];
           mtx[0] = new short[3];
           mtx[1] = new short[3];
           mtx[2] = new short[3];

           int[] vecIdxs = new int[3];



       }*/


        /// <summary>
        /// Encode a frame.
        /// </summary>
        /// <param name="pcmInOut"></param>
        /// <param name="sampleCount"></param>
        /// <param name="adpcmOut"></param>
        /// <param name="coefsIn"></param>
        public void DSPEncodeFrame(short[] pcmInOut, int sampleCount, ref byte[] adpcmOut, short[] coefsIn) {

            //int[][] inSamples =
            //int[][] outSamples;
        
        }

    }

}

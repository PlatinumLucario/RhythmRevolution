using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// AST stream.
    /// </summary>
    public class AST : StreamedAudioFile {

        public override Type[] SupportedEncodings() => new Type[] { typeof(PCM16), typeof(SignedPCM8) };

        public override void OnConversion() {
            throw new NotImplementedException();
        }

        public override void Read(FileReader r) {

            //Read data.
            r.ReadUInt32();
            uint size = r.ReadUInt32();
            if (r.ReadUInt16() != 1) { throw new Exception("Different type of audio encoding, please send Gota7 this file."); }
            int bitsPerSample = r.ReadUInt16();
            int numChannels = r.ReadUInt16();
            r.ReadUInt16();
            SampleRate = r.ReadUInt32();
            uint numSamples = r.ReadUInt32();
            LoopStart = r.ReadUInt32();
            LoopEnd = r.ReadUInt32();
            uint blockSize1 = r.ReadUInt32();
            r.ReadUInt32s(7);

            //Read block.


        }

        public override void Write(FileWriter w) {
            throw new NotImplementedException();
        }

    }

}

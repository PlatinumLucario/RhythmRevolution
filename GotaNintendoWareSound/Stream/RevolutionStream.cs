using GotaNintendoWare;
using GotaNintendoWare.Wave;
using GotaSoundIO;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWareSound.Stream {
    
    /// <summary>
    /// Revolution stream used for Wii games.
    /// </summary>
    public class RevolutionStream : SoundFile {

        /// <summary>
        /// Supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public override Type[] SupportedEncodings() => new Type[] { typeof(DspAdpcm), typeof(PCM16), typeof(PCM8Signed) };

        /// <summary>
        /// Name.
        /// </summary>
        /// <returns>The name.</returns>
        public override string Name() => "BRSTM";

        /// <summary>
        /// Extensions.
        /// </summary>
        /// <returns>The extensions.</returns>
        public override string[] Extensions() => new string[] { "BRSTM" };

        /// <summary>
        /// Description.
        /// </summary>
        /// <returns>The description.</returns>
        public override string Description() => "Binary Revolution Stream used in Wii games.";

        /// <summary>
        /// If the file supports tracks.
        /// </summary>
        /// <returns>It doesn't.</returns>
        public override bool SupportsTracks() => true;

        /// <summary>
        /// Preferred encoding.
        /// </summary>
        /// <returns>The preferred encoding.</returns>
        public override Type PreferredEncoding() => typeof(DspAdpcm);

        /// <summary>
        /// Main file version.
        /// </summary>
        public readonly RVersion MAIN_VERSION = new RVersion() { Major = 1 };

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read file.
            r.OpenFile<RFileHeader>(out _);

            //Head block.
            r.OpenBlock(0, out _, out _);
            r.OpenReference<RReference<object>>("strmInfo");
            r.OpenReference<RReference<object>>("trackTable");
            r.OpenReference<RReference<object>>("channelTable");
            PcmFormat pcmFormat = PcmFormat.Encoded;
            byte numChannels = 0;
            uint dataOffset = 0;
            uint numBlocks = 0;
            uint blockSize = 0;
            uint blockSamples = 0;
            uint lastBlockSize = 0;
            uint lastBlockSamples = 0;
            uint lastBlockPaddedSize = 0;
            List<DspAdpcmContext> dspData = new List<DspAdpcmContext>();
            if (!r.ReferenceNull("strmInfo")) {
                r.JumpToReference("strmInfo");
                pcmFormat = (PcmFormat)r.ReadByte();
                Loops = r.ReadBoolean();
                numChannels = r.ReadByte();
                SampleRate = r.Read<UInt24>();
                r.ReadUInt16(); //blockHeaderOffset = r.ReadUInt16();
                LoopStart = r.ReadUInt32();
                LoopEnd = r.ReadUInt32();
                dataOffset = r.ReadUInt32();
                numBlocks = r.ReadUInt32();
                blockSize = r.ReadUInt32();
                blockSamples = r.ReadUInt32();
                lastBlockSize = r.ReadUInt32();
                lastBlockSamples = r.ReadUInt32();
                lastBlockPaddedSize = r.ReadUInt32();
                //Version 1.0+
                //u32 adpcmDataInterveral.
                //u32 adpcmDataSize.
            }
            //TODO! TRACK INFO!

            if (!r.ReferenceNull("channelTable")) {
                r.JumpToReference("channelTable");
                byte num = r.ReadByte();
                r.ReadBytes(3);
            }

            //Encoding type.
            Type encodingType = null;
            switch (pcmFormat) {
                case PcmFormat.SignedPCM8:
                    encodingType = typeof(PCM8Signed);
                    break;
                case PcmFormat.PCM16:
                    encodingType = typeof(PCM16);
                    break;
                case PcmFormat.Encoded:
                    encodingType = typeof(DspAdpcm);
                    break;
            }

            //Data block.
            r.Jump(dataOffset, true);
            Audio.Read(r, encodingType, numChannels, numBlocks, blockSize, blockSamples, lastBlockSize, lastBlockSamples, lastBlockPaddedSize - lastBlockSize);

            //Set DSP-ADPCM info and SEEK.
            if (pcmFormat == PcmFormat.Encoded) {

            }

        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
        }

        /// <summary>
        /// Before conversion.
        /// </summary>
        public override void BeforeConversion() {
            if (Audio.BlockSize == -1) {
                Audio.ChangeBlockSize(0x200);
            }
        }

        /// <summary>
        /// After conversion.
        /// </summary>
        public override void AfterConversion() {
            AlignLoopToBlock((uint)Audio.BlockSamples);
            TrimAfterLoopEnd();
        }

    }

}

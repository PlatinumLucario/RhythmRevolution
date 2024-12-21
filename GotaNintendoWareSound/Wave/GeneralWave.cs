using GotaSoundIO;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.Wave {

    /// <summary>
    /// Wave for Wii U, 3ds, and Switch.
    /// </summary>
    public class XWave : SoundFile {

        /// <summary>
        /// If the XWave is the F type.
        /// </summary>
        public bool FType = true;

        /// <summary>
        /// Supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public override Type[] SupportedEncodings() => new Type[] { typeof(DspAdpcm), typeof(PCM16), typeof(PCM8Signed) };

        /// <summary>
        /// Name.
        /// </summary>
        /// <returns>The name.</returns>
        public override string Name() => "BXWAV";

        /// <summary>
        /// Extensions.
        /// </summary>
        /// <returns>The extensions.</returns>
        public override string[] Extensions() => new string[] { "BCWAV", "BFWAV" };

        /// <summary>
        /// Description.
        /// </summary>
        /// <returns>The description.</returns>
        public override string Description() => "Wave used for Wii U, 3ds, and Switch.";

        /// <summary>
        /// If the file supports tracks.
        /// </summary>
        /// <returns>It doesn't.</returns>
        public override bool SupportsTracks() => false;

        /// <summary>
        /// Preferred encoding.
        /// </summary>
        /// <returns>The preferred encoding.</returns>
        public override Type PreferredEncoding() => typeof(DspAdpcm);

        /// <summary>
        /// Minimum version allowed to be saved.
        /// </summary>
        public XVersion MinimumVersion => FType ? new XVersion() { FType = true, Major = 1 } : new XVersion() { FType = false, Major = 2 };

        /// <summary>
        /// Has the original loop start.
        /// </summary>
        public XVersion HasOriginalLoopStart => FType ? new XVersion() { FType = true, Major = 1, Minor = 2 } : new XVersion() { FType = false, Major = 2, Minor = 1, Revision = 1 };

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Header.
            FileHeader header;
            r.OpenFile<XFileHeader>(out header);
            ByteOrder = header.ByteOrder;
            Version = header.Version;
            FType = (header as XFileHeader).FType;

            //INFO block.
            r.OpenBlock(0, out _, out _);

            //Switch type.
            PcmFormat pcmFormat = (PcmFormat)r.ReadByte();
            Type format = null;
            switch (pcmFormat) {
                case PcmFormat.SignedPCM8:
                    format = typeof(PCM8Signed);
                    break;
                case PcmFormat.PCM16:
                    format = typeof(PCM16);
                    break;
                case PcmFormat.Encoded:
                    format = typeof(DspAdpcm);
                    break;
            }
            Loops = r.ReadBoolean();
            r.ReadUInt16();
            SampleRate = r.ReadUInt32();
            LoopStart = r.ReadUInt32();
            LoopEnd = r.ReadUInt32(); //Num samples.
            OriginalLoopStart = r.ReadUInt32();
            if (Version < HasOriginalLoopStart) {
                OriginalLoopStart = LoopStart;
            }

            //Channels.
            var channels = new List<int>();

            //DATA block.
            r.OpenBlock(1, out _, out _);

            //Get size and padding.
            int dataSize = 0;
            int dataPadding = 0;
            while ((dataSize + dataPadding) % 0x20 != 0) {
                dataPadding++;
            }

            //Read data.
            Audio.Read(r, format, channels.Count, dataSize, (int)LoopEnd, dataPadding);

        }

        /// <summary>
        /// Channel data.
        /// </summary>
        /*private class ChannelData : IReadable, IWriteable { 
        


        }

        private class OtherDspApcm { 
            
        }*/

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

        }

    }

}

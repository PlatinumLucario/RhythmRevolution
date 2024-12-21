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
    /// Revolution wave file.
    /// </summary>
    public class RevolutionWave : SoundFile {

        /// <summary>
        /// Supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public override Type[] SupportedEncodings() => new Type[] { typeof(DspAdpcm), typeof(PCM16), typeof(PCM8Signed) };

        /// <summary>
        /// Name.
        /// </summary>
        /// <returns>The name.</returns>
        public override string Name() => "BRWAV";

        /// <summary>
        /// Extensions.
        /// </summary>
        /// <returns>The extensions.</returns>
        public override string[] Extensions() => new string[] { "BRWAV" };

        /// <summary>
        /// Description.
        /// </summary>
        /// <returns>The description.</returns>
        public override string Description() => "Revolution wave used in Wii Sound Archives.";

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
        /// Default version.
        /// </summary>
        public readonly RVersion DefaultVersion = new RVersion() { Major = 1 };

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Open file.
            FileHeader header;
            r.OpenFile<RFileHeader>(out header);
            Version = header.Version;

            //Read data.
            r.OpenBlock(0, out _, out _);
            var info = r.Read<WaveInfo>();
            r.OpenBlock(1, out _, out _);
            var tmp = FromWaveInfo(info, r, (RVersion)Version);
            Version = tmp.Version;
            ByteOrder = tmp.ByteOrder;
            Loops = tmp.Loops;
            LoopStart = tmp.LoopStart;
            LoopEnd = tmp.LoopEnd;
            Audio = tmp.Audio;
            OriginalLoopStart = tmp.OriginalLoopStart;
            OriginalLoopEnd = tmp.OriginalLoopEnd;
            SampleRate = tmp.SampleRate;
            Tracks = tmp.Tracks;

        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Default.
            
        }

        public static RevolutionWave FromWaveInfo(WaveInfo w, FileReader r, RVersion version = null) {

            return null;

        }

        public WaveInfo GenWaveInfo(uint offset) {
            WaveInfo w = new WaveInfo();
            w.Format = PcmFormat.Encoded;
            if (Audio.EncodingType == typeof(PCM16)) { w.Format = PcmFormat.PCM16; }
            if (Audio.EncodingType == typeof(PCM8Signed)) { w.Format = PcmFormat.SignedPCM8; }
            w.Loops = Loops;
            w.SampleRate = (UInt24)SampleRate;
            w.LoopStart = LoopStart;
            w.LoopEnd = LoopEnd;
            w.Channels = new List<WaveInfo.ChannelInfo>();
            foreach (var c in Audio.Channels) {
                if (c.Count > 0) { 
                    
                }
            }
            return null; //TODO!!!
        }

        /// <summary>
        /// Wave info.
        /// </summary>
        public class WaveInfo : IReadable, IWriteable {

            /// <summary>
            /// Format.
            /// </summary>
            public PcmFormat Format;

            /// <summary>
            /// If the wave loops.
            /// </summary>
            public bool Loops;

            /// <summary>
            /// Sample rate.
            /// </summary>
            public UInt24 SampleRate;

            /// <summary>
            /// If the offset for wave data is absolute.
            /// </summary>
            public bool OffsetIsAbsolute;

            /// <summary>
            /// Loop start.
            /// </summary>
            public uint LoopStart;

            /// <summary>
            /// Loop end.
            /// </summary>
            public uint LoopEnd;

            /// <summary>
            /// Channels.
            /// </summary>
            public List<ChannelInfo> Channels = new List<ChannelInfo>();

            /// <summary>
            /// Read the info.
            /// </summary>
            /// <param name="r">The reader.</param>
            public void Read(FileReader r) {



            }

            /// <summary>
            /// Write the info.
            /// </summary>
            /// <param name="w">The writer.</param>
            public void Write(FileWriter w) {

            }

            /// <summary>
            /// Channel info.
            /// </summary>
            public class ChannelInfo : IReadable, IWriteable {

                /// <summary>
                /// Data offset.
                /// </summary>
                public uint DataOff;

                /// <summary>
                /// DSP-ADPCM Context.
                /// </summary>
                public DspAdpcmContext DspAdpcmContext;

                /// <summary>
                /// Front left volume.
                /// </summary>
                public uint VolumeFrontLeft = 0x01000000;

                /// <summary>
                /// Front right volume.
                /// </summary>
                public uint VolumeFrontRight = 0x01000000;

                /// <summary>
                /// Back left volume.
                /// </summary>
                public uint VolumeBackLeft = 0x01000000;

                /// <summary>
                /// Back right volume.
                /// </summary>
                public uint VolumeBackRight = 0x01000000;

                /// <summary>
                /// Read the info.
                /// </summary>
                /// <param name="r">The reader.</param>
                public void Read(FileReader r) {
                    throw new NotImplementedException();
                }

                /// <summary>
                /// Write the info.
                /// </summary>
                /// <param name="w">The writer.</param>
                public void Write(FileWriter w) {
                    throw new NotImplementedException();
                }
            
            }

        }

    }

}

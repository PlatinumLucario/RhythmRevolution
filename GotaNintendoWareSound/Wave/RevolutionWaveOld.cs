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
    /// A revolution wave.
    /// </summary>
    public class RevolutionWaveOld : SoundFile {

        /// <summary>
        /// Create a wave.
        /// </summary>
        public RevolutionWaveOld() { }

        /// <summary>
        /// Create a wave from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public RevolutionWaveOld(string filePath) : base(filePath) {}

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
        public override string Description() => "Revolution wave used in Wii sound archives.";

        /// <summary>
        /// If the file supports tracks.
        /// </summary>
        /// <returns>It doesn't.</returns>
        public override bool SupportsTracks() => false;

        /// <summary>
        /// Get the preferred encoding.
        /// </summary>
        /// <returns>Preferred encoding for the wave.</returns>
        public override Type PreferredEncoding() => typeof(DspAdpcm);

        /// <summary>
        /// Wave block body.
        /// </summary>
        public WaveBlockBody Body;

        /// <summary>
        /// Extra channel info.
        /// </summary>
        public class ExtraInfo {

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

        }

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Open file.
            FileHeader header;
            r.OpenFile<RFileHeader>(out header);
            ByteOrder = header.ByteOrder;
            Version = header.Version;

            //Read info block.
            r.OpenBlock(0, out _, out _);
            Body = r.Read<WaveBlockBody>();
            Body.SetData(this);
            Body.InWaveFile = true;

            //Data block.
            uint size;
            r.OpenBlock(1, out _, out size);
            long bas = 8;
            uint realSize = (uint)(size - bas);
            if (Body.NumChannels > 1) { realSize = (uint)(Body.ChannelOffs[1]); }
            for (int i = 0; i < Body.NumChannels; i++) {
                r.Jump(Body.ChannelOffs[i]);
                //Channels[i].DataSize = (int)realSize;
                //if (Body.PcmFormat == PcmFormat.Encoded) { (Channels[i] as DspAdpcm).LoopStart = LoopStart; }
                //Channels[i].Read(r);
                //Channels[i].NumSamples = (int)LoopEnd;
            }

        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Init file.
            w.InitFile<RFileHeader>("RWAV", ByteOrder.BigEndian, new RVersion() { Major = Version.Major, Minor = Version.Minor }, 2);

            //Info block.
            w.InitBlock("INFO");

            //Get the format.
            Body = new WaveBlockBody(this);
            Body.InWaveFile = true;
            uint currOff = 0;
            //for (int i = 0; i < Channels.Count; i++) {
            //    Body.ChannelOffs.Add(currOff);
            //    currOff += (uint)Channels[i].DataSize;
            //}
            w.Write(Body);
            w.CloseBlock();

            //Init data block.
            w.InitBlock("DATA");

            //Write data.
            //for (int i = 0; i < Channels.Count; i++) {
            //    w.Write(Channels[i]);
            //}

            //Close the block.
            w.Align(0x20, true);
            w.CloseBlock();

            //Close the file.
            w.CloseFile();

        }

        /// <summary>
        /// A wave block body.
        /// </summary>
        public class WaveBlockBody : IReadable, IWriteable {

            /// <summary>
            /// PCM format.
            /// </summary>
            public PcmFormat PcmFormat;

            /// <summary>
            /// Loops.
            /// </summary>
            public bool Loops;

            /// <summary>
            /// Number of channels.
            /// </summary>
            public byte NumChannels;

            /// <summary>
            /// Sample rate.
            /// </summary>
            public uint SampleRate;

            /// <summary>
            /// Looop start.
            /// </summary>
            public uint LoopStart;

            /// <summary>
            /// Loop end.
            /// </summary>
            public uint LoopEnd;

            /// <summary>
            /// Data offset.
            /// </summary>
            public uint DataOffset;

            /// <summary>
            /// Channel extension.
            /// </summary>
            public List<ExtraInfo> ChannelExtension = new List<ExtraInfo>();

            /// <summary>
            /// DSP-ADPCM context.
            /// </summary>
            public List<DspAdpcmContext> DspAdpcmContext = new List<DspAdpcmContext>();

            /// <summary>
            /// Channel offsets.
            /// </summary>
            public List<uint> ChannelOffs = new List<uint>();

            /// <summary>
            /// If in wave file.
            /// </summary>
            public bool InWaveFile;

            /// <summary>
            /// Wave block body.
            /// </summary>
            public WaveBlockBody() { }

            /// <summary>
            /// Create a wave block file from a sound file.
            /// </summary>
            /// <param name="s">Sound file.</param>
            public WaveBlockBody(SoundFile s) {
                PcmFormat format = PcmFormat.Encoded;
                //if (s.Channels[0] as SignedPCM8 != null) { format = PcmFormat.SignedPCM8; }
                //if (s.Channels[0] as PCM16 != null) { format = PcmFormat.PCM16; }
                PcmFormat = format;
                Loops = s.Loops;
                //NumChannels = (byte)s.Channels.Count;
                SampleRate = s.SampleRate;
                LoopStart = s.LoopStart;
                LoopEnd = s.LoopEnd;
                var w = s as RevolutionWave;
                if (w != null) {
                //    ChannelExtension = w.ChannelExtension;
                }
                //if (format == PcmFormat.DspAdpcm) {
                //    DspAdpcmContext = w.Channels.Select(x => (x as DspAdpcm).Context).ToList();
                //}
            }

            /// <summary>
            /// Set sound data.
            /// </summary>
            /// <param name="s">The sound file.</param>
            public void SetData(SoundFile s) {
                s.Loops = Loops;
                s.SampleRate = SampleRate;
                s.LoopStart = LoopStart;
                s.LoopEnd = LoopEnd;
                var w = s as RevolutionWave;
                if (w != null) {
                //    w.ChannelExtension = ChannelExtension;
                }
                //if (s.Channels[0] as DspAdpcm != null && PcmFormat == PcmFormat.DspAdpcm) {
                //    for (int i = 0; i < s.Channels.Count; i++) {
                //        (s.Channels[i] as DspAdpcm).Context = DspAdpcmContext[i];
                //    }
                //}
            }

            /// <summary>
            /// Read the wave block.
            /// </summary>
            /// <param name="r">The reader.</param>
            public void Read(FileReader r) {

                //Read info.
                r.StartStructure();
                PcmFormat = (PcmFormat)r.ReadByte();
                Loops = r.ReadBoolean();
                NumChannels = r.ReadByte();
                SampleRate = (uint)((r.ReadByte() << 24) | r.ReadUInt16());
                bool dataIsAddress = r.ReadBoolean();
                r.ReadByte();
                //LoopStart = PcmFormat == PcmFormat.DspAdpcm ? DspAdpcm.Nibbles2Samples(r.ReadUInt32()) : r.ReadUInt32();
                //LoopEnd = PcmFormat == PcmFormat.DspAdpcm ? DspAdpcm.Nibbles2Samples(r.ReadUInt32()) : r.ReadUInt32();
                r.OpenOffset("channelInfoTable");
                DataOffset = r.ReadUInt32();
                r.ReadUInt32();

                //Read wave info.
                r.JumpToOffset("channelInfoTable");
                for (int i = 0; i < NumChannels; i++) {
                    r.OpenOffset("channelInfo" + i);
                }

                //Channel info.
                ChannelExtension = new List<ExtraInfo>();
                DspAdpcmContext = new List<DspAdpcmContext>();
                for (int i = 0; i < NumChannels; i++) {

                    //Read channel info.
                    ChannelExtension.Add(new ExtraInfo());
                    r.JumpToOffset("channelInfo" + i);
                    ChannelOffs.Add(r.ReadUInt32());
                    r.OpenOffset("adpcmData");
                    ChannelExtension[i].VolumeFrontLeft = r.ReadUInt32();
                    ChannelExtension[i].VolumeFrontRight = r.ReadUInt32();
                    ChannelExtension[i].VolumeBackLeft = r.ReadUInt32();
                    ChannelExtension[i].VolumeBackRight = r.ReadUInt32();
                    r.ReadUInt32();

                    //Read DSPADPCM info.
                    //if (PcmFormat == PcmFormat.DspAdpcm) {
                    //    r.JumpToOffset("adpcmData", false);
                    //    DspAdpcmContext.Add(r.Read<DspAdpcmContext>());
                    //}
                    r.CloseOffset("adpcmData");

                }
                r.EndStructure();

            }

            /// <summary>
            /// Write the wave block.
            /// </summary>
            /// <param name="w">The writer.</param>
            public void Write(FileWriter w) {

                //Get the format.
                w.StartStructure();
                PcmFormat format = PcmFormat;
                w.Write((byte)format);

                //Other info.
                w.Write(Loops);
                w.Write(NumChannels);
                w.Write((byte)((SampleRate & 0xFF0000) >> 16));
                w.Write((ushort)(SampleRate & 0xFFFF));
                w.Write((ushort)0);
                //if (PcmFormat == PcmFormat.DspAdpcm) { w.Write(DspAdpcm.Samples2Nibbles(LoopStart)); } else { w.Write(LoopStart); }
                //if (PcmFormat == PcmFormat.DspAdpcm) { w.Write(DspAdpcm.Samples2Nibbles(LoopEnd)); } else { w.Write(LoopEnd); }
                w.InitOffset("channelInfoTable");
                if (InWaveFile) { w.InitOffset("dataLocation"); } else { w.Write(DataOffset); }
                w.Write((uint)0);

                //Init channel table.
                w.CloseOffset("channelInfoTable");
                for (int i = 0; i < NumChannels; i++) {
                    w.InitOffset("channelInfo" + i);
                }

                //Write channel data.
                for (int i = 0; i < NumChannels; i++) {
                    w.CloseOffset("channelInfo" + i);
                    w.Write(ChannelOffs[i]);
                    w.InitOffset("adpcmData" + i);
                    w.Write(ChannelExtension[i].VolumeFrontLeft);
                    w.Write(ChannelExtension[i].VolumeFrontRight);
                    w.Write(ChannelExtension[i].VolumeBackLeft);
                    w.Write(ChannelExtension[i].VolumeBackRight);
                    w.Write((uint)0);
                }

                //Write ADPCM data.
                for (int i = 0; i < NumChannels; i++) {
                    //if (format == PcmFormat.DspAdpcm) {
                    //    w.CloseOffset("adpcmData" + i);
                    //    w.Write(DspAdpcmContext[i]);
                    //    w.Align(0x4);
                    //} else {
                    //    w.CloseOffset("adpcmData" + i, false, 0);
                    //}
                }

                //Write offset.
                if (InWaveFile) { w.Align(0x20); w.CloseOffset("dataLocation"); }
                w.EndStructure();

            }

        }

    }

}

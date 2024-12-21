using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Revolution wave.
    /// </summary>
    public class RevolutionWave : StreamedAudioFile {

        /// <summary>
        /// Get the supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public override Type[] SupportedEncodings() => new Type[] { typeof(DspAdpcm), typeof(PCM16), typeof(SignedPCM8) };

        /// <summary>
        /// Extra info.
        /// </summary>
        public List<ChannelExtraInfo> ExtraInfo = new List<ChannelExtraInfo>();

        /// <summary>
        /// Extra channel info.
        /// </summary>
        public class ChannelExtraInfo {

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
        /// Blank constructor.
        /// </summary>
        public RevolutionWave() {}

        /// <summary>
        /// Read a revolution wave from a file path.
        /// </summary>
        /// <param name="filePath">The path to read from.</param>
        public RevolutionWave(string filePath) : base(filePath) {}

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Init file.
            r.OpenFile(out _, out ByteOrder, out Version, out _, out _, out _);

            //Read info block.
            r.OpenBlock(0, out _, out _);
            PcmFormat format = (PcmFormat)r.ReadByte();
            Loops = r.ReadBoolean();
            byte numChannels = r.ReadByte();
            SampleRate = (uint)((r.ReadByte() << 24) | r.ReadUInt16());
            bool dataIsAddress = r.ReadBoolean();
            r.ReadByte();
            LoopStart = r.ReadUInt32();
            LoopEnd = r.ReadUInt32();
            r.OpenOffset("channelInfoTable");
            r.OpenOffset("dataLocation");
            r.CloseOffset("dataLocation");
            r.ReadUInt32();

            //Init channels.
            ExtraInfo = new List<ChannelExtraInfo>();
            switch (format) {
                case PcmFormat.SignedPCM8:
                    for (int i = 0; i < numChannels; i++) {
                        Channels.Add(new SignedPCM8());
                    }
                    break;
                case PcmFormat.PCM16:
                    for (int i = 0; i < numChannels; i++) {
                        Channels.Add(new PCM16());
                    }
                    break;
                case PcmFormat.DspAdpcm:
                    for (int i = 0; i < numChannels; i++) {
                        Channels.Add(new DspAdpcm());
                    }
                    break;
            }

            //Read wave info.
            r.JumpToOffset("channelInfoTable");
            for (int i = 0; i < numChannels; i++) {
                r.OpenOffset("channelInfo" + i);
            }

            //Channel info.
            for (int i = 0; i < numChannels; i++) {
                ExtraInfo.Add(new ChannelExtraInfo());
                r.JumpToOffset("channelInfo" + i);
                r.OpenOffset("channelData" + i);
                r.OpenOffset("adpcmData");
                ExtraInfo[i].VolumeFrontLeft = r.ReadUInt32();
                ExtraInfo[i].VolumeFrontRight = r.ReadUInt32();
                ExtraInfo[i].VolumeBackLeft = r.ReadUInt32();
                ExtraInfo[i].VolumeBackRight = r.ReadUInt32();
                r.ReadUInt32();

                //Read DSPADPCM info.
                if (format == PcmFormat.DspAdpcm) {
                    r.JumpToOffset("adpcmData", false);
                    (Channels[i] as DspAdpcm).DspAdpcmInfo = r.Read<DspAdpcmInfo>();
                }
                r.CloseOffset("adpcmData");

            }

            //Data block.
            uint size;
            r.OpenBlock(1, out _, out size);
            size -= 8;
            size -= r.Offsets["channelData0"];
            for (int i = 0; i < numChannels; i++) {
                r.JumpToOffset("channelData" + i);
                int s = (int)(size - r.Position);
                if (i != numChannels - 1) { s = (int)(r.Offsets["channelData" + (i + 1)]); }
                Channels[i].DataSize = s;
                if (format == PcmFormat.DspAdpcm) { (Channels[i] as DspAdpcm).LoopStart = LoopStart; }
                Channels[i].Read(r);
                if (format == PcmFormat.DspAdpcm) { Channels[i].NumSamples = (int)LoopEnd; }
            }

        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            throw new NotImplementedException();
        }

    }

}

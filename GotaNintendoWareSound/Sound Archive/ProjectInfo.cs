using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// A file item.
    /// </summary>
    public class ProjectInfo : IPlatformChangeable {

        /// <summary>
        /// Max sequences.
        /// </summary>
        public ushort MaxSequences = 0x40;

        /// <summary>
        /// Max sequence tracks.
        /// </summary>
        public ushort MaxSequenceTracks = 0x40;

        /// <summary>
        /// Max streams.
        /// </summary>
        public ushort MaxStreams = 0x04;

        /// <summary>
        /// Max stream tracks.
        /// </summary>
        public ushort MaxStreamTracks = 0x04;

        /// <summary>
        /// Max stream channels.
        /// </summary>
        public ushort MaxStreamChannels = 0x08;

        /// <summary>
        /// Max waves.
        /// </summary>
        public ushort MaxWaves = 0x40;

        /// <summary>
        /// Max wave tracks.
        /// </summary>
        public ushort MaxWaveTracks = 0x40;

        /// <summary>
        /// Stream buffer times.
        /// </summary>
        public byte StreamBufferTimes;

        /// <summary>
        /// Developer flags.
        /// </summary>
        public byte DeveloperFlags;

        /// <summary>
        /// Options.
        /// </summary>
        public uint Options;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                //This doesn't exist.
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                MaxSequences = r.ReadUInt16();
                MaxSequenceTracks = r.ReadUInt16();
                MaxStreams = r.ReadUInt16();
                MaxStreamTracks = r.ReadUInt16();
                MaxStreamChannels = r.ReadUInt16();
                MaxWaves = r.ReadUInt16();
                MaxWaveTracks = r.ReadUInt16();
                r.ReadBytes(6);
            }

            //CTR, Cafe, NX.
            else {
                MaxSequences = r.ReadUInt16();
                MaxSequenceTracks = r.ReadUInt16();
                MaxStreams = r.ReadUInt16();
                MaxStreamTracks = r.ReadUInt16();
                MaxStreamChannels = r.ReadUInt16();
                MaxWaves = r.ReadUInt16();
                MaxWaveTracks = r.ReadUInt16();
                StreamBufferTimes = r.ReadByte();
                if (platform == Platform.NX) { DeveloperFlags = r.ReadByte(); } else { r.ReadByte(); }
                Options = r.ReadUInt32();
            }

        }

        /// <summary>
        /// Write the platform.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="platform">Platform.</param>
        public void Write(FileWriter w, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                //This doesn't exist.
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                w.Write(MaxSequences);
                w.Write(MaxSequenceTracks);
                w.Write(MaxStreams);
                w.Write(MaxStreamTracks);
                w.Write(MaxStreamChannels);
                w.Write(MaxWaves);
                w.Write(MaxWaveTracks);
                w.Write(new byte[6]);
            }

            //CTR, Cafe, NX.
            else {
                w.Write(MaxSequences);
                w.Write(MaxSequenceTracks);
                w.Write(MaxStreams);
                w.Write(MaxStreamTracks);
                w.Write(MaxStreamChannels);
                w.Write(MaxWaves);
                w.Write(MaxWaveTracks);
                w.Write(StreamBufferTimes);
                if (platform == Platform.NX) { w.Write(DeveloperFlags); } else { w.Write((byte)0); }
                w.Write(Options);
            }

        }

    }

}

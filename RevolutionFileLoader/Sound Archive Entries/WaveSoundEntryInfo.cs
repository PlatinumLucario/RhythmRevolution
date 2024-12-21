using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// Wave sound entry info.
    /// </summary>
    public class WaveSoundEntryInfo : SoundInfo {

        /// <summary>
        /// Sound file.
        /// </summary>
        public FileData<WaveSoundData> SoundFile;

        /// <summary>
        /// Wave sound entry.
        /// </summary>
        public WaveSoundEntry WaveSoundEntry;

        /// <summary>
        /// Sub number.
        /// </summary>
        public int SubNumber;

        /// <summary>
        /// Track allocation flags.
        /// </summary>
        public uint TrackAllocation = 1;

        /// <summary>
        /// Channel priority.
        /// </summary>
        public byte ChannelPriority = 64;

        /// <summary>
        /// Release priority fix.
        /// </summary>
        public bool ReleasePriorityFix;

        /// <summary>
        /// Priority fix supported.
        /// </summary>
        public bool SupportsPriorityFix = true;

        /// <summary>
        /// The sound type for the wave sound info.
        /// </summary>
        /// <returns>The wave sound type.</returns>
        public override byte SoundType() => 3;

        /// <summary>
        /// Read the sound info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            ReadSoundInfo(r);
            SubNumber = r.ReadInt32();
            TrackAllocation = r.ReadUInt32();
            ChannelPriority = r.ReadByte();
            ReleasePriorityFix = r.ReadBoolean();
            r.ReadUInt16();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the sound info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            WriteSoundInfo(w);
            w.Write(SubNumber);
            w.Write(TrackAllocation);
            w.Write(ChannelPriority);
            w.Write(SupportsPriorityFix ? ReleasePriorityFix : false);
            w.Write((ushort)0);
            w.Write((uint)0);
            WriteSoundInfo3D(w);
        }

    }

}

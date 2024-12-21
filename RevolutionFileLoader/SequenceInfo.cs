using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// Sequence info.
    /// </summary>
    public class SequenceInfo : SoundInfo {

        /// <summary>
        /// Data offset.
        /// </summary>
        public uint DataOffset;

        /// <summary>
        /// Bank.
        /// </summary>
        public BankInfo Bank;

        /// <summary>
        /// Reading bank Id.
        /// </summary>
        public Id ReadingBankId;

        /// <summary>
        /// Track allocation.
        /// </summary>
        public uint TrackAllocation;

        /// <summary>
        /// Channel priority.
        /// </summary>
        public byte ChannelPriority = 64;

        /// <summary>
        /// Release priority fix.
        /// </summary>
        public byte ReleasePriorityFix;

        /// <summary>
        /// Get the sound type.
        /// </summary>
        /// <returns>The sequence sound type.</returns>
        public override byte SoundType() => 1;

        /// <summary>
        /// Read the entry.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            ReadSoundInfo(r);
            DataOffset = r.ReadUInt32();
            ReadingBankId = r.Read<Id>();
            TrackAllocation = r.ReadUInt32();
            ChannelPriority = r.ReadByte();
            ReleasePriorityFix = r.ReadByte();
            r.ReadUInt16();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

        }

    }

}

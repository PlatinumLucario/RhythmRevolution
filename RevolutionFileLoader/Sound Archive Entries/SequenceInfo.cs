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
        /// Sound file.
        /// </summary>
        public FileData<Sequence> SoundFile;

        /// <summary>
        /// Sequence label.
        /// </summary>
        public string SequenceLabel;

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
        public bool ReleasePriorityFix;

        /// <summary>
        /// Priority fix supported.
        /// </summary>
        public bool SupportsPriorityFix = true;

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
            ReleasePriorityFix = r.ReadBoolean();
            r.ReadUInt16();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            WriteSoundInfo(w);
            w.Write(DataOffset);
            w.Write(ReadingBankId);
            w.Write(TrackAllocation);
            w.Write(ChannelPriority);
            w.Write(SupportsPriorityFix ? ReleasePriorityFix : false);
            w.Write((ushort)0);
            w.Write((uint)0);
            WriteSoundInfo3D(w);
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// Stream info.
    /// </summary>
    public class StreamInfo : SoundInfo {

        /// <summary>
        /// Sound file.
        /// </summary>
        public FileData<RevolutionStream> SoundFile;

        /// <summary>
        /// Start position.
        /// </summary>
        public uint StartPosition;

        /// <summary>
        /// Bitflag before version 1.4.
        /// </summary>
        public ushort ChannelAllocationCount;

        /// <summary>
        /// Track allocation flags.
        /// </summary>
        public ushort TrackAllocationFlags;

        /// <summary>
        /// If channel count is supported.
        /// </summary>
        public bool SupportsChannelCount = false;

        /// <summary>
        /// Get the sound type.
        /// </summary>
        /// <returns>The sound type.</returns>
        public override byte SoundType() => 2;

        /// <summary>
        /// Read the stream info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            ReadSoundInfo(r);
            StartPosition = r.ReadUInt32();
            ChannelAllocationCount = r.ReadUInt16();
            TrackAllocationFlags = r.ReadUInt16();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the sound info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            WriteSoundInfo(w);
            w.Write(StartPosition);
            if (SupportsChannelCount) {
                ushort count = 0;
                for (int i = 0; i < 16; i++) {
                    if ((ChannelAllocationCount & (0b1 << i)) > 0) {
                        count++;
                    }
                }
                w.Write(count);
            } else {
                w.Write(ChannelAllocationCount);
            }
            w.Write(TrackAllocationFlags);
            w.Write((uint)0);
            WriteSoundInfo3D(w);
        }

    }

}

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
            throw new NotImplementedException();
        }

    }

}

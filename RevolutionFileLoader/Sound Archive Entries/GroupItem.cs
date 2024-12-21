using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// A group item.
    /// </summary>
    public class GroupItem : IReadable, IWriteable {

        /// <summary>
        /// File Id when reading.
        /// </summary>
        public Id ReadingFileId;

        /// <summary>
        /// Offset to the file.
        /// </summary>
        public uint Offset;

        /// <summary>
        /// Size of the file.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Offset to the wave data.
        /// </summary>
        public uint WaveDataOffset;

        /// <summary>
        /// Wave data size.
        /// </summary>
        public uint WaveDataSize;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            ReadingFileId = r.Read<Id>();
            Offset = r.ReadUInt32();
            Size = r.ReadUInt32();
            WaveDataOffset = r.ReadUInt32();
            WaveDataSize = r.ReadUInt32();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the item.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(ReadingFileId);
            w.Write(Offset);
            w.Write(Size);
            w.Write(WaveDataOffset);
            w.Write(WaveDataSize);
            w.Write((uint)0);
        }

    }

}

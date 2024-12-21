using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Player info.
    /// </summary>
    public class PlayerInfo : IReadable, IWriteable {

        /// <summary>
        /// Name of the player.
        /// </summary>
        public string Name;

        /// <summary>
        /// Number of playable sounds.
        /// </summary>
        public byte PlayableSoundCount;

        /// <summary>
        /// Heap size.
        /// </summary>
        public uint HeapSize;

        /// <summary>
        /// String id when reading or writing.
        /// </summary>
        public Id ReadingStringId;

        /// <summary>
        /// Read the player info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            ReadingStringId = r.Read<Id>();
            PlayableSoundCount = r.ReadByte();
            r.ReadBytes(3);
            HeapSize = r.ReadUInt32();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the player info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(ReadingStringId);
            w.Write(PlayableSoundCount);
            w.Write(new byte[3]);
            w.Write(HeapSize);
            w.Write((uint)0);
        }

    }

}

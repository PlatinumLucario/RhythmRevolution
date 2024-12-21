using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {
    
    /// <summary>
    /// Note event.
    /// </summary>
    public class NoteEvent : IReadable, IWriteable {

        /// <summary>
        /// Note position.
        /// </summary>
        public float Position;

        /// <summary>
        /// Note length.
        /// </summary>
        public float Length;

        /// <summary>
        /// Note index.
        /// </summary>
        public uint NoteIndex;

        /// <summary>
        /// Read the info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            Position = r.ReadSingle();
            Length = r.ReadSingle();
            NoteIndex = r.ReadUInt32();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(Position);
            w.Write(Length);
            w.Write(NoteIndex);
            w.Write((uint)0);
        }

    }

}

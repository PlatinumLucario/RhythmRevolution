using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Note info.
    /// </summary>
    public class NoteInfo : IReadable, IWriteable {

        /// <summary>
        /// Wave index.
        /// </summary>
        public int WaveIndex;

        /// <summary>
        /// Attack.
        /// </summary>
        public byte Attack = 127;

        /// <summary>
        /// Decay.
        /// </summary>
        public byte Decay = 127;

        /// <summary>
        /// Sustain.
        /// </summary>
        public byte Sustain = 127;

        /// <summary>
        /// Release.
        /// </summary>
        public byte Release = 127;

        /// <summary>
        /// Hold.
        /// </summary>
        public byte Hold;

        /// <summary>
        /// Original key.
        /// </summary>
        public byte OriginalKey = 60;

        /// <summary>
        /// Volume.
        /// </summary>
        public byte Volume = 127;

        /// <summary>
        /// Pan.
        /// </summary>
        public byte Pan = 64;

        /// <summary>
        /// Surround pan.
        /// </summary>
        public byte SurroundPan;

        /// <summary>
        /// Pitch.
        /// </summary>
        public float Pitch = 1;

        /// <summary>
        /// Read the info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            WaveIndex = r.ReadInt32();
            Attack = r.ReadByte();
            Decay = r.ReadByte();
            Sustain = r.ReadByte();
            Release = r.ReadByte();
            Hold = r.ReadByte();
            r.ReadBytes(3);
            OriginalKey = r.ReadByte();
            Volume = r.ReadByte();
            Pan = r.ReadByte();
            SurroundPan = r.ReadByte();
            Pitch = r.ReadSingle();
            r.ReadBytes(28);
        }

        /// <summary>
        /// Write the info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(WaveIndex);
            w.Write(Attack);
            w.Write(Decay);
            w.Write(Sustain);
            w.Write(Release);
            w.Write(Hold);
            w.Write(new byte[3]);
            w.Write(OriginalKey);
            w.Write(Volume);
            w.Write(Pan);
            w.Write(SurroundPan);
            w.Write(Pitch);
            w.Write(new byte[28]);
        }

    }

}

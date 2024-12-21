using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Sound 3d info.
    /// </summary>
    public class Sound3dInfo : IReadable, IWritable {

        /// <summary>
        /// Flags.
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Decay curve type.
        /// </summary>
        public byte DecayCurve;

        /// <summary>
        /// Decay ratio.
        /// </summary>
        public byte DecayRatio;

        /// <summary>
        /// Doppler factor.
        /// </summary>
        public byte DopplerFactor;

        /// <summary>
        /// Read the 3d info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            Flags = r.ReadUInt32();
            DecayCurve = r.ReadByte();
            DecayRatio = r.ReadByte();
            DopplerFactor = r.ReadByte();
            r.ReadByte();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the 3d info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(Flags);
            w.Write(DecayCurve);
            w.Write(DecayRatio);
            w.Write(DopplerFactor);
            w.Write((byte)0);
            w.Write((uint)0);
        }

    }

}

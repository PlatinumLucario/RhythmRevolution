using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {
    
    /// <summary>
    /// Wave sound info.
    /// </summary>
    public class WaveSoundInfo : IReadable, IWriteable {

        /// <summary>
        /// Pitch.
        /// </summary>
        public float Pitch = 1f;

        /// <summary>
        /// Pan.
        /// </summary>
        public byte Pan = 64;

        /// <summary>
        /// Surround pan.
        /// </summary>
        public byte SurroundPan;

        /// <summary>
        /// Fx send A.
        /// </summary>
        public byte FxSendA;

        /// <summary>
        /// Fx send B.
        /// </summary>
        public byte FxSendB;

        /// <summary>
        /// Fx send C.
        /// </summary>
        public byte FxSendC;

        /// <summary>
        /// Main send.
        /// </summary>
        public byte MainSend = 127;

        /// <summary>
        /// Read the info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            Pitch = r.ReadSingle();
            Pan = r.ReadByte();
            SurroundPan = r.ReadByte();
            FxSendA = r.ReadByte();
            FxSendB = r.ReadByte();
            FxSendC = r.ReadByte();
            MainSend = r.ReadByte();
            r.ReadBytes(22);
        }

        /// <summary>
        /// Write the info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(Pitch);
            w.Write(Pan);
            w.Write(SurroundPan);
            w.Write(FxSendA);
            w.Write(FxSendB);
            w.Write(FxSendC);
            w.Write(MainSend);
            w.Write(new byte[22]);
        }

    }

}

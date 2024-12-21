using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare {

    /// <summary>
    /// NDS header.
    /// </summary>
    public class NFileHeader : FileHeader {

        /// <summary>
        /// Read the header.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            Magic = new string(r.ReadChars(4));
            r.ByteOrder = ByteOrder.BigEndian;
            r.ByteOrder = ByteOrder = r.ReadUInt16() == 0xFEFF ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
            r.ReadUInt16(); //Version is always constant.
            FileSize = r.ReadUInt32();
            HeaderSize = r.ReadUInt16();
            r.ReadUInt16();
            BlockOffsets = new long[] { 0x10 };
        }

        /// <summary>
        /// Write the header.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            HeaderSize = 0x10;
            w.ByteOrder = ByteOrder.LittleEndian;
            w.Write(Magic.ToCharArray());
            w.Write((ushort)0xFEFF);
            w.Write((ushort)0x0100);
            w.Write((uint)FileSize);
            w.Write((ushort)HeaderSize);
            w.Write((ushort)1);
        }

    }

}

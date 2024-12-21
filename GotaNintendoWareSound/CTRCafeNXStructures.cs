using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Version = GotaSoundIO.IO.Version;

namespace GotaNintendoWare {

    /// <summary>
    /// CTR, Cafe, Switch version.
    /// </summary>
    public class XVersion : Version {

        /// <summary>
        /// If the version is an F one and not a C one.
        /// </summary>
        public bool FType = true;

        /// <summary>
        /// Read the version.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            uint version = r.ReadUInt32();
            if (FType) {
                Major = (byte)((version & 0x00FF0000) >> 16);
                Minor = (byte)((version & 0x0000FF00) >> 8);
                Revision = (byte)(version & 0x000000FF);
            } else {
                Major = (byte)((version & 0xFF000000) >> 24);
                Minor = (byte)((version & 0x00FF0000) >> 16);
                Revision = (byte)((version & 0x0000FF00) >> 8);
            }
        }

        /// <summary>
        /// Write the version.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            uint ver = 0;
            if (FType) {
                ver |= (uint)(Major << 16);
                ver |= (uint)(Minor << 8);
                ver |= (uint)(Revision << 0);
            } else {
                ver |= (uint)(Major << 24);
                ver |= (uint)(Minor << 16);
                ver |= (uint)(Revision << 8);
            }
            w.Write(ver);
        }

    }

    /// <summary>
    /// CTR, Cafe, Switch header.
    /// </summary>
    public class XFileHeader : FileHeader {

        /// <summary>
        /// F type.
        /// </summary>
        public bool FType => Magic.StartsWith("F");

        /// <summary>
        /// Read the header.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            Magic = new string(r.ReadChars(4));
            r.ByteOrder = ByteOrder = ByteOrder.BigEndian;
            if (r.ReadUInt16() == 0xFFFE) {
                ByteOrder = r.ByteOrder = ByteOrder.LittleEndian;
            }
            HeaderSize = r.ReadUInt16();
            Version = new XVersion();
            (Version as XVersion).FType = FType;
            Version.Read(r);
            FileSize = r.ReadUInt32();
            ushort numBlocks = r.ReadUInt16();
            r.ReadUInt16();
            BlockOffsets = new long[numBlocks];
            BlockSizes = new long[numBlocks];
            BlockTypes = new long[numBlocks];
            for (int i = 0; i < numBlocks; i++) {
                BlockTypes[i] = r.ReadUInt16();
                r.ReadUInt16();
                BlockOffsets[i] = r.ReadUInt32();
                BlockSizes[i] = r.ReadUInt32();
            }
        }

        /// <summary>
        /// Write the header.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            w.Write(Magic.ToCharArray());
            w.ByteOrder = ByteOrder;
            w.Write((ushort)0xFEFF);
            w.Write((ushort)HeaderSize);
            (Version as XVersion).FType = FType;
            w.Write(Version);
            w.Write((uint)FileSize);
            w.Write((ushort)BlockOffsets.Length);
            w.Write((ushort)0);
            for (int i = 0; i < BlockOffsets.Length; i++) {
                w.Write((ushort)BlockTypes[i]);
                w.Write((ushort)0);
                w.Write((uint)BlockOffsets[i]);
                w.Write((uint)BlockSizes[i]);
            }
            w.Align(0x20);
        }

    }

    /// <summary>
    /// Reference for 3ds, Wii U, and Switch.
    /// </summary>
    /// <typeparam name="T">Reference type.</typeparam>
    public class XReference<T> : Reference<T> {

        /// <summary>
        /// Null reference is 0.
        /// </summary>
        /// <returns>If the null reference is 0.</returns>
        public override bool NullReferenceIs0() => false;

        /// <summary>
        /// If to set the current offset when jumping.
        /// </summary>
        /// <returns>Set current offset when jumping.</returns>
        public override bool SetCurrentOffsetOnJump() => true;

        /// <summary>
        /// Read the reference.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void ReadRef(FileReader r) {
            Identifier = r.ReadUInt16();
            r.ReadUInt16();
            Offset = r.ReadInt32();
        }

        /// <summary>
        /// Write the reference.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="ignoreNullData">If to ignore raw data.</param>
        public override void WriteRef(FileWriter w, bool ignoreNullData = false) {
            if (!ignoreNullData && ((NullReferenceIs0() && Offset == 0) || (!NullReferenceIs0() && Offset == -1))) {
                w.Write((uint)0);
                w.Write(-1);
            } else {
                w.Write((ushort)Identifier);
                w.Write((ushort)0);
                w.Write((int)Offset);
            }
        }

    }

    /// <summary>
    /// Reference for 3ds, Wii U, and Switch with size.
    /// </summary>
    /// <typeparam name="T">Reference type.</typeparam>
    public class XRSizedReference<T> : Reference<T> {

        /// <summary>
        /// Null reference is 0.
        /// </summary>
        /// <returns>If the null reference is 0.</returns>
        public override bool NullReferenceIs0() => false;

        /// <summary>
        /// If to set the current offset when jumping.
        /// </summary>
        /// <returns>Set current offset when jumping.</returns>
        public override bool SetCurrentOffsetOnJump() => true;

        /// <summary>
        /// Read the reference.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void ReadRef(FileReader r) {
            Identifier = r.ReadUInt16();
            r.ReadUInt16();
            Offset = r.ReadInt32();
            Size = r.ReadInt32();
        }

        /// <summary>
        /// Write the reference.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="ignoreNullData">If to ignore raw data.</param>
        public override void WriteRef(FileWriter w, bool ignoreNullData = false) {
            if (!ignoreNullData && ((NullReferenceIs0() && Offset == 0) || (!NullReferenceIs0() && Offset == -1))) {
                w.Write((uint)0);
                w.Write(-1);
                w.Write(-1);
            } else {
                w.Write((ushort)Identifier);
                w.Write((ushort)0);
                w.Write((int)Offset);
                w.Write((int)Size);
            }
        }

    }

}

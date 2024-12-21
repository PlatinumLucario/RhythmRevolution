using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Version = GotaSoundIO.IO.Version;

namespace GotaNintendoWare {

    /// <summary>
    /// A version.
    /// </summary>
    public class RVersion : Version {

        /// <summary>
        /// Create a version from a ushort.
        /// </summary>
        /// <param name="s">The ushort.</param>
        public void FromUShort(ushort s) {
            Major = (byte)((s & 0xFF00) >> 8);
            Minor = (byte)(s & 0xFF);
        }

        /// <summary>
        /// Convert the version to a ushort.
        /// </summary>
        /// <returns>The version as a ushort.</returns>
        public ushort ToUShort() {
            return (ushort)((Major << 8) | Minor);
        }

        /// <summary>
        /// Read the version.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            FromUShort(r.ReadUInt16());
        }

        /// <summary>
        /// Write the version.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            w.Write(ToUShort());
        }

    }

    /// <summary>
    /// A revolution file header.
    /// </summary>
    public class RFileHeader : FileHeader {

        /// <summary>
        /// Read the header.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            Magic = new string(r.ReadChars(4));
            ByteOrder = r.ByteOrder = ByteOrder.BigEndian;
            if (r.ReadUInt16() == 0xFFFE) {
                ByteOrder = ByteOrder.LittleEndian;
            }
            Version = r.Read<RVersion>();
            FileSize = r.ReadUInt32();
            HeaderSize = r.ReadUInt16();
            ushort numBlocks = r.ReadUInt16();
            BlockOffsets = new long[numBlocks];
            BlockSizes = new long[numBlocks];
            for (int i = 0; i < numBlocks; i++) {
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
            w.Write(Version);
            w.Write((uint)FileSize);
            HeaderSize = 0x10 + 8 * BlockOffsets.Length;
            while (HeaderSize % 0x20 != 0) {
                HeaderSize++;
            }
            w.Write((ushort)HeaderSize);
            w.Write((ushort)BlockOffsets.Length);
            for (int i = 0; i < BlockOffsets.Length; i++) {
                w.Write((uint)BlockOffsets[i]);
                w.Write((uint)BlockSizes[i]);
            }
            w.Align(0x20);
        }

    }

    /// <summary>
    /// Offset to data.
    /// </summary>
    public class ROffset<T> : Reference<T> {

        /// <summary>
        /// Don't set current offset on jump.
        /// </summary>
        public override bool SetCurrentOffsetOnJump() => false;

        /// <summary>
        /// Null reference is not 0.
        /// </summary>
        public override bool NullReferenceIs0() => false;

        /// <summary>
        /// Read the reference.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void ReadRef(FileReader r) {
            Offset = r.ReadInt32();
        }

        /// <summary>
        /// Write the reference.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void WriteRef(FileWriter w, bool ignoreNullData = false) {
            w.Write((int)Offset);
        }

    }

    /// <summary>
    /// Reference to data.
    /// </summary>
    public class RReference<T> : Reference<T> {

        /// <summary>
        /// Don't set current offset on jump.
        /// </summary>
        public override bool SetCurrentOffsetOnJump() => false;

        /// <summary>
        /// Null reference is not 0.
        /// </summary>
        public override bool NullReferenceIs0() => true;

        /// <summary>
        /// Read the reference.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void ReadRef(FileReader r) {
            Absolute = !r.ReadBoolean();
            Identifier = r.ReadByte();
            r.ReadUInt16();
            Offset = r.ReadInt32();
        }

        /// <summary>
        /// Write the reference.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="ignoreNullData">Ignore null data.</param>
        public override void WriteRef(FileWriter w, bool ignoreNullData = false) {
            if (!ignoreNullData && ((NullReferenceIs0() && Offset == 0) || (!NullReferenceIs0() && Offset == -1))) {
                w.Write(new byte[8]);
            } else {
                w.Write(!Absolute);
                w.Write((byte)Identifier);
                w.Write((ushort)0);
                w.Write((int)Offset);
            }
        }

    }

    /// <summary>
    /// Sized reference to data.
    /// </summary>
    public class RSizedReference<T> : Reference<T> {

        /// <summary>
        /// Don't set current offset on jump.
        /// </summary>
        public override bool SetCurrentOffsetOnJump() => false;

        /// <summary>
        /// Null reference is not 0.
        /// </summary>
        public override bool NullReferenceIs0() => true;

        /// <summary>
        /// Read the reference.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void ReadRef(FileReader r) {
            Absolute = !r.ReadBoolean();
            Identifier = r.ReadByte();
            r.ReadUInt16();
            Offset = r.ReadInt32();
            Size = r.ReadUInt32();
        }

        /// <summary>
        /// Write the reference.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="ignoreNullData">Ignore null data.</param>
        public override void WriteRef(FileWriter w, bool ignoreNullData = false) {
            if (!ignoreNullData && ((NullReferenceIs0() && Offset == 0) || (!NullReferenceIs0() && Offset == -1))) {
                w.Write(new byte[12]);
            } else {
                w.Write(!Absolute);
                w.Write((byte)Identifier);
                w.Write((ushort)0);
                w.Write((int)Offset);
                w.Write((uint)Size);
            }
        }

    }

}

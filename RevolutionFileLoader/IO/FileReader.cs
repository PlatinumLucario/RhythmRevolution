using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// File reader.
    /// </summary>
    public class FileReader : BinaryDataReader {

        /// <summary>
        /// The start of the file.
        /// </summary>
        public long FileOffset;

        /// <summary>
        /// File offsets cache.
        /// </summary>
        public Stack<long> FileOffsetsCache = new Stack<long>();

        /// <summary>
        /// Current offset position.
        /// </summary>
        public long CurrentOffset;

        /// <summary>
        /// Structure offsets.
        /// </summary>
        public Stack<long> StructureOffsets = new Stack<long>();

        /// <summary>
        /// Block offsets.
        /// </summary>
        public uint[] BlockOffsets;

        /// <summary>
        /// Block offsets cache.
        /// </summary>
        public Stack<uint[]> BlockOffsetsCache = new Stack<uint[]>();

        /// <summary>
        /// Block sizes.
        /// </summary>
        public uint[] BlockSizes;

        /// <summary>
        /// Block sizes cache.
        /// </summary>
        public Stack<uint[]> BlockSizesCache = new Stack<uint[]>();

        /// <summary>
        /// Offsets.
        /// </summary>
        public Dictionary<string, uint> Offsets = new Dictionary<string, uint>();

        /// <summary>
        /// References.
        /// </summary>
        public Dictionary<string, Reference> References = new Dictionary<string, Reference>();

        //Constructors.
        #region Constructors

        public FileReader(Stream input) : base(input) {
        }

        public FileReader(Stream input, bool leaveOpen) : base(input, leaveOpen) {
        }

        public FileReader(Stream input, Encoding encoding) : base(input, encoding) {
        }

        public FileReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) {
        }

        #endregion

        /// <summary>
        /// Read an item.
        /// </summary>
        /// <typeparam name="T">IReadable type to read.</typeparam>
        /// <returns>The item.</returns>
        public T Read<T>() {

            //Invalid type.
            if (!typeof(IReadable).IsAssignableFrom(typeof(T))) {
                throw new Exception("Type \"" + typeof(T).ToString() + "\" does not implement IReadable.");
            }

            //Read.
            T r = (T)Activator.CreateInstance(typeof(T));
            (r as IReadable).Read(this);
            return r;

        }

        /// <summary>
        /// Read a null terminated string.
        /// </summary>
        /// <returns>The string.</returns>
        public string ReadNullTerminated() {
            string s = "";
            char c = ReadChar();
            while (c != 0) {
                s += c;
                c = ReadChar();
            }
            return s;
        }

        /// <summary>
        /// Open a file.
        /// </summary>
        /// <param name="magic">The file magic.</param>
        /// <param name="byteOrder">Byte order of the file.</param>
        /// <param name="version">Version of the file.</param>
        /// <param name="fileSize">Size of the file.</param>
        /// <param name="headerSize">Size of the header.</param>
        /// <param name="numBlocks">Number of blocks.</param>
        /// <param name="setOffset">Whether or not to set the current offset.</param>
        public void OpenFile(out string magic, out ByteOrder byteOrder, out Version version, out uint fileSize, out ushort headerSize, out ushort numBlocks, bool setOffset = true) {
            FileOffsetsCache.Push(FileOffset);
            BlockOffsetsCache.Push(BlockOffsets);
            BlockSizesCache.Push(BlockSizes);
            FileOffset = Position;
            if (setOffset) CurrentOffset = Position;
            magic = new string(ReadChars(4));
            ByteOrder = ByteOrder.BigEndian;
            if (ReadUInt16() == 0xFFFE) {
                ByteOrder = ByteOrder.LittleEndian;
            }
            byteOrder = ByteOrder;
            version = Read<Version>();
            fileSize = ReadUInt32();
            headerSize = ReadUInt16();
            numBlocks = ReadUInt16();
            BlockOffsets = new uint[numBlocks];
            BlockSizes = new uint[numBlocks];
            for (int i = 0; i < numBlocks; i++) {
                BlockOffsets[i] = ReadUInt32();
                BlockSizes[i] = ReadUInt32();
            }
        }

        /// <summary>
        /// Open a block.
        /// </summary>
        /// <param name="blockNum">The block number.</param>
        /// <param name="magic">Block magic.</param>
        /// <param name="size">Block size.</param>
        /// <param name="readMagicAndSize">Whether or not to read the block magic and size.</param>
        /// <param name="setOffset">Whether or not to set the current offset.</param>
        public void OpenBlock(int blockNum, out string magic, out uint size, bool readMagicAndSize = true, bool setOffset = true) {
            Position = FileOffset + BlockOffsets[blockNum];
            if (setOffset) { CurrentOffset = Position; }
            magic = "";
            size = 0;
            if (readMagicAndSize) {
                magic = new string(ReadChars(4));
                size = ReadUInt32();
            }
            CurrentOffset = Position;
        }

        /// <summary>
        /// Start a structure.
        /// </summary>
        public void StartStructure() {
            StructureOffsets.Push(CurrentOffset);
            CurrentOffset = Position;
        }

        /// <summary>
        /// End a structure.
        /// </summary>
        public void EndStructure() {
            CurrentOffset = StructureOffsets.Pop();
        }

        /// <summary>
        /// Jump to an offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="absolute">If the offset is absolute.</param>
        public void Jump(long offset, bool absolute = false) {
            if (absolute) {
                Position = FileOffset + offset;
            } else {
                Position = CurrentOffset + offset;
            }
        }

        /// <summary>
        /// Open an offset.
        /// </summary>
        /// <param name="name">Offset name.</param>
        public void OpenOffset(string name) {
            Offsets.Add(name, ReadUInt32());
        }

        /// <summary>
        /// Jump to an offset.
        /// </summary>
        /// <param name="name">Offset name.</param>
        /// <param name="remove">Remove the offset after jumping.</param>
        /// <param name="absolute">If the offset is absolute.</param>
        public void JumpToOffset(string name, bool remove = true, bool absolute = false) {
            if (absolute) {
                Position = FileOffset + Offsets[name];
            } else {
                Position = CurrentOffset + Offsets[name];
            }
            if (remove) { CloseOffset(name); }
        }

        /// <summary>
        /// If an offset is null.
        /// </summary>
        /// <param name="name">The offset.</param>
        /// <returns>If the offset is null.</returns>
        public bool OffsetNull(string name) {
            if (!Offsets[name].Equals(0xFFFFFFFF) && !Offsets[name].Equals(0)) {
                return false;
            }
            Offsets.Remove(name);
            return true;
        }

        /// <summary>
        /// Close an offset.
        /// </summary>
        /// <param name="name">Offset to close.</param>
        public void CloseOffset(string name) {
            Offsets.Remove(name);
        }

        /// <summary>
        /// Open a reference.
        /// </summary>
        /// <param name="name">Name of the reference.</param>
        public void OpenReference(string name) {
            References.Add(name, Read<Reference>());
        }

        /// <summary>
        /// Jump to a reference.
        /// </summary>
        /// <param name="name">Name of the reference to jump to.</param>
        /// <param name="remove">If to remove the reference.</param>
        public void JumpToReference(string name, bool remove = true) {
            if (References[name].Type == ReferenceType.Address) {
                Position = FileOffset + References[name].Value;
            } else {
                Position = CurrentOffset + References[name].Value;
            }
            if (remove) { CloseReference(name); }
        }

        /// <summary>
        /// If a reference is null.
        /// </summary>
        /// <param name="name">Reference.</param>
        /// <returns>If the reference is null.</returns>
        public bool ReferenceNull(string name) {
            if (!References[name].Value.Equals(0xFFFFFFFF) && !References[name].Value.Equals(0)) {
                return false;
            }
            References.Remove(name);
            return true;
        }

        /// <summary>
        /// Get the data type of a reference.
        /// </summary>
        /// <param name="name">The name of the reference.</param>
        /// <returns>The data type of the reference.</returns>
        public DataIdentifier ReferenceIdentifier(string name) {
            return References[name].Identifier;
        }

        /// <summary>
        /// Close a reference.
        /// </summary>
        /// <param name="name">Reference to close.</param>
        public void CloseReference(string name) {
            References.Remove(name);
        }

    }

}

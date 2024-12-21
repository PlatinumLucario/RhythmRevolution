using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// File writer.
    /// </summary>
    public class FileWriter : BinaryDataWriter {

        /// <summary>
        /// Current offset.
        /// </summary>
        public long CurrentOffset;

        /// <summary>
        /// The start of the file.
        /// </summary>
        public long FileOffset;

        /// <summary>
        /// Structure offsets.
        /// </summary>
        public Stack<long> StructureOffsets = new Stack<long>();

        /// <summary>
        /// Offsets.
        /// </summary>
        public Dictionary<string, long> Offsets = new Dictionary<string, long>();

        //Contructors.
        #region

        public FileWriter(Stream output) : base(output) {
        }

        public FileWriter(Stream output, bool leaveOpen) : base(output, leaveOpen) {
        }

        public FileWriter(Stream output, Encoding encoding) : base(output, encoding) {
        }

        public FileWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen) {
        }

        #endregion

        /// <summary>
        /// Write an item.
        /// </summary>
        ///<param name="w">Item to write.</param>
        public void Write(IWritable w) {

            //Write.
            w.Write(this);

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
        /// Initialize an offset.
        /// </summary>
        /// <param name="name">Name of the offset.</param>
        public void InitOffset(string name) {
            Offsets.Add(name, Position);
            Write((uint)0);
        }

        /// <summary>
        /// Close and offset.
        /// </summary>
        /// <param name="name">Offset name.</param>
        /// <param name="absolute">If the offset is absolute.</param>
        public void CloseOffset(string name, bool absolute = false) {
            long bak = Position;
            Position = Offsets[name];
            Offsets.Remove(name);
            if (absolute) {
                Write((uint)(bak - FileOffset));
            } else {
                Write((uint)(bak - CurrentOffset));
            }
            Position = bak;
        }

    }

}

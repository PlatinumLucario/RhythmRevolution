using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Group info.
    /// </summary>
    public class GroupInfo : IReadable, IWriteable {

        /// <summary>
        /// Name.
        /// </summary>
        public string Name;

        /// <summary>
        /// String Id when reading.
        /// </summary>
        public Id ReadingStringId;

        /// <summary>
        /// Entry number.
        /// </summary>
        public int EntryNumber;

        /// <summary>
        /// External file path.
        /// </summary>
        public string ExternalFilePath;

        /// <summary>
        /// Offset to the first file in the group. (Relative to BRSAR start).
        /// </summary>
        public uint Offset;

        /// <summary>
        /// Size of the cluster of files in the group.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Wave data offset. (Relative to BRSAR start).
        /// </summary>
        public uint WaveDataOffset;

        /// <summary>
        /// Wave data size.
        /// </summary>
        public uint WaveDataSize;

        /// <summary>
        /// Group items.
        /// </summary>
        public List<GroupItem> Items;

        /// <summary>
        /// Read the entry.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            ReadingStringId = r.Read<Id>();
            EntryNumber = r.ReadInt32();
            var extDat = r.Read<RReference<NullTerminatedString>>().Data;
            ExternalFilePath = extDat != null ? extDat.Data : null;
            Offset = r.ReadUInt32();
            Size = r.ReadUInt32();
            WaveDataOffset = r.ReadUInt32();
            WaveDataSize = r.ReadUInt32();
            var tableDat = r.Read<RReference<Table<RReference<GroupItem>>>>().Data;
            Items = tableDat == null ? null : tableDat.Select(x => x.Data).ToList();
        }

        /// <summary>
        /// Write the entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(ReadingStringId);
            w.Write(EntryNumber);
            w.InitReference<RReference<object>>("ExtData");
            w.Write(Offset);
            w.Write(Size);
            w.Write(WaveDataOffset);
            w.Write(WaveDataSize);
            w.InitReference<RReference<object>>("Tbl");
            if (ExternalFilePath == null) {
                w.CloseReference("ExtData", 0, true, 0);
            } else {
                w.CloseReference("ExtData");
                w.Write(new NullTerminatedString(ExternalFilePath));
                w.Align(0x4);
            }
            if (Items == null) {
                w.CloseReference("Tbl", 0, true, 0);
            } else {
                w.CloseReference("Tbl");
                w.WriteTableWithReferences(Items);
            }
        }

    }

}

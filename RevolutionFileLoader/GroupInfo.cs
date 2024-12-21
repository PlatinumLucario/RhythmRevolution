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
    public class GroupInfo : IReadable, IWritable {

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
        /// First file.
        /// </summary>
        //public FileEntry FirstFile;

        /// <summary>
        /// Last file.
        /// </summary>
        //public FileEntry LastFile;

        /// <summary>
        /// Offset to the first file in the group. (Relative to BRSAR start).
        /// </summary>
        public uint Offset;

        /// <summary>
        /// Size of the cluster of files in the group.
        /// </summary>
        public uint Size;

        /// <summary>
        /// First WAR.
        /// </summary>
        //public FileEntry FirstWAR;

        /// <summary>
        /// Last WAR.
        /// </summary>
        //public FileEntry LastWAR;

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
            r.OpenReference<RReference>("ExtGroupName");
            Offset = r.ReadUInt32();
            Size = r.ReadUInt32();
            WaveDataOffset = r.ReadUInt32();
            WaveDataSize = r.ReadUInt32();
            r.OpenReference<RReference>("GroupItemTable");
            if (!r.ReferenceNull("ExtGroupName")) {
                r.JumpToReference("ExtGroupName");
                ExternalFilePath = r.ReadNullTerminated();
            }
            if (!r.ReferenceNull("GroupItemTable")) {
                r.JumpToReference("GroupItemTable");
                Items = new List<GroupItem>();
                Table<RReference> groupItems = r.Read<Table<RReference>>();
                for (int i = 0; i < groupItems.Count; i++) {
                    if (groupItems[i].Offset != 0) {
                        r.Jump(groupItems[i].Offset);
                        Items.Add(r.Read<GroupItem>());
                    } else {
                        Items.Add(null);
                    }
                }
            }
        }

        /// <summary>
        /// Write the entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            throw new NotImplementedException();
        }

    }

}

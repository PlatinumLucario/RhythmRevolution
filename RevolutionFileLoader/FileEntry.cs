using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// File entry.
    /// </summary>
    public class FileEntry : IReadable, IWritable {

        /// <summary>
        /// File contained.
        /// </summary>
        public IOFile File {

            //Get.
            get {
                return m_File;
            }

            //Set.
            set {
                m_File = value;
            }
        
        }
        private IOFile m_File;

        /// <summary>
        /// File size.
        /// </summary>
        public uint FileSize;

        /// <summary>
        /// Wave data file size.
        /// </summary>
        public uint WaveDataFileSize;

        /// <summary>
        /// Entry number.
        /// </summary>
        public int EntryNumber;

        /// <summary>
        /// External file path.
        /// </summary>
        public string ExternalFilePath;

        /// <summary>
        /// File info.
        /// </summary>
        public List<FileInfo> FileInfo;

        /// <summary>
        /// Read the file entry.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {

            //Read info.
            FileSize = r.ReadUInt32();
            WaveDataFileSize = r.ReadUInt32();
            EntryNumber = r.ReadInt32();
            r.OpenReference<RReference>("ExtPath");
            r.OpenReference<RReference>("PosTable");
            if (!r.ReferenceNull("ExtPath")) {
                r.JumpToReference("ExtPath");
                ExternalFilePath = r.ReadNullTerminated();
            }
            if (!r.ReferenceNull("PosTable")) {
                r.JumpToReference("PosTable");
                var fileInfoRefs = r.Read<Table<RReference>>();
                FileInfo = new List<FileInfo>();
                foreach (var f in fileInfoRefs) {
                    if (f.Offset != 0 && f.Offset != -1) {
                        r.Jump(f.Offset, f.Absolute);
                        FileInfo.Add(r.Read<FileInfo>());
                    } else {
                        FileInfo.Add(null);
                    }
                }
            }

        }

        /// <summary>
        /// Write the file entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            
            //TODO.

        }

    }

}

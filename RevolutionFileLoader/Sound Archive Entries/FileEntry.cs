using GotaSoundIO;
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
    public class FileEntry : IReadable, IWriteable {

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
        /// Wave data offset.
        /// </summary>
        public uint WaveDataOffset;

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
            var extDat = r.Read<RReference<NullTerminatedString>>().Data;
            ExternalFilePath = extDat != null ? extDat.Data : null;
            var posDat = r.Read<RReference<Table<RReference<FileInfo>>>>().Data;
            FileInfo = posDat != null ? posDat.Select(x => x.Data).ToList() : null;

        }

        /// <summary>
        /// Write the file entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {

            //Write info.
            w.Write(FileSize);
            w.Write(WaveDataFileSize);
            w.Write(EntryNumber);
            w.InitReference<RReference<object>>("ExtPath");
            w.InitReference<RReference<object>>("FileInfoTable");

            //External file path.
            if (ExternalFilePath == null) {
                w.CloseReference("ExtPath", 0, true, 0);
            } else {
                w.CloseReference("ExtPath");
                w.Write(new NullTerminatedString(ExternalFilePath));
                w.Align(0x4);
            }

            //File info table.
            if (FileInfo == null) {
                w.CloseReference("FileInfoTable", 0, true, 0);
            } else {
                w.CloseReference("FileInfoTable");
                w.WriteTableWithReferences(FileInfo);
            }

        }

    }

}

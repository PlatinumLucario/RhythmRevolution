using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Bank info.
    /// </summary>
    public class BankInfo : IReadable, IWriteable {

        /// <summary>
        /// Name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Bank file.
        /// </summary>
        public FileData<Bank> BankFile;

        /// <summary>
        /// String Id when reading.
        /// </summary>
        public Id ReadingStringId;

        /// <summary>
        /// File Id when reading.
        /// </summary>
        public Id ReadingFileId;

        /// <summary>
        /// Read the entry.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            ReadingStringId = r.Read<Id>();
            ReadingFileId = r.Read<Id>();
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(ReadingStringId);
            w.Write(ReadingFileId);
            w.Write((uint)0);
        }

    }

}

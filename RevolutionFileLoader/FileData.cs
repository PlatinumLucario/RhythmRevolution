using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {
    
    /// <summary>
    /// File data.
    /// </summary>
    public class FileData<T> : FileData where T : IOFile {

        /// <summary>
        /// Actual file data.
        /// </summary>
        public override IOFile File {
            get => FileS;
            set { FileS = (T)value; }
        }

        /// <summary>
        /// Get the file.
        /// </summary>
        public T GetFile() => GetFile<T>();

        /// <summary>
        /// Specific type of file.
        /// </summary>
        public T FileS;

    }

    /// <summary>
    /// File data without type.
    /// </summary>
    public class FileData {

        /// <summary>
        /// Sound archive folder path.
        /// </summary>
        public string SoundArchiveFolder = "";

        /// <summary>
        /// If the file is external.
        /// </summary>
        public bool External;

        /// <summary>
        /// External file path.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Wave archive data. Used for banks and WSDs.
        /// </summary>
        public WaveArchive WaveArchive;

        /// <summary>
        /// Groups.
        /// </summary>
        internal List<int> Groups;

        /// <summary>
        /// Indices in groups.
        /// </summary>
        internal List<uint> GroupIndices;

        /// <summary>
        /// Raw file data.
        /// </summary>
        public virtual IOFile File {
            get => m_File;
            set { m_File = value; }
        }
        private IOFile m_File;

        /// <summary>
        /// Get the file.
        /// </summary>
        /// <returns>Actual file data.</returns>
        public T GetFile<T>() where T : IOFile {
            if (External) {
                var ret = Activator.CreateInstance<T>();
                ret.Read(SoundArchiveFolder + "/" + FilePath);
                return ret;
            } else {
                return (T)File;
            }
        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="file">The file.</param>
        public void WriteFile(IOFile file) {
            File = file;
            if (External) {
                file.Write(SoundArchiveFolder + "/" + FilePath);
            }
        }

    }

}

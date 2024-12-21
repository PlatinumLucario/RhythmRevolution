using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {
    
    /// <summary>
    /// Easier to use group.
    /// </summary>
    public class Group {

        /// <summary>
        /// Group name.
        /// </summary>
        public string Name;

        /// <summary>
        /// External path.
        /// </summary>
        public string ExternalPath;

        /// <summary>
        /// If the group is external.
        /// </summary>
        public bool IsExternal;

        /// <summary>
        /// Files.
        /// </summary>
        public List<FileData> Files = null;

        /// <summary>
        /// If wave archives should be shared between the files.
        /// </summary>
        public bool ShareWaveArchives;

    }

}

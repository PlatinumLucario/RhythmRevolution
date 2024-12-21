using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare {
    
    /// <summary>
    /// Something that can change platform.
    /// </summary>
    public interface IPlatformChangeable {

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform to read.</param>
        void Read(FileReader r, Platform platform);

        /// <summary>
        /// Write the item.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="platform">Platform to write.</param>
        void Write(FileWriter w, Platform platform);

    }

}

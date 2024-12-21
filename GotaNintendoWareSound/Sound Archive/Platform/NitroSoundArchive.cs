using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Nitro sound archive.
    /// </summary>
    public class NitroSoundArchive : SoundArchive {
        
        /// <summary>
        /// Read the sound archive.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write the sound archive.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert the sound archive.
        /// </summary>
        /// <param name="platform">Target platform.</param>
        /// <returns>Converted sound archive.</returns>
        public override SoundArchive Convert(Platform platform) {
            throw new NotImplementedException();
        }

    }

}

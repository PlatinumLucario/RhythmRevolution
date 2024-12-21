using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Wave sound set item.
    /// </summary>
    public class WaveSoundSetItem : ISoundArchiveItem {

        /// <summary>
        /// Name Id.
        /// </summary>
        public int NameId { get; set; }

        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {

            }

            //Revolution.
            else if (platform == Platform.Revolution) {

            }

            //CTR, Cafe, NX.
            else {

            }

        }

        /// <summary>
        /// Write the platform.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="platform">Platform.</param>
        public void Write(FileWriter w, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {

            }

            //Revolution.
            else if (platform == Platform.Revolution) {

            }

            //CTR, Cafe, NX.
            else {

            }

        }

    }

}

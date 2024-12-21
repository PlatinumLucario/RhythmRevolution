using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Sequence archive item.
    /// </summary>
    public class SequenceArchiveItem : ISoundArchiveItem {

        /// <summary>
        /// Name Id.
        /// </summary>
        public int NameId { get; set; }

        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Files.
        /// </summary>
        public List<ItemReference<FileItem>> Files;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                Files = new List<ItemReference<FileItem>>() { new ItemReference<FileItem>() { Index = r.ReadInt32() } };
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                //Can not be read.
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
                w.Write(Files[0].Index);
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                //Can not be read.
            }

            //CTR, Cafe, NX.
            else {

            }

        }

    }

}

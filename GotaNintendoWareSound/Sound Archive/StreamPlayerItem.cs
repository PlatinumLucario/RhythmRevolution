using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Stream player item.
    /// </summary>
    public class StreamPlayerItem : ISoundArchiveItem {

        /// <summary>
        /// Name Id.
        /// </summary>
        public int NameId { get; set; }

        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Left or mono channel.
        /// </summary>
        public byte LeftMonoChannel;

        /// <summary>
        /// Right channel.
        /// </summary>
        public byte? RightChannel;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                bool isStereo = r.ReadBoolean();
                LeftMonoChannel = r.ReadByte();
                if (isStereo) { RightChannel = r.ReadByte(); } else { r.ReadByte(); }
                r.ReadBytes(21);
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                //Does not exist.
            }

            //CTR, Cafe, NX.
            else {
                //Does not exist.
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
                w.Write(RightChannel != null);
                w.Write(LeftMonoChannel);
                w.Write(RightChannel ?? 0);
                w.Write(new byte[21]);
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                //Does not exist.
            }

            //CTR, Cafe, NX.
            else {
                //Does not exist.
            }

        }

    }

}

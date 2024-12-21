using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// A wave archive item.
    /// </summary>
    public class WaveArchiveItem : ISoundArchiveItem {

        /// <summary>
        /// Name Id.
        /// </summary>
        public int NameId { get; set; }

        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// File reference.
        /// </summary>
        public ItemReference<FileItem> File;

        /// <summary>
        /// Load wave items individually.
        /// </summary>
        public bool LoadIndividually;

        /// <summary>
        /// Write wave count.
        /// </summary>
        public bool WriteWaveCount;

        /// <summary>
        /// Wave count.
        /// </summary>
        public uint WaveCount;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                uint val = r.ReadUInt32();
                LoadIndividually = (val & 0xFF000000) > 0;
                File = new ItemReference<FileItem>() { Index = (int)(val & 0xFFFFFF) };
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                //This doesn't even exist.
            }

            //CTR, Cafe, NX.
            else {
                File = new ItemReference<FileItem>() { Index = r.ReadInt32() };
                LoadIndividually = r.ReadBoolean();
                r.ReadBytes(3);
                var f = r.Read<FlagParameters>();
                NameId = (int)(f[0] ?? new uint?(0xFFFFFFFF));
                if (f[1] != null) {
                    WriteWaveCount = true;
                    WaveCount = f[1].Value;
                }
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
                uint val = (uint)(File.Index & 0xFFFFFF);
                if (LoadIndividually) { val |= 0xFF000000; }
                w.Write(val);
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                //This doesn't even exist.
            }

            //CTR, Cafe, NX.
            else {
                w.Write(File.Index);
                w.Write(LoadIndividually);
                w.Write(new byte[3]);
                var f = new FlagParameters();
                if (NameId != -1) { f[0] = (uint)NameId; }
                if (WriteWaveCount) { f[1] = WaveCount; }
                w.Write(f);
            }

        }

    }

}

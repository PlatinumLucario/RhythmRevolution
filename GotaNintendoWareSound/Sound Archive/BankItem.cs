using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Bank item.
    /// </summary>
    public class BankItem : ISoundArchiveItem {

        /// <summary>
        /// Name Id.
        /// </summary>
        public int NameId { get; set; }

        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// File.
        /// </summary>
        public ItemReference<FileItem> File;

        /// <summary>
        /// Wave archives.
        /// </summary>
        public List<ItemReference<WaveArchiveItem>> WaveArchives;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                File = new ItemReference<FileItem>() { Index = r.ReadInt32() };
                WaveArchives = new List<ItemReference<WaveArchiveItem>>();
                for (int i = 0; i < 4; i++) {
                    WaveArchives.Add(new ItemReference<WaveArchiveItem>() { Index = (int)r.ReadUInt16() });
                    if (WaveArchives.Last().Index == 0xFFFF) { WaveArchives.Last().Index = -1; }
                }
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                NameId = r.Read<IdType>().Id;
                File = new ItemReference<FileItem>() { Index = r.Read<IdType>().Id };
                r.ReadUInt32();
            }

            //CTR, Cafe, NX.
            else {
                File = new ItemReference<FileItem>() { Index = r.ReadInt32() };
                int[] waveIds = r.Read<XReference<Table<int>>>().Data.ToArray();
                WaveArchives = waveIds.Select(x => new ItemReference<WaveArchiveItem>() { Index = x }).ToList();
                var f = r.Read<FlagParameters>();
                NameId = (int)(f[0] ?? new uint?(0xFFFFFFFF));
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
                w.Write(File.Index);
                for (int i = 0; i < 4; i++) {
                    if (i < WaveArchives.Count) {
                        if (WaveArchives[i].Index == -1) {
                            w.Write((short)-1);
                        } else {
                            w.Write((ushort)WaveArchives[i].Index);
                        }
                    } else {
                        w.Write((short)-1);
                    }
                }
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                w.Write(new IdType() { Id = (Int24)NameId });
                w.Write(new IdType() { Id = (Int24)File.Index });
                w.Write((uint)0);
            }

            //CTR, Cafe, NX.
            else {
                w.Write(File.Index);
                XReference<Table<int>> warcs = new XReference<Table<int>>() { Data = (Table<int>)WaveArchives.Select(x => x.Index).ToList() };
                warcs.InitWrite(w);
                var f = new FlagParameters();
                if (NameId != -1) { f[0] = (uint)NameId; }
                w.Write(f);
                warcs.WriteData(w);
            }

        }

    }

}

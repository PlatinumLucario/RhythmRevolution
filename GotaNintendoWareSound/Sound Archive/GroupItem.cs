using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Group item.
    /// </summary>
    public class GroupItem : ISoundArchiveItem {

        /// <summary>
        /// Name Id.
        /// </summary>
        public int NameId { get; set; }

        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Group file.
        /// </summary>
        public ItemReference<FileItem> File;

        /// <summary>
        /// Entries.
        /// </summary>
        public List<GroupEntry> Entries;

        /// <summary>
        /// Data offset.
        /// </summary>
        public uint DataOffset;

        /// <summary>
        /// Data size.
        /// </summary>
        public uint DataSize;

        /// <summary>
        /// Wave data offset.
        /// </summary>
        public uint WaveDataOffset;

        /// <summary>
        /// Wave data size.
        /// </summary>
        public uint WaveDataSize;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                Entries = r.Read<Table<GroupEntry>>();
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                File = new ItemReference<FileItem>() { Index = r.Read<IdType>().Id };
                DataOffset = r.ReadUInt32();
                DataSize = r.ReadUInt32();
                WaveDataOffset = r.ReadUInt32();
                WaveDataSize = r.ReadUInt32();
                r.ReadUInt32();
            }

            //CTR, Cafe, NX.
            else {
                File = new ItemReference<FileItem>() { Index = r.ReadInt32() };
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
                w.Write((Table<GroupEntry>)Entries);
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                w.Write(new IdType() { Id = (Int24)File.Index });
                w.Write(DataOffset);
                w.Write(DataSize);
                w.Write(WaveDataOffset);
                w.Write(WaveDataSize);
                w.Write((uint)0);
            }

            //CTR, Cafe, NX.
            else {
                w.Write(File.Index);
                var f = new FlagParameters();
                if (NameId != -1) { f[0] = (uint)NameId; }
                w.Write(f);
            }

        }

        /// <summary>
        /// Group entry.
        /// </summary>
        public class GroupEntry : IPlatformChangeable, IReadable, IWriteable {

            /// <summary>
            /// Sound type.
            /// </summary>
            public byte Type;

            /// <summary>
            /// Item.
            /// </summary>
            public ItemReference<ISoundArchiveItem> Item;

            /// <summary>
            /// Load sound.
            /// </summary>
            public bool LoadSound;

            /// <summary>
            /// Load sequence archive.
            /// </summary>
            public bool LoadSequenceArchive;

            /// <summary>
            /// Load bank.
            /// </summary>
            public bool LoadBank;

            /// <summary>
            /// Load wave archive.
            /// </summary>
            public bool LoadWaveArchive;

            /// <summary>
            /// Read the item.
            /// </summary>
            /// <param name="r">The reader.</param>
            /// <param name="platform">Platform.</param>
            public void Read(FileReader r, Platform platform) {

                //Nitro.
                if (platform == Platform.Nitro) {
                    Type = r.ReadByte();
                    byte flags = r.ReadByte();
                    LoadSound = (flags & 0b1) > 0;
                    LoadBank = (flags & 0b10) > 0;
                    LoadWaveArchive = (flags & 0b100) > 0;
                    LoadSequenceArchive = (flags & 0b1000) > 0;
                    r.ReadUInt16();
                    Item = new ItemReference<ISoundArchiveItem>() { Index = r.ReadInt32() };
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
                    w.Write(Type);
                    byte flags = 0;
                    if (LoadSound) { flags |= 0b1; }
                    if (LoadBank) { flags |= 0b10; }
                    if (LoadWaveArchive) { flags |= 0b100; }
                    if (LoadSequenceArchive) { flags |= 0b1000; }
                    w.Write(flags);
                    w.Write((ushort)0);
                    w.Write(Item.Index);
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
            /// Read.
            /// </summary>
            /// <param name="r">The reader.</param>
            public void Read(FileReader r) {
                Read(r, Platform.Nitro);
            }

            /// <summary>
            /// Write.
            /// </summary>
            /// <param name="w">The writer.</param>
            public void Write(FileWriter w) {
                Write(w, Platform.Nitro);
            }
            
        }

    }

}

using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {
    
    /// <summary>
    /// Can play data.
    /// </summary>
    public abstract class PlayerItem : ISoundArchiveItem {

        /// <summary>
        /// Name Id.
        /// </summary>
        public int NameId { get; set; }

        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// How many sounds can play at once.
        /// </summary>
        public uint PlayableSoundMax;

        /// <summary>
        /// Heap size of the player.
        /// </summary>
        public uint HeapSize;

        /// <summary>
        /// If to write the heap size.
        /// </summary>
        public bool WriteHeapSize = true;

        /// <summary>
        /// Allocated channels.
        /// </summary>
        public ushort AllocatedChannels = 0xFFFF;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">The platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                PlayableSoundMax = r.ReadByte();
                r.ReadByte();
                AllocatedChannels = r.ReadUInt16();
                if (AllocatedChannels == 0) { AllocatedChannels = 0xFFFF; }
                HeapSize = r.ReadUInt32();
                WriteHeapSize = HeapSize != 0;
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                NameId = r.Read<IdType>().Id;
                PlayableSoundMax = r.ReadByte();
                r.ReadBytes(3);
                HeapSize = r.ReadUInt32();
                WriteHeapSize = HeapSize != 0;
                r.ReadUInt32();
            }

            //CTR, Cafe, NX.
            else {
                PlayableSoundMax = r.ReadUInt32();
                var f = r.Read<FlagParameters>();
                NameId = (int)(f[0] ?? new uint?(0xFFFFFFFF));
                if (f[1] != null) {
                    WriteHeapSize = true;
                    HeapSize = f[1].Value;
                }
            }

        }

        /// <summary>
        /// Write the item.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="platform">The platform.</param>
        public void Write(FileWriter w, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {
                w.Write((byte)PlayableSoundMax);
                w.Write((byte)0);
                if (AllocatedChannels == 0xFFFF) { w.Write((ushort)0); } else { w.Write(AllocatedChannels); }
                w.Write(HeapSize);
            }

            //Revolution.
            else if (platform == Platform.Revolution) {
                w.Write(new IdType() { Id = (Int24)NameId });
                w.Write((byte)PlayableSoundMax);
                w.Write((Int24)0);
                w.Write(HeapSize);
                w.Write((uint)0);
            }

            //CTR, Cafe, NX.
            else {
                w.Write(PlayableSoundMax);
                var f = new FlagParameters();
                if (NameId != -1) { f[0] = (uint)NameId; }
                if (WriteHeapSize) { f[1] = HeapSize; }
            }

        }

    }
}

using GotaNintendoWare.Wave;
using GotaSoundIO;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare {

    /// <summary>
    /// Wave archive.
    /// </summary>
    public class WaveArchive : IOFile, IPlatformChangeable {

        /// <summary>
        /// Waves contained in the wave archive.
        /// </summary>
        public List<SoundFile> Waves = new List<SoundFile>();

        /// <summary>
        /// Convert the wave archive to a set of RIFF waves.
        /// </summary>
        /// <returns>RIFF waves.</returns>
        public RiffWave[] ToWaves() => Waves.ConvertAll(x => x.Convert<RiffWave>()).ToArray();

        /// <summary>
        /// Wii version.
        /// </summary>
        public readonly RVersion WII_VERSION = new RVersion() { Major = 1 };

        /// <summary>
        /// Version for other platforms.
        /// </summary>
        public readonly XVersion X_VERSION = new XVersion() { Major = 1 };

        /// <summary>
        /// Read the wave archive.
        /// </summary>
        /// <param name="r">File reader.</param>
        /// <param name="platform">Platform.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {

                //Read data.
                r.OpenFile<NFileHeader>(out _);
                r.OpenBlock(0, out _, out _, false);
                r.ReadUInt32();
                uint size = r.ReadUInt32();
                r.ReadUInt32s(8); //Reserved.
                var offs = r.Read<Table<uint>>();
                Waves = new List<SoundFile>();
                for (int i = 0; i < offs.Count; i++) {
                    uint len;
                    if (i == offs.Count - 1) {
                        len = size - (offs[i] - 0x10);
                    } else {
                        len = offs[i + 1] - offs[i];
                    }
                    r.Jump(offs[i], true);
                    Waves.Add(NitroWave.ReadShortened(r, len));
                }

            }

            //Revolution.
            else if (platform == Platform.Revolution) {

                //Read file header.
                r.OpenFile<RFileHeader>(out _);

                //Open the table block.
                r.OpenBlock(0, out _, out _);

                //Read the offsets.
                List<RReference<RevolutionWave>> offs = new List<RReference<RevolutionWave>>();
                uint num = r.ReadUInt32();
                for (uint i = 0; i < num; i++) {
                    RReference<RevolutionWave> rev = new RReference<RevolutionWave>();
                    rev.ReadRef(r);
                    offs.Add(rev);
                    r.ReadUInt32();
                }

                //Open the data block.
                r.OpenBlock(1, out _, out _, false);

                //Open the waves.
                Waves = new List<SoundFile>();
                foreach (var off in offs) {
                    off.ReadData(r);
                    Waves.Add(off.Data);
                }

            }

            //Other.
            else if (platform == Platform.Cafe || platform == Platform.CTR || platform == Platform.Revolution) {

                //Read header.
                r.OpenFile<XFileHeader>(out _);

                //Open the INFO block.
                r.OpenBlock(0, out _, out _);

                //Read the offsets.
                List<XRSizedReference<XWave>> offs = new List<XRSizedReference<XWave>>();
                uint num = r.ReadUInt32();
                for (uint i = 0; i < num; i++) {
                    XRSizedReference<XWave> rev = new XRSizedReference<XWave>();
                    rev.ReadRef(r);
                    offs.Add(rev);
                }

                //Open the FILE block.
                r.OpenBlock(1, out _, out _, false);

                //Open the waves.
                Waves = new List<SoundFile>();
                foreach (var off in offs) {
                    off.ReadData(r);
                    Waves.Add(off.Data);
                }

            }

        }

        /// <summary>
        /// Write the wave archive.
        /// </summary>
        /// <param name="w">File writer.</param>
        /// <param name="platform">Platform.</param>
        public void Write(FileWriter w, Platform platform) {
            
            //Nitro.
            if (platform == Platform.Nitro) {

                //Write the wave archive.
                if (Waves.Count > 0 && Waves[0] as NitroWave == null) {
                    Waves = Waves.ConvertAll(x => x.Convert(typeof(NitroWave)));
                }
                w.InitFile<NFileHeader>("SWAR", ByteOrder.LittleEndian, null, 1);
                w.InitBlock("DATA");
                w.Write(new uint[8]);
                w.Write((uint)Waves.Count());
                long bak = w.Position;
                w.Write(new uint[Waves.Count()]);
                for (int i = 0; i < Waves.Count(); i++) {
                    long bak2 = w.Position;
                    w.Position = bak + i * 4;
                    w.Write((uint)(bak2 - w.FileOffset));
                    w.Position = bak2;
                    (Waves[i] as NitroWave).WriteShortened(w);
                }
                w.Pad(4);
                w.CloseBlock();
                w.CloseFile();

            }

            //Revolution.
            else if (platform == Platform.Revolution) {

                //Convert files.
                if (Waves.Count > 0 && Waves[0] as RevolutionWave == null) {
                    Waves = Waves.ConvertAll(x => x.Convert(typeof(RevolutionWave)));
                }

                //Init the file.
                w.InitFile<RFileHeader>("RWAR", ByteOrder.BigEndian, WII_VERSION, 2);

                //Open the table block.
                w.InitBlock("TABL");

                //Init the references.
                w.Write((uint)Waves.Count);
                RReference<byte[]>[] waveFiles = new RReference<byte[]>[Waves.Count];
                for (int i = 0; i < waveFiles.Count(); i++) {
                    waveFiles[i] = new RReference<byte[]>() { Data = Waves[i].Write() };
                    waveFiles[i].InitWrite(w);
                    w.Write((uint)waveFiles[i].Data.Length);
                }

                //Close the block.
                w.Align(0x20);
                w.CloseBlock();

                //Open the data block.
                w.InitBlock("DATA", false);
                w.Write("DATA".ToCharArray());
                w.Write((uint)0);
                w.Align(0x20);
                for (int i = 0; i < waveFiles.Count(); i++) {
                    waveFiles[i].WriteData(w);
                    w.Align(0x20, true);
                }

                //Close the block.
                w.CloseBlock();

                //Close the file.
                w.CloseFile();

            }

            //Other.
            else if (platform == Platform.Cafe || platform == Platform.CTR || platform == Platform.Revolution) {

                //Convert files.
                if (Waves.Count > 0 && Waves[0] as XWave == null) {
                    Waves = Waves.ConvertAll(x => x.Convert(typeof(XWave)));
                }

                //Init the file.
                w.InitFile<XFileHeader>(platform == Platform.CTR ? "CWAR" : "FWAR", platform == Platform.Cafe ? ByteOrder.BigEndian : ByteOrder.LittleEndian, X_VERSION, 2);

                //Open the INFO block.
                w.InitBlock("TABL");

                //Init the references.
                w.Write((uint)Waves.Count);
                XReference<byte[]>[] waveFiles = new XReference<byte[]>[Waves.Count];
                for (int i = 0; i < waveFiles.Count(); i++) {
                    waveFiles[i] = new XReference<byte[]>() { Identifier = 0x1F00, Data = Waves[i].Write() };
                    waveFiles[i].InitWrite(w);
                    w.Write((uint)waveFiles[i].Data.Length);
                }

                //Close the block.
                w.Align(0x20);
                w.CloseBlock();

                //Open the FILE block.
                w.InitBlock("FILE", false);
                w.Write("FILE".ToCharArray());
                w.Write((uint)0);
                w.Align(0x20);
                for (int i = 0; i < waveFiles.Count(); i++) {
                    waveFiles[i].WriteData(w);
                    w.Align(0x20, true);
                }

                //Close the block.
                w.CloseBlock();

                //Close the file.
                w.CloseFile();

            }

        }

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            throw new Exception("You have to specify a platform to read this!");
        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            throw new Exception("You have to specify a platform to write this!");
        }

    }

}

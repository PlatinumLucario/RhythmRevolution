using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// A wave archive.
    /// </summary>
    public class WaveArchive : IOFile {

        /// <summary>
        /// Header.
        /// </summary>
        public FileHeader Header;

        /// <summary>
        /// Waves.
        /// </summary>
        public List<RevolutionWave> Waves = new List<RevolutionWave>();

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public WaveArchive() {}

        /// <summary>
        /// Create a wave archive from a file path.
        /// </summary>
        /// <param name="filePath">Path of the file.</param>
        public WaveArchive(string filePath) : base(filePath) {}

        /// <summary>
        /// Read the wave archive.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read file header.
            r.OpenFile<RFileHeader>(out Header);

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
            Waves = new List<RevolutionWave>();
            foreach (var off in offs) {
                off.ReadData(r);
                Waves.Add(off.Data);
            }

        }

        /// <summary>
        /// Write the wave archive.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Init the file.
            w.InitFile<RFileHeader>("RWAR", ByteOrder.BigEndian, new RVersion() { Major = 1 }, 2);

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

        /// <summary>
        /// Convert this wave archive to a standard wave array.
        /// </summary>
        /// <returns>Standard waves.</returns>
        public RiffWave[] GetWaves() {
            RiffWave[] ret = new RiffWave[Waves.Count];
            for (int i = 0; i < Waves.Count; i++) {
                ret[i] = new RiffWave();
                ret[i].FromOtherStreamFile(Waves[i]);
            }
            return ret;
        }

    }

}

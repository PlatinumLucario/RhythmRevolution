using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// A standard RIFF wave.
    /// </summary>
    public class RiffWave : StreamedAudioFile {

        /// <summary>
        /// Get the supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public override Type[] SupportedEncodings() => new Type[] { typeof(PCM16), typeof(PCM8) };

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public RiffWave() {}

        /// <summary>
        /// Create a new wave from a file.
        /// </summary>
        /// <param name="filePath">The file.</param>
        public RiffWave(string filePath) : base(filePath) {}

        /// <summary>
        /// Read the RIFF.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Get filesize.
            long fileStart = r.Position;
            r.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
            r.ReadUInt32();
            uint fileSize = r.ReadUInt32() + 8;
            r.ReadUInt32();

            //Read other blocks.
            while (!new string(r.ReadChars(4)).Equals("fmt ")) {
                int size = r.ReadInt32();
                r.ReadBytes(size);
            }
            r.Position -= 4;

            //Read fmt.
            long fmtStart = r.Position;
            r.ReadUInt32();
            uint fmtSize = r.ReadUInt32() + 8;
            if (r.ReadUInt16() != 1) { throw new Exception("Unexpected standard WAV data format."); }
            int numChannels = r.ReadUInt16();
            SampleRate = r.ReadUInt32();
            r.ReadUInt32(); //Byte rate.
            r.ReadUInt16(); //Blocks.
            ushort bitsPerSample = r.ReadUInt16();
            LoopStart = 0;
            LoopEnd = 0;
            Loops = false;
            if (bitsPerSample != 8 && bitsPerSample != 16) { throw new Exception("This tool only accepts 8-bit or 16-bit WAV files."); }

            //Read extra fmt.
            r.ReadBytes((int)fmtSize - 0x18);

            //Read other blocks.
            string next = new string(r.ReadChars(4));
            while (!next.Equals("data") && !next.Equals("smpl")) {
                int size = r.ReadInt32();
                r.ReadBytes(size);
            }
            r.Position -= 4;

            //Sample.
            if (next.Equals("smpl")) {

                //Read other blocks.
                next = new string(r.ReadChars(4));
                while (!next.Equals("data")) {
                    int size = r.ReadInt32();
                    r.ReadBytes(size);
                }
                r.Position -= 4;

            }

            //Init channels.
            Channels = new List<AudioEncoding>();
            if (bitsPerSample == 8) {
                for (int i = 0; i < numChannels; i++) {
                    Channels.Add(new PCM8());
                }
            } else {
                for (int i = 0; i < numChannels; i++) {
                    Channels.Add(new PCM16());
                }
            }

            //Data.
            r.ReadUInt32();
            uint dataSize = r.ReadUInt32();
            int numBlocks = (int)(dataSize / numChannels / (bitsPerSample / 8));
            for (int i = 0; i < numChannels; i++) {
                Channels[i].InitFromBlocks((uint)numBlocks, (uint)bitsPerSample / 8, 1, (uint)bitsPerSample / 8, 1);
            }
            for (int i = 0; i < numBlocks; i++) {
                for (int j = 0; j < numChannels; j++) {
                    Channels[j].ReadBlock(r, i, (uint)bitsPerSample / 8, 1);
                }
            }

        }

        /// <summary>
        /// Write the RIFF.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Init.
            w.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
            w.Write("RIFF".ToCharArray());
            w.InitOffset("riffSize");
            w.StartStructure();
            w.Write("WAVE".ToCharArray());

            //Fmt.
            w.Write("fmt ".ToCharArray());
            w.InitOffset("fmtSize");
            w.StartStructure();
            w.Write((ushort)1);
            w.Write((ushort)Channels.Count);
            w.Write(SampleRate);
            uint bitsPerSample = (Channels[0] as PCM16) != null ? 16u : 8u;
            w.Write((uint)(SampleRate * Channels.Count * (bitsPerSample / 8)));
            w.Write((ushort)(bitsPerSample / 8 * Channels.Count));
            w.Write((ushort)bitsPerSample);
            w.CloseOffset("fmtSize");
            w.EndStructure();

            //Smpl.

            //Data.
            w.Write("data".ToCharArray());
            w.InitOffset("dataSize");
            w.StartStructure();
            int numBlocks = (int)Channels[0].NumBlocks(bitsPerSample == 16 ? 2u : 1u, 1);
            for (int i = 0; i < numBlocks; i++) {
                for (int j = 0; j < Channels.Count; j++) {
                    Channels[j].WriteBlock(w, i, bitsPerSample / 8, 1);
                }
            }
            while (w.Position % 2 != 0) {
                w.Write((byte)0);
            }
            w.CloseOffset("dataSize");
            w.EndStructure();
            w.CloseOffset("riffSize");
            w.EndStructure();

        }

    }

}

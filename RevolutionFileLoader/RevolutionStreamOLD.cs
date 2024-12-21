using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// A stream file.
    /// </summary>
    public class RevolutionStreamOLD : SoundFile {

        /// <summary>
        /// Block size.
        /// </summary>
        public uint BlockSize = 0x2000;

        /// <summary>
        /// Supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public override Type[] SupportedEncodings() => new Type[] { typeof(DspAdpcm), typeof(PCM16), typeof(SignedPCM8) };

        /// <summary>
        /// Name.
        /// </summary>
        /// <returns>The name.</returns>
        public override string Name() => "BRSTM";

        /// <summary>
        /// Extensions.
        /// </summary>
        /// <returns>The extensions.</returns>
        public override string[] Extensions() => new string[] { "BRSTM" };

        /// <summary>
        /// Description.
        /// </summary>
        /// <returns>The description.</returns>
        public override string Description() => "A revolution stream used in many Wii games.";

        /// <summary>
        /// If the file supports tracks.
        /// </summary>
        /// <returns>It doesn't.</returns>
        public override bool SupportsTracks() => true;

        /// <summary>
        /// Preferred encoding.
        /// </summary>
        /// <returns>The preferred encoding.</returns>
        public override Type PreferredEncoding() => typeof(DspAdpcm);

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public RevolutionStream() {
            Version = new RVersion() { Major = 1 };
        }

        /// <summary>
        /// Read a BRSTM from a file path.
        /// </summary>
        /// <param name="filePath">The path to read from.</param>
        public RevolutionStream(string filePath) : base(filePath) {
            Version = new RVersion() { Major = 1 };
        }

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read header.
            FileHeader header;
            r.OpenFile<RSTMFileHeader>(out header);
            Version = header.Version;

            //Head block.
            r.OpenBlock(0, out _, out _);
            r.OpenReference<RReference<object>>("strmInfo");
            r.OpenReference<RReference<object>>("trackTable");
            r.OpenReference<RReference<object>>("channelTable");

            //Stream info.
            r.JumpToReference("strmInfo");
            PcmFormat format = (PcmFormat)r.ReadByte();
            Loops = r.ReadBoolean();
            int numChannels = r.ReadByte();
            SampleRate = (uint)((r.ReadByte() << 24) | r.ReadUInt16());
            ushort blockHeaderOffset = r.ReadUInt16();
            LoopStart = r.ReadUInt32();
            LoopEnd = r.ReadUInt32();
            r.OpenOffset("dataOffset");
            uint numBlocks = r.ReadUInt32();
            uint blockSize = BlockSize = r.ReadUInt32();
            uint blockSamples = r.ReadUInt32();
            uint lastBlockSize = r.ReadUInt32();
            uint lastBlockSamples = r.ReadUInt32();
            uint lastBlockPaddedSize = r.ReadUInt32();
            uint adpcmDataInterval = blockSamples;
            uint adpcmDataSize = 0x4;
            if (Version >= new RVersion() { Major = 1 }) {
                adpcmDataInterval = r.ReadUInt32();
                adpcmDataSize = r.ReadUInt32();
            }

            //Track info.
            if (!r.ReferenceNull("trackTable")) {
                r.JumpToReference("trackTable");
                byte numTracks = r.ReadByte();
                bool isEx = r.ReadBoolean();
                r.ReadUInt16();
                Tracks = new List<TrackData>();
                for (int i = 0; i < numTracks; i++) {
                    Tracks.Add(r.Read<TrackReference>());
                }
            }

            //Channel info.
            r.JumpToReference("channelTable");
            numChannels = r.ReadByte();
            r.ReadBytes(3);
            Channels = new List<AudioEncoding>();
            if (format == PcmFormat.DspAdpcm) {
                for (int i = 0; i < numChannels; i++) {
                    r.OpenReference<RReference<object>>("channel" + i);
                }
                for (int i = 0; i < numChannels; i++) {
                    Channels.Add(new DspAdpcm());
                    r.JumpToReference("channel" + i);
                    (Channels[i] as DspAdpcm).LoopStart = LoopStart;
                    r.OpenReference<RReference<object>>("dspInfo");
                    r.JumpToReference("dspInfo");
                    (Channels[i] as DspAdpcm).Context = r.Read<DspAdpcmContext>();
                }
            } else {
                for (int i = 0; i < numChannels; i++) {
                    Channels.Add(format == PcmFormat.PCM16 ? (AudioEncoding)new PCM16() : new SignedPCM8());
                }
            }

            //ADPC block.
            if (format == PcmFormat.DspAdpcm && header.BlockOffsets[1] != 0) {
                uint adpcSize = 0;
                r.OpenBlock(1, out _, out adpcSize);
                adpcSize -= 8;
                ReadSeek(r, adpcSize, adpcmDataInterval, (int)(blockSamples * (numBlocks - 1) + lastBlockSamples));
            }

            //Data block.
            r.OpenBlock(header.BlockOffsets.Length - 1, out _, out _);
            r.Position += r.ReadUInt32();
            ReadChannels(r, numChannels, numBlocks, blockSize, blockSamples, lastBlockSize, lastBlockSamples, lastBlockPaddedSize - lastBlockSize);

        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Write header.
            w.InitFile<RSTMFileHeader>("RSTM", ByteOrder.BigEndian, Version, 3);

            //Head block.
            w.InitBlock("HEAD");

            //Get format.
            PcmFormat format = PcmFormat.DspAdpcm;
            if (Channels[0] as PCM16 != null) { format = PcmFormat.PCM16; }
            if (Channels[0] as SignedPCM8 != null) { format = PcmFormat.SignedPCM8; }

            //Get block data.
            uint blockSize = BlockSize;
            uint blockSamples = 0x3800;
            uint adpcInterval = 0x3800;
            uint adpcSize = 0x4;
            switch (format) {
                case PcmFormat.SignedPCM8:
                    blockSamples = blockSize;
                    adpcSize = 0;
                    adpcInterval = 0;
                    break;
                case PcmFormat.PCM16:
                    blockSamples = blockSize / 2;
                    adpcSize = 0;
                    adpcInterval = 0;
                    break;
                case PcmFormat.DspAdpcm:
                    blockSamples = blockSize / 16 * 14 * 2;
                    adpcInterval = blockSamples;
                    break;
            }
            uint numBlocks = Channels[0].NumBlocks(blockSize, blockSamples);
            uint lastBlockSize = Channels[0].LastBlockSize(blockSize, blockSamples);
            uint lastBlockSamples = Channels[0].LastBlockSamples(blockSize, blockSamples);
            uint lastBlockPaddedSize = lastBlockSize;
            while (lastBlockPaddedSize % 0x20 != 0) {
                lastBlockPaddedSize++;
            }

            //References.
            w.InitReference<RReference<object>>("strmInfo");
            w.InitReference<RReference<object>>("trackTable");
            w.InitReference<RReference<object>>("channelTable");

            //Stream info.
            w.CloseReference("strmInfo");
            w.Write((byte)format);
            w.Write(Loops);
            w.Write((byte)Channels.Count);
            w.Write((byte)((SampleRate & 0xFF0000) >> 24));
            w.Write((ushort)(SampleRate & 0xFFFF));
            w.Write((ushort)0); //Might always just be 0 though.
            w.Write(LoopStart);
            w.Write(LoopEnd);
            w.InitOffset("dataOff");
            w.Write(numBlocks);
            w.Write(blockSize);
            w.Write(blockSamples);
            w.Write(lastBlockSize);
            w.Write(lastBlockSamples);
            w.Write(lastBlockPaddedSize);
            if (Version >= new RVersion() { Major = 1 }) {
                w.Write(adpcInterval);
                w.Write(adpcSize);
            }

            //Track info.
            if (Tracks == null) {
                w.CloseReference("trackTable", 0, false, 0, 0);
            } else {
                w.CloseReference("trackTable");
                bool isEx = Tracks.Count == 0 ? false : Tracks[0] as TrackEx != null;
                w.Write((byte)Tracks.Count);
                w.Write(isEx);
                w.Write((ushort)0);
                for (int i = 0; i < Tracks.Count; i++) {
                    w.InitReference<RReference<object>>("track" + i);
                }
                for (int i = 0; i < Tracks.Count; i++) {
                    w.CloseReference("track" + i, isEx ? 1 : 0);
                    w.Write(Tracks[i] as Track);
                }
            }

            //Channel info.
            w.CloseReference("channelTable");
            w.Write((byte)Channels.Count);
            w.Write(new byte[3]);
            for (int i = 0; i < Channels.Count; i++) {
                w.InitReference<RReference<object>>("chan" + i);
            }
            for (int i = 0; i < Channels.Count; i++) {
                w.CloseReference("chan" + i);
                w.InitReference<RReference<object>>("adpcm");
                if (format == PcmFormat.DspAdpcm) {
                    w.CloseReference("adpcm");
                    w.Write((Channels[i] as DspAdpcm).Context);
                    w.Align(0x4);
                } else {
                    w.CloseReference("adpcm", 0, false, 0, 0);
                }
            }

            //Close head.
            w.Align(0x20);
            w.CloseBlock();

            //Adpc block.
            if (format == PcmFormat.DspAdpcm) {
                w.InitBlock("ADPC");
                WriteSeek(w, blockSamples, (int)(blockSamples * (numBlocks - 1) + lastBlockSamples));
                w.CloseBlock();
            } else {
                w.InitBlock("ADPC", false, false);
                w.CloseBlock(false);
                w.BlockOffsets[1] = 0;
            }

            //Data block.
            w.CloseOffset("dataOff", true, w.Position - w.FileOffset + 0x20);
            w.InitBlock("DATA");
            w.Write((uint)0x18);
            w.Align(0x20);
            WriteChannels(w, blockSize, blockSamples, lastBlockPaddedSize - lastBlockSize);
            w.Align(0x20);
            w.CloseBlock();

            //Close file.
            w.CloseFile();

        }

        /// <summary>
        /// On conversion.
        /// </summary>
        public override void OnConversion() {
            PcmFormat format = PcmFormat.DspAdpcm;
            if (Channels[0] as PCM16 != null) { format = PcmFormat.PCM16; }
            if (Channels[0] as SignedPCM8 != null) { format = PcmFormat.SignedPCM8; }
            uint blockSamples = 0x3800;
            switch (format) {
                case PcmFormat.SignedPCM8:
                    blockSamples = BlockSize;
                    break;
                case PcmFormat.PCM16:
                    blockSamples = BlockSize / 2;
                    break;
                case PcmFormat.DspAdpcm:
                    BlockSize = (Channels[0] as DspAdpcm).SeekInterval / 14 / 2 * 16;
                    blockSamples = BlockSize / 16 * 14 * 2;
                    break;
            }
            AlignLoopToBlock(blockSamples);
            if (LoopEnd == 0) { LoopEnd = (Channels[0].NumBlocks(BlockSize, blockSamples) - 1) * blockSamples + Channels[0].LastBlockSamples(BlockSize, blockSamples) - 1; }
            var tracks = Tracks;
            Tracks = new List<TrackData>();
            if (tracks == null) { Tracks = null; } else {
                foreach (var t in tracks) {
                    Tracks.Add(new TrackEx() { Channels = t.Channels });
                }
            }
        }

        /// <summary>
        /// Track data.
        /// </summary>
        public class Track : TrackData, IReadable, IWriteable {

            /// <summary>
            /// Read data.
            /// </summary>
            /// <param name="r">The reader.</param>
            public virtual void Read(FileReader r) {
                Channels = r.ReadBytes(r.ReadByte()).ConvertTo<int>().ToList();
                r.Align(0x4);
            }

            /// <summary>
            /// Write data.
            /// </summary>
            /// <param name="w">The writer.</param>
            public virtual void Write(FileWriter w) {
                w.Write((byte)Channels.Count);
                w.Write(Channels.ConvertTo<byte>().ToArray());
                w.Align(0x4);
            }

        }

        /// <summary>
        /// Track with extra data.
        /// </summary>
        public class TrackEx : Track {

            /// <summary>
            /// Track volume.
            /// </summary>
            public byte Volume = 127;

            /// <summary>
            /// Track panning.
            /// </summary>
            public byte Pan = 64;

            /// <summary>
            /// Read data.
            /// </summary>
            /// <param name="r">The reader.</param>
            public override void Read(FileReader r) {
                Volume = r.ReadByte();
                Pan = r.ReadByte();
                r.ReadBytes(6);
                base.Read(r);
            }

            /// <summary>
            /// Write data.
            /// </summary>
            /// <param name="w">The writer.</param>
            public override void Write(FileWriter w) {
                w.Write(Volume);
                w.Write(Pan);
                w.Write(new byte[6]);
                base.Write(w);
            }

        }

        /// <summary>
        /// Track reference.
        /// </summary>
        public class TrackReference : RReference<Track> {
            public override List<Type> DataTypes => new List<Type>() { typeof(Track), typeof(TrackEx) };
        }

    }

}

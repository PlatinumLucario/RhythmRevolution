using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Wave sound data.
    /// </summary>
    public class WaveSoundData : IOFile {

        /// <summary>
        /// Version that fixes a parameter bug and changes the wave block.
        /// </summary>
        public static RVersion VersionFixParamBugAndChangeWaveBlock = new RVersion() { Major = 1, Minor = 1 };

        /// <summary>
        /// Version that adds send data.
        /// </summary>
        public static RVersion VersionSendData = new RVersion() { Major = 1, Minor = 2 };

        /// <summary>
        /// Version that uses wave archives.
        /// </summary>
        public static RVersion VersionUseWaveArchive = new RVersion() { Major = 1, Minor = 3 };

        /// <summary>
        /// Wave sound entries.
        /// </summary>
        public List<WaveSoundEntry> Entries = new List<WaveSoundEntry>();

        /// <summary>
        /// Waves.
        /// </summary>
        public List<RevolutionWave.WaveBlockBody> Waves = new List<RevolutionWave.WaveBlockBody>();

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public WaveSoundData() {
            Version = VersionUseWaveArchive;
        }

        /// <summary>
        /// Read from a file.
        /// </summary>
        /// <param name="filePath">The reader.</param>
        public WaveSoundData(string filePath) : base(filePath) {}

        /// <summary>
        /// Read the wave sound data.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read the file.
            FileHeader header;
            r.OpenFile<RFileHeader>(out header);
            Version = header.Version;

            //Data block.
            r.OpenBlock(0, out _, out _);

            //Table of WSD references.
            Entries = r.Read<Table<RReference<WaveSoundEntry>>>().Select(x => x.Data).ToList();

            //Fix entries.
            foreach (var e in Entries) {
                if (e != null) {
                    if (e.Info != null) {
                        if (Version < VersionFixParamBugAndChangeWaveBlock) {
                            e.Info.Pan = 64;
                            e.Info.SurroundPan = 0;
                            e.Info.Pitch = 1;
                        }
                        if (Version < VersionSendData) {
                            e.Info.FxSendA = 0;
                            e.Info.FxSendB = 0;
                            e.Info.FxSendC = 0;
                            e.Info.MainSend = 127;
                        }
                    }
                    if (e.NoteInfo != null) {
                        foreach (var n in e.NoteInfo) {
                            if (n != null) {
                                if (Version < VersionFixParamBugAndChangeWaveBlock) {
                                    n.Pan = 64;
                                    n.SurroundPan = 0;
                                    n.Pitch = 1;
                                }
                            }
                        }
                    }
                }
            }

            //Get waves.
            if (Version < VersionUseWaveArchive) {

                //Open block.
                r.OpenBlock(1, out _, out _, false);
                r.ReadUInt64();

                //I don't know how many waves there are.
                if (Version < VersionFixParamBugAndChangeWaveBlock) {
                    throw new Exception("Unable to determine the amount of waves due to file version, please send this to Gota7.");
                }

                //Read waves.
                Waves = r.Read<Table<ROffset<RevolutionWave.WaveBlockBody>>>().Select(x => x.Data).ToList();

            }

        }

        /// <summary>
        /// Write the wave sound data.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Init the file.
            w.InitFile<RFileHeader>("RWSD", ByteOrder.BigEndian, Version, (RVersion)Version >= VersionUseWaveArchive ? 2 : 1);

            //Fix entries.
            foreach (var e in Entries) {
                if (e != null) {
                    if (e.Info != null) {
                        if (Version < VersionFixParamBugAndChangeWaveBlock) {
                            e.Info.Pan = 64;
                            e.Info.SurroundPan = 0;
                            e.Info.Pitch = 1;
                        }
                        if (Version < VersionSendData) {
                            e.Info.FxSendA = 0;
                            e.Info.FxSendB = 0;
                            e.Info.FxSendC = 0;
                            e.Info.MainSend = 127;
                        }
                    }
                    if (e.NoteInfo != null) {
                        foreach (var n in e.NoteInfo) {
                            if (n != null) {
                                if (Version < VersionFixParamBugAndChangeWaveBlock) {
                                    n.Pan = 64;
                                    n.SurroundPan = 0;
                                    n.Pitch = 1;
                                }
                            }
                        }
                    }
                }
            }

            //Data block.
            w.InitBlock("DATA");
            Table<RReference<WaveSoundEntry>> entriesToWrite = new Table<RReference<WaveSoundEntry>>();
            foreach (var e in Entries) {
                entriesToWrite.Add(new RReference<WaveSoundEntry>() { Data = null });
            }
            w.Write(entriesToWrite);
            for (int i = 0; i < Entries.Count; i++) {
                entriesToWrite[i].Data = Entries[i];
                entriesToWrite[i].WriteData(w);
            }
            w.Align(0x20);
            w.CloseBlock();

            //Wave block.
            if (Version < VersionUseWaveArchive) {
                w.InitBlock("WAVE", false);
                w.Write("WAVE".ToCharArray());
                w.Write((uint)0);
                var t = new Table<ROffset<RevolutionWave.WaveBlockBody>>();
                foreach (var wav in Waves) {
                    t.Add(new ROffset<RevolutionWave.WaveBlockBody>() { Data = wav });
                }
                w.Write(t);
                w.Align(0x20);
                w.CloseBlock();
            }

            //Close the file.
            w.CloseFile();

        }

    }

}

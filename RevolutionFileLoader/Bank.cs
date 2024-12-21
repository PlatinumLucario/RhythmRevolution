using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// A bank file.
    /// </summary>
    public class Bank : IOFile, PlayableBank {

        /// <summary>
        /// Version that adds instrument volume.
        /// </summary>
        public static RVersion VersionAddInstrumentVolume = new RVersion() { Major = 1, Minor = 1 };

        /// <summary>
        /// Version that uses wave archives instead.
        /// </summary>
        public static RVersion VersionUseWaveArchive = new RVersion() { Major = 1, Minor = 2 };

        /// <summary>
        /// Instruments.
        /// </summary>
        public List<Instrument> Instruments = new List<Instrument>();

        /// <summary>
        /// Waves.
        /// </summary>
        public List<RevolutionWave.WaveBlockBody> Waves = new List<RevolutionWave.WaveBlockBody>();

        /// <summary>
        /// Create a new bank.
        /// </summary>
        public Bank() {
            Version = VersionUseWaveArchive;
        }

        /// <summary>
        /// Create a new bank from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public Bank(string filePath) : base(filePath) {}

        /// <summary>
        /// Read the bank.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Open the file.
            FileHeader header;
            r.OpenFile<RFileHeader>(out header);
            Version = header.Version;

            //Instrument reference table.
            r.OpenBlock(0, out _, out _);
            Instruments = r.Read<Table<InstrumentRef>>().Select(x => x.Data).ToList();
            for (int i = 0; i < Instruments.Count; i++) {
                if (Instruments[i] != null) {
                    Instruments[i].Index = i;
                }
            }
            Instruments = Instruments.Where(x => x != null).ToList();

            //Get waves.
            if (Version < VersionUseWaveArchive) {
                r.OpenBlock(1, out _, out _);
                Waves = r.Read<Table<RReference<RevolutionWave.WaveBlockBody>>>().Select(x => x.Data).ToList();
            }

        }

        /// <summary>
        /// Write the bank.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Write the file.
            w.InitFile<RFileHeader>("RBNK", ByteOrder.BigEndian, Version, Version >= VersionUseWaveArchive ? 1 : 2);

            //Data block.
            w.InitBlock("DATA");
            Instruments = Instruments.OrderBy(x => x.Index).ToList();
            if (Instruments.Count != 0) {
                int count = Instruments.Last().Index + 1;
                w.Write((uint)count);
                List<InstrumentRef> instRefs = new List<InstrumentRef>();
                for (int i = 0; i < count; i++) {
                    instRefs.Add(new InstrumentRef());
                    instRefs.Last().InitWrite(w);
                }
                for (int i = 0; i < count; i++) {
                    var inst = Instruments.Where(x => x.Index == i).FirstOrDefault();
                    if (inst != null) {
                        instRefs[i].Data = inst;
                    }
                    instRefs[i].WriteData(w);
                }
            } else {
                w.Write((uint)0);
            }
            w.Align(0x20);
            w.CloseBlock();

            //Wave block.
            if (Version < VersionUseWaveArchive) {
                w.InitBlock("WAVE");
                Table<RReference<RevolutionWave.WaveBlockBody>> waves = new Table<RReference<RevolutionWave.WaveBlockBody>>();
                for (int i = 0; i < Waves.Count; i++) {
                    waves.Add(new RReference<RevolutionWave.WaveBlockBody>());
                }
                w.Write(waves);
                for (int i = 0; i < Waves.Count; i++) {
                    waves[i].Data = Waves[i];
                    waves[i].WriteData(w);
                    w.Align(0x4);
                }
                w.Align(0x20);
                w.CloseBlock();
            }

            //Close the file.
            w.CloseFile();

        }

        /// <summary>
        /// Get note playback info.
        /// </summary>
        /// <param name="program">Program number.</param>
        /// <param name="note">Note.</param>
        /// <param name="velocity">Velocity.</param>
        /// <returns>Note playback info.</returns>
        public NotePlayBackInfo GetNotePlayBackInfo(int program, Notes note, byte velocity) {

            //Instrument.
            var inst = Instruments.Where(x => x.Index == program).FirstOrDefault();
            if (inst != null) {
                for (int i = 0; i < inst.Regions.Count; i++) {
                    if ((int)note >= inst.Regions[i].MinKey && (int)note <= inst.Regions[i].MaxKey) {
                        var p = inst.Regions[i].GetNoteParameter(velocity);
                        if (p == null) { return null; }
                        return new NotePlayBackInfo() { Attack = p.Attack, BaseKey = (byte)p.OriginalKey, Decay = p.Decay, Hold = p.Hold, InstrumentType = GotaSequenceLib.Playback.InstrumentType.PCM, Pan = p.Pan, Release = p.Release, Sustain = p.Sustain, WarId = 0, WaveId = p.WaveId, Volume = p.Volume, KeyGroup = p.KeyGroup, PercussionMode = p.PercussionMode, Tune = p.Tune };
                    }
                }
            }

            //Default.
            return null;

        }

    }

    /// <summary>
    /// Instrument reference.
    /// </summary>
    public class InstrumentRef : RReference<Instrument> {
        public override List<Type> DataTypes => new List<Type>() { null, typeof(DirectInstrument), typeof(RangeInstrument), typeof(IndexInstrument) };
    }

}

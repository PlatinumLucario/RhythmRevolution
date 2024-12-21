using GotaNintendoWare.SoundArchive;
using GotaSequenceLib;
using GotaSequenceLib.Playback;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare {

    /// <summary>
    /// A bank.
    /// </summary>
    public abstract class Bank : IOFile, PlayableBank, IPlatformChangeable {

        /// <summary>
        /// Instruments.
        /// </summary>
        public List<Instrument> Instruments = new List<Instrument>();

        /// <summary>
        /// Wave map.
        /// </summary>
        public List<WaveReference> WaveMap = new List<WaveReference>();

        /// <summary>
        /// Get note playback info.
        /// </summary>
        /// <param name="program">Program number.</param>
        /// <param name="note">Note.</param>
        /// <param name="velocity">Velocity.</param>
        /// <returns>Note playback info.</returns>
        public NotePlayBackInfo GetNotePlayBackInfo(int program, Notes note, byte velocity) {

            //Instrument.
            var inst = Instruments.Where(x => x.Program == program).FirstOrDefault();
            if (inst == null) { return null; }

            //Key.
            foreach (var k in inst.Keys) {

                //Match.
                if ((byte)note >= k.Start && (byte)note <= k.End) {

                    //Velocity.
                    foreach (var v in k.Velocities) {

                        //Match.
                        if (velocity >= v.Start && velocity <= v.End) {

                            //Switch instrument type.
                            switch (v.InstrumentType) {
                                case InstrumentType.PCM:
                                    return new NotePlayBackInfo() {
                                        Attack = v.NoteInfo.Attack,
                                        BaseKey = (byte)v.NoteInfo.OriginalKey,
                                        Decay = v.NoteInfo.Decay,
                                        Hold = v.NoteInfo.Hold,
                                        InstrumentType = GotaSequenceLib.Playback.InstrumentType.PCM,
                                        //IsLinearInterpolation = v.NoteInfo.InterPolationType == NoteInfo.EInterPolationType.Linear,
                                        KeyGroup = v.NoteInfo.KeyGroup,
                                        Pan = (byte)v.NoteInfo.Pan,
                                        PercussionMode = v.NoteInfo.PercussionMode,
                                        Release = v.NoteInfo.Release,
                                        //SurroundPan = v.NoteInfo.SurroundPan,
                                        Sustain = v.NoteInfo.Sustain,
                                        Tune = v.NoteInfo.Pitch,
                                        Volume = v.NoteInfo.Volume,
                                        WarId = v.NoteInfo.WaveId.Item.WaveArchive.Index,
                                        WaveId = v.NoteInfo.WaveId.Item.SoundFile.Index
                                    };
                                case InstrumentType.PSG:
                                    return new NotePlayBackInfo() {
                                        Attack = v.NoteInfo.Attack,
                                        BaseKey = (byte)v.NoteInfo.OriginalKey,
                                        Decay = v.NoteInfo.Decay,
                                        Hold = v.NoteInfo.Hold,
                                        InstrumentType = GotaSequenceLib.Playback.InstrumentType.PSG,
                                        //IsLinearInterpolation = v.NoteInfo.InterPolationType == NoteInfo.EInterPolationType.Linear,
                                        KeyGroup = v.NoteInfo.KeyGroup,
                                        Pan = (byte)v.NoteInfo.Pan,
                                        PercussionMode = v.NoteInfo.PercussionMode,
                                        Release = v.NoteInfo.Release,
                                        //SurroundPan = v.NoteInfo.SurroundPan,
                                        Sustain = v.NoteInfo.Sustain,
                                        Tune = v.NoteInfo.Pitch,
                                        Volume = v.NoteInfo.Volume,
                                        WaveId = v.NoteInfo.PSGDuty
                                    };
                                case InstrumentType.Noise:
                                    return new NotePlayBackInfo() {
                                        Attack = v.NoteInfo.Attack,
                                        BaseKey = (byte)v.NoteInfo.OriginalKey,
                                        Decay = v.NoteInfo.Decay,
                                        Hold = v.NoteInfo.Hold,
                                        InstrumentType = GotaSequenceLib.Playback.InstrumentType.Noise,
                                        //IsLinearInterpolation = v.NoteInfo.InterPolationType == NoteInfo.EInterPolationType.Linear,
                                        KeyGroup = v.NoteInfo.KeyGroup,
                                        Pan = (byte)v.NoteInfo.Pan,
                                        PercussionMode = v.NoteInfo.PercussionMode,
                                        Release = v.NoteInfo.Release,
                                        //SurroundPan = v.NoteInfo.SurroundPan,
                                        Sustain = v.NoteInfo.Sustain,
                                        Tune = v.NoteInfo.Pitch,
                                        Volume = v.NoteInfo.Volume
                                    };
                                case InstrumentType.DirectPCM:
                                    return new NotePlayBackInfo() {
                                        Attack = v.NoteInfo.Attack,
                                        BaseKey = (byte)v.NoteInfo.OriginalKey,
                                        Decay = v.NoteInfo.Decay,
                                        Hold = v.NoteInfo.Hold,
                                        InstrumentType = GotaSequenceLib.Playback.InstrumentType.PCM,
                                        //IsLinearInterpolation = v.NoteInfo.InterPolationType == NoteInfo.EInterPolationType.Linear,
                                        KeyGroup = v.NoteInfo.KeyGroup,
                                        Pan = (byte)v.NoteInfo.Pan,
                                        PercussionMode = v.NoteInfo.PercussionMode,
                                        Release = v.NoteInfo.Release,
                                        //SurroundPan = v.NoteInfo.SurroundPan,
                                        Sustain = v.NoteInfo.Sustain,
                                        Tune = v.NoteInfo.Pitch,
                                        Volume = v.NoteInfo.Volume,
                                        WarId = v.NoteInfo.WaveId.Item.WaveArchive.Index,
                                        WaveId = v.NoteInfo.WaveId.Item.SoundFile.Index
                                    };
                                case InstrumentType.Blank:
                                    return null;
                            }

                        }

                    }

                    //Default.
                    return null;

                }

            }

            //Default.
            return null;

        }

        /// <summary>
        /// Read the bank.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform to read.</param>
        public void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {

                //Open block.
                r.OpenFile<NFileHeader>(out _);
                r.OpenBlock(0, out _, out _);

                //Read info.
                r.ReadUInt32s(8);
                uint numInsts = r.ReadUInt32();
                List<byte> records = new List<byte>();
                List<uint> offs = new List<uint>();
                for (uint i = 0; i < numInsts; i++) {
                    records.Add(r.ReadByte());
                    offs.Add(r.ReadUInt16());
                    r.ReadByte();
                }

                //Read the instrument.
                for (int i = 0; i < records.Count; i++) {

                    //Switch the instrument type.
                    switch (records[i]) {

                        //Blank.
                        case 0:
                            break;

                        //Direct.
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            r.Jump(offs[i], true);
                            Instruments.Add(ReadDirect(records[i]));
                            Instruments[Instruments.Count - 1].Program = i;
                            Instruments[Instruments.Count - 1].Order = (int)offs[i];
                            break;

                        //Drum set.
                        case 0x10:
                            r.Jump(offs[i], true);
                            //Instruments.Add(r.Read<DrumSetInstrument>());
                            Instruments[Instruments.Count - 1].Program = i;
                            Instruments[Instruments.Count - 1].Order = (int)offs[i];
                            break;

                        //Key split.
                        case 0x11:
                            r.Jump(offs[i], true);
                            //Instruments.Add(r.Read<KeySplitInstrument>());
                            Instruments[Instruments.Count - 1].Program = i;
                            Instruments[Instruments.Count - 1].Order = (int)offs[i];
                            break;

                    }

                }

                //Read note info.
                NoteInfo ReadNoteInfo(byte programType) {
                    NoteInfo n = new NoteInfo();
                    if (programType == 0) {  }
                    return n;
                }

                //Read a direct instrument.
                Instrument ReadDirect(byte programType) {
                    var ret = new Instrument() { RegionType = RegionType.Direct, Keys = new List<KeyRegion>() { new KeyRegion() { RegionType = RegionType.Direct, Velocities = new List<VelocityRegion>() { new VelocityRegion() { NoteInfo = ReadNoteInfo(programType) } } } } };
                    switch (programType) {
                        case 1:
                            ret.Keys[0].Velocities[0].InstrumentType = InstrumentType.PCM;
                            break;
                        case 2:
                            ret.Keys[0].Velocities[0].InstrumentType = InstrumentType.PSG;
                            break;
                        case 3:
                            ret.Keys[0].Velocities[0].InstrumentType = InstrumentType.Noise;
                            break;
                        case 4:
                            ret.Keys[0].Velocities[0].InstrumentType = InstrumentType.DirectPCM;
                            break;
                        case 5:
                            ret.Keys[0].Velocities[0].InstrumentType = InstrumentType.Blank;
                            break;
                    }
                    return ret;
                }

                //Read an index instrument.
                Instrument ReadIndex(byte programType) {

                    //Ret.
                    Instrument ret = new Instrument();

                    //Get indices.
                    List<byte> indices = new List<byte>();
                    for (int i = 0; i < 8; i++) {
                        byte b = r.ReadByte();
                        if (b != 0) {
                            indices.Add(b);
                        }
                    }

                    //Read note parameters.
                    for (int i = 0; i < indices.Count; i++) {

                        //Get the instrument type.
                        InstrumentType t = (InstrumentType)r.ReadUInt16();
                        //ret.Add(r.Read<NoteInfo>());
                        //NoteInfo.Last().Key = (Notes)indices[i];
                        //NoteInfo.Last().InstrumentType = t;

                    }

                    //Return data.
                    return ret;

                }

            }

            //Revolution.
            else if (platform == Platform.Revolution) {
               
            }

            //CTR, Cafe, NX.
            else {
                
            }

        }

        /// <summary>
        /// Write the bank.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="platform">Platform to write.</param>
        public void Write(FileWriter w, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {

            }

            //Revolution.
            else if (platform == Platform.Revolution) {

            }

            //CTR, Cafe, NX.
            else {

            }

        }

        /// <summary>
        /// Region type.
        /// </summary>
        public enum RegionType {
            Null,
            Direct,
            Range,
            Index
        }

        /// <summary>
        /// Instrument type.
        /// </summary>
        public enum InstrumentType {
            PCM,
            PSG,
            Noise,
            DirectPCM,
            Blank
        }

        /// <summary>
        /// Reference to a wave file.
        /// </summary>
        public class WaveReference {

            /// <summary>
            /// Wave archive reference.
            /// </summary>
            public ItemReference<WaveArchive> WaveArchive;

            /// <summary>
            /// Wave reference.
            /// </summary>
            public ItemReference<SoundFile> SoundFile;

        }

        /// <summary>
        /// Instrument.
        /// </summary>
        public class Instrument {

            /// <summary>
            /// Program number.
            /// </summary>
            public int Program;

            /// <summary>
            /// Order for writing.
            /// </summary>
            public int Order;

            /// <summary>
            /// Region type.
            /// </summary>
            public RegionType RegionType;

            /// <summary>
            /// Key list.
            /// </summary>
            public List<KeyRegion> Keys = new List<KeyRegion>();

        }

        /// <summary>
        /// Key region.
        /// </summary>
        public class KeyRegion {

            /// <summary>
            /// Start key.
            /// </summary>
            public byte Start = 0;

            /// <summary>
            /// End key.
            /// </summary>
            public byte End = 127;

            /// <summary>
            /// Region type.
            /// </summary>
            public RegionType RegionType;

            /// <summary>
            /// Velocities.
            /// </summary>
            public List<VelocityRegion> Velocities = new List<VelocityRegion>();

        }

        /// <summary>
        /// Velocity region.
        /// </summary>
        public class VelocityRegion {

            /// <summary>
            /// Start key.
            /// </summary>
            public byte Start = 0;

            /// <summary>
            /// End key.
            /// </summary>
            public byte End = 127;

            /// <summary>
            /// Instrument type.
            /// </summary>
            public InstrumentType InstrumentType;

            /// <summary>
            /// Note info.
            /// </summary>
            public NoteInfo NoteInfo = new NoteInfo();

        }

        /// <summary>
        /// Note playback information.
        /// </summary>
        public class NoteInfo {

            /// <summary>
            /// Wave index.
            /// </summary>
            public ItemReference<WaveReference> WaveId;

            /// <summary>
            /// Original key, positive.
            /// </summary>
            public Notes OriginalKey = Notes.cn4;

            /// <summary>
            /// PSG Duty.
            /// </summary>
            public byte PSGDuty;

            /// <summary>
            /// Volume.
            /// </summary>
            public byte Volume = 127;

            /// <summary>
            /// Pan, positive.
            /// </summary>
            public sbyte Pan = 64;

            /// <summary>
            /// Surround pan.
            /// </summary>
            public sbyte SurroundPan;

            /// <summary>
            /// Pitch.
            /// </summary>
            public float Pitch = 1f;

            /// <summary>
            /// Key group, 0-15.
            /// </summary>
            public byte KeyGroup;

            /// <summary>
            /// Attack.
            /// </summary>
            public byte Attack = 127;

            /// <summary>
            /// Decay.
            /// </summary>
            public byte Decay = 127;

            /// <summary>
            /// Sustain.
            /// </summary>
            public byte Sustain = 127;

            /// <summary>
            /// Hold.
            /// </summary>
            public byte Hold = 0;

            /// <summary>
            /// Release.
            /// </summary>
            public byte Release = 127;

            /// <summary>
            /// Percussing mode. (Is ignore off.)
            /// </summary>
            public bool PercussionMode;

            /// <summary>
            /// Interpolation type.
            /// </summary>
            public EInterPolationType InterPolationType;

            /// <summary>
            /// Interpolation type.
            /// </summary>
            public enum EInterPolationType {
                PolyPhase, Linear
            }

        }

    }

}

using GotaSequenceLib.Playback;
using GotaSoundIO;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Sound archive (BRSAR).
    /// </summary>
    public class SoundArchive : IOFile {

        /// <summary>
        /// File header.
        /// </summary>
        public FileHeader Header;

        /// <summary>
        /// Write string data.
        /// </summary>
        public bool WriteSymbols;

        /// <summary>
        /// Sounds.
        /// </summary>
        public List<SoundInfo> Sounds = new List<SoundInfo>();

        /// <summary>
        /// Banks.
        /// </summary>
        public List<BankInfo> Banks = new List<BankInfo>();

        /// <summary>
        /// Players.
        /// </summary>
        public List<PlayerInfo> Players = new List<PlayerInfo>();

        /// <summary>
        /// Groups.
        /// </summary>
        public List<Group> Groups = new List<Group>();

        /// <summary>
        /// Files. Each file that is not a stream is automatically added to the last group if not added to any others.
        /// </summary>
        public List<FileData> Files = new List<FileData>();

        /// <summary>
        /// Raw groups. WILL DELETE!!!
        /// </summary>
        public List<GroupInfo> RawGroups = new List<GroupInfo>();

        /// <summary>
        /// Raw files. WILL DELETE!!!
        /// </summary>
        public List<FileEntry> RawFiles = new List<FileEntry>();

        /// <summary>
        /// Project info.
        /// </summary>
        public ProjectInfo ProjectInfo = new ProjectInfo();

        /// <summary>
        /// Common sound info table used.
        /// </summary>
        public static RVersion CommonSoundInfo = new RVersion() { Major = 1, Minor = 1 };

        /// <summary>
        /// Pan mode support used.
        /// </summary>
        public static RVersion PanModeSupport = new RVersion() { Major = 1, Minor = 2 };

        /// <summary>
        /// Release and priority fixed.
        /// </summary>
        public static RVersion ReleasePriorityFix = new RVersion() { Major = 1, Minor = 3 };

        /// <summary>
        /// Stream info uses a count instead of a bitflag.
        /// </summary>
        public static RVersion StreamChannelCount = new RVersion() { Major = 1, Minor = 4 };

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public SoundArchive() {
            Header = new RFileHeader() { Version = new RVersion() { Major = 1, Minor = 4 } };
        }

        /// <summary>
        /// Create a sound archive from a file path.
        /// </summary>
        /// <param name="filePath">Path of the file.</param>
        public SoundArchive(string filePath) : base(filePath) {
            Header = new RFileHeader() { Version = new RVersion() { Major = 1, Minor = 4 } };
        }

        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read file header.
            r.OpenFile<RFileHeader>(out Header);

            //Symb exists.
            bool symbExists = WriteSymbols = Header.BlockOffsets.Count() >= 3;
            List<string> symb = null;

            //Read symbol block.
            if (symbExists) {

                //Open block.
                r.OpenBlock(0, out _, out _);

                //Read the strings.
                symb = r.Read<ROffset<Table<ROffset<NullTerminatedString>>>>().Data.Select(x => x.Data).Select(x => x.Data).ToList();

                /* On second thought, there is no need to read these tables.
                r.OpenOffset("SoundTreeTable");
                r.OpenOffset("PlayerTreeTable");
                r.OpenOffset("GroupTreeTable");
                r.OpenOffset("BankTreeTable"); */
                r.ReadUInt32s(4);

            }

            //Read info block.
            r.OpenBlock(symbExists ? 1 : 0, out _, out _);

            //Get data.
            if (Header.Version >= CommonSoundInfo) {
                long bak = r.Position;
                r.OpenReference<RReference<object>>("Tbl");
                r.JumpToReference("Tbl");
                uint numSounds = r.ReadUInt32();
                for (uint i = 0; i < numSounds; i++) {
                    r.OpenReference<RReference<object>>("Snd" + i);
                }
                for (uint i = 0; i < numSounds; i++) {
                    if (r.ReferenceNull("Snd" + i)) {
                        Sounds.Add(null);
                    } else {
                        r.JumpToReference("Snd" + i);
                        Sounds.Add(SoundInfo.GetSoundInfo(r));
                    }
                }
                r.Position = bak + 8;
            } else {
                Sounds = r.Read<RReference<Table<SoundReference>>>().Data.Select(x => x.Data).ToList(); //Yeah, I need to double check this; common info may be different.
            }
            Banks = r.Read<RReference<Table<RReference<BankInfo>>>>().Data.Select(x => x.Data).ToList();
            Players = r.Read<RReference<Table<RReference<PlayerInfo>>>>().Data.Select(x => x.Data).ToList();
            RawFiles = r.Read<RReference<Table<RReference<FileEntry>>>>().Data.Select(x => x.Data).ToList();
            RawGroups = r.Read<RReference<Table<RReference<GroupInfo>>>>().Data.Select(x => x.Data).ToList();
            ProjectInfo = r.Read<RReference<ProjectInfo>>();

            //Add file entries.
            Files = new List<FileData>();
            foreach (var f in RawFiles) {
                Files.Add(null);
            }

            //Refine names, read files, adjust data.
            foreach (var o in Sounds) {
                if (symbExists && o.ReadingStringId.Index != 0xFFFFFF && o.ReadingStringId.Type != IdType.Null) {
                    o.Name = symb[o.ReadingStringId.Index];
                }
                if (Header.Version < PanModeSupport) {
                    o.PanMode = 1;
                    o.PanCurve = 0;
                    o.SupportsPanStuff = false;
                }
                o.Player = Players[o.ReadingPlayerId.Index];
                switch ((IdType)o.SoundType()) {
                    case IdType.Sequence:
                        (o as SequenceInfo).SoundFile = ReadFile<Sequence>(r, RawGroups, RawFiles, o.ReadingFileId.Index);
                        if (Header.Version < ReleasePriorityFix) {
                            (o as SequenceInfo).ReleasePriorityFix = (o as SequenceInfo).SupportsPriorityFix = false;
                        }
                        if (((o as SequenceInfo).SoundFile.File as Sequence).Labels.Values.Contains((o as SequenceInfo).DataOffset)) {
                            (o as SequenceInfo).SequenceLabel = ((o as SequenceInfo).SoundFile.File as Sequence).Labels.FirstOrDefault(x => x.Value == (o as SequenceInfo).DataOffset).Key;
                        }
                        (o as SequenceInfo).Bank = Banks[(o as SequenceInfo).ReadingBankId.Index];
                        break;
                    case IdType.Stream:
                        (o as StreamInfo).SoundFile = ReadFile<RevolutionStream>(r, RawGroups, RawFiles, o.ReadingFileId.Index);
                        //Convert count back to flags.
                        if (Header.Version >= StreamChannelCount) {
                            ushort flags = 0;
                            for (int i = 0; i < (o as StreamInfo).ChannelAllocationCount; i++) {
                                flags |= (ushort)(0b1 << i);
                            }
                            (o as StreamInfo).ChannelAllocationCount = flags;
                            (o as StreamInfo).SupportsChannelCount = true;
                        }
                        break;
                    case IdType.Wave:
                        (o as WaveSoundEntryInfo).SoundFile = ReadFile<WaveSoundData>(r, RawGroups, RawFiles, o.ReadingFileId.Index);
                        if (Header.Version < ReleasePriorityFix) {
                            (o as WaveSoundEntryInfo).ReleasePriorityFix = (o as WaveSoundEntryInfo).SupportsPriorityFix = false;
                        }
                        (o as WaveSoundEntryInfo).WaveSoundEntry = (o as WaveSoundEntryInfo).SoundFile.FileS.Entries[(o as WaveSoundEntryInfo).SubNumber];
                        break;
                }
            }
            foreach (var o in Banks) {
                if (symbExists && o.ReadingStringId.Index != 0xFFFFFF && o.ReadingStringId.Type != IdType.Null) {
                    o.Name = symb[o.ReadingStringId.Index];
                }
                o.BankFile = ReadFile<Bank>(r, RawGroups, RawFiles, o.ReadingFileId.Index);
            }
            foreach (var o in Players) {
                if (symbExists && o.ReadingStringId.Index != 0xFFFFFF && o.ReadingStringId.Type != IdType.Null) {
                    o.Name = symb[o.ReadingStringId.Index];
                }
            }
            foreach (var o in RawGroups) {
                if (symbExists && o.ReadingStringId.Index != 0xFFFFFF && o.ReadingStringId.Type != IdType.Null) {
                    o.Name = symb[o.ReadingStringId.Index];
                }
            }

            //Better groups.
            Groups = new List<Group>();
            foreach (var g in RawGroups) {
                var n = new Group();
                n.Name = g.Name;
                n.ExternalPath = g.ExternalFilePath;
                n.IsExternal = g.ExternalFilePath != null;
                if (g.Items != null) {
                    n.Files = new List<FileData>();
                    foreach (var i in g.Items) {
                        n.Files.Add(Files[i.ReadingFileId.Index]);
                        if (g.WaveDataOffset != 0 && g.WaveDataSize != 0 && i.WaveDataSize != 0 ){
                            r.Position = g.WaveDataOffset + i.WaveDataOffset; //TODO: EXTERNAL GROUP SUPPORT!!!
                            if (n.Files.Last() != null) {
                                if (n.Files.Last().File as Bank != null) {
                                    n.Files.Last().WaveArchive = GetWaveArchive(r, n.Files.Last().GetFile<Bank>(), i.WaveDataSize);
                                } else {
                                    n.Files.Last().WaveArchive = GetWaveArchive(r, n.Files.Last().GetFile<WaveSoundData>(), i.WaveDataSize);
                                }
                            }
                        }
                    }
                    var hasWarFiles = n.Files.Where(x => x != null && x.WaveArchive != null).ToList();
                    if (hasWarFiles.Count > 1) {
                        n.ShareWaveArchives = true;
                        //for (int i = 0; i < hasWarFiles.Count - 1; i++) {
                        //    for (int j = i + 1; j < hasWarFiles.Count; j++) {
                        //        if (hasWarFiles[i].WaveArchive.Md5Sum != hasWarFiles[j].WaveArchive.Md5Sum) { n.ShareWaveArchives = false; }
                        //    }
                        //}
                    }
                }
                Groups.Add(n);
            }

            //Remove the last group because it actually doesn't exist.
            Groups.Remove(Groups.Last());

            //This causes problems if not taken care of.
            Files = Files.Where(x => x != null).ToList();
            foreach (var g in Groups) {
                g.Files = g.Files.Where(x => x != null).ToList();
            }

        }

        /// <summary>
        /// Get a wave archive.
        /// </summary>
        /// <param name="r">File reader.</param>
        /// <param name="b">Bank.</param>
        /// <param name="size">Size.</param>
        /// <returns>Wave archive.</returns>
        private WaveArchive GetWaveArchive(FileReader r, Bank b, uint size) {
            if (b.Version >= Bank.VersionUseWaveArchive) {
                return r.ReadFile<WaveArchive>();
            } else {
                //TODO: RAW PARSING!!!
                return null;
            }
        }

        /// <summary>
        /// Get a wave archive.
        /// </summary>
        /// <param name="r">File reader.</param>
        /// <param name="w">Wave sound data.</param>
        /// <param name="size">Size.</param>
        /// <returns>Wave archive.</returns>
        private WaveArchive GetWaveArchive(FileReader r, WaveSoundData w, uint size) {
            if (w.Version >= WaveSoundData.VersionUseWaveArchive) {
                return r.ReadFile<WaveArchive>();
            } else {
                //TODO: RAW PARSING!!!
                return null;
            }
        }

        /// <summary>
        /// Write a wave archive.
        /// </summary>
        /// <param name="w">File writer.</param>
        /// <param name="war">Wave archive.</param>
        /// <param name="b">Bank.</param>
        private void WriteWaveArchive(FileWriter w, WaveArchive war, Bank b) {
            if (b.Version >= Bank.VersionUseWaveArchive) {
                w.WriteFile(war);
            } else { 
                //TODO: RAW WRITING!!!
            }
        }

        /// <summary>
        /// Write a wave archive.
        /// </summary>
        /// <param name="w">File writer.</param>
        /// <param name="war">Wave archive.</param>
        /// <param name="d">Wave sound data.</param>
        private void WriteWaveArchive(FileWriter w, WaveArchive war, WaveSoundData d) {
            if (d.Version >= WaveSoundData.VersionUseWaveArchive) {
                w.WriteFile(war);
            } else {
                //TODO: RAW WRITING!!!
            }
        }

        /// <summary>
        /// Read a file.
        /// </summary>
        /// <typeparam name="T">Type to read.</typeparam>
        /// <param name="r">File reader.</param>
        /// <param name="rawGroups">Raw groups.</param>
        /// <param name="rawFiles">Raw files.</param>
        /// <param name="fileId">File Id.</param>
        /// <returns>File.</returns>
        private FileData<T> ReadFile<T>(FileReader r, List<GroupInfo> rawGroups, List<FileEntry> rawFiles, int fileId) where T : IOFile {

            //File data.
            FileData<T> d = null;

            //Null.
            if (fileId == 0xFFFFFF) {
                Files[fileId] = d;
                return d;
            } else {
                d = new FileData<T>();
                d.External = rawFiles[fileId].ExternalFilePath != null;
                d.FilePath = rawFiles[fileId].ExternalFilePath;
            }

            //Read data.
            if (!d.External) {
                var info = rawFiles[fileId].FileInfo[0];
                var g = rawGroups[info.GroupId.Index];
                //TODO: EXTERNAL GROUPS.
                r.Position = g.Offset + g.Items[(int)info.Index].Offset;
                d.File = Activator.CreateInstance<T>();
                d.File.Read(r);
            }

            //Return entry.
            if (Files[fileId] is null) { Files[fileId] = d; }
            return Files[fileId] as FileData<T>;

        }

        /// <summary>
        /// Write the sound archive.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Init header.
            w.InitFile<RFileHeader>("RSAR", ByteOrder.BigEndian, Header.Version, WriteSymbols ? 3 : 2);

            //Symbol block.
            if (WriteSymbols) {

                //Start.
                w.InitBlock("SYMB");

                //Offsets table.
                w.InitOffset("StringTable");
                w.InitOffset("SoundTree");
                w.InitOffset("PlayerTree");
                w.InitOffset("GroupTree");
                w.InitOffset("BankTree");

                //String table.
                w.CloseOffset("StringTable");
                List<string> strs = new List<string>();
                if (Players != null) {
                    foreach (var o in Players) {
                        if (o.Name != null) {
                            if (!strs.Contains(o.Name)) {
                                strs.Add(o.Name);
                            }
                            o.ReadingStringId = new Id() { Index = strs.IndexOf(o.Name) };
                        } else {
                            o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                        }
                    }
                }
                if (RawGroups != null) {
                    foreach (var o in RawGroups) {
                        if (o.Name != null) {
                            if (!strs.Contains(o.Name)) {
                                strs.Add(o.Name);
                            }
                            o.ReadingStringId = new Id() { Index = strs.IndexOf(o.Name) };
                        } else {
                            o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                        }
                    }
                }
                if (Banks != null) {
                    foreach (var o in Banks) {
                        if (o.Name != null) {
                            if (!strs.Contains(o.Name)) {
                                strs.Add(o.Name);
                            }
                            o.ReadingStringId = new Id() { Index = strs.IndexOf(o.Name) };
                        } else {
                            o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                        }
                    }
                }
                if (Sounds != null) {
                    foreach (var o in Sounds) {
                        if (o.Name != null) {
                            if (!strs.Contains(o.Name)) {
                                strs.Add(o.Name);
                            }
                            o.ReadingStringId = new Id() { Index = strs.IndexOf(o.Name) };
                        } else {
                            o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                        }
                        try { o.Player = Players[o.ReadingPlayerId.Index]; } catch { }
                    }
                }

                //Write table.
                w.WriteTableWithOffsets(strs.Select(x => new NullTerminatedString(x)).ToList());
                w.Align(0x4);

                //Sound table.
                w.CloseOffset("SoundTree");
                PatriciaTree sndTree = new PatriciaTree();
                foreach (var o in Sounds) {
                    if (o.Name != null) {
                        sndTree.Add(new PatriciaTree.PatriciaTreeItem() { ItemIndex = Sounds.IndexOf(o), Key = o.Name, SoundType = IdType.Invalid, StringTableIndex = strs.IndexOf(o.Name) });
                    }
                }
                w.Write(sndTree);

                //Player table.
                w.CloseOffset("PlayerTree");
                PatriciaTree plyTree = new PatriciaTree();
                foreach (var o in Players) {
                    if (o.Name != null) {
                        plyTree.Add(new PatriciaTree.PatriciaTreeItem() { ItemIndex = Players.IndexOf(o), Key = o.Name, SoundType = IdType.Invalid, StringTableIndex = strs.IndexOf(o.Name) });
                    }
                }
                w.Write(plyTree);

                //Group table.
                w.CloseOffset("GroupTree");
                PatriciaTree grpTree = new PatriciaTree();
                foreach (var o in RawGroups) {
                    if (o.Name != null) {
                        grpTree.Add(new PatriciaTree.PatriciaTreeItem() { ItemIndex = RawGroups.IndexOf(o), Key = o.Name, SoundType = IdType.Invalid, StringTableIndex = strs.IndexOf(o.Name) });
                    }
                }
                w.Write(grpTree);

                //Bank table.
                w.CloseOffset("BankTree");
                PatriciaTree bnkTree = new PatriciaTree();
                foreach (var o in Banks) {
                    if (o.Name != null) {
                        bnkTree.Add(new PatriciaTree.PatriciaTreeItem() { ItemIndex = Banks.IndexOf(o), Key = o.Name, SoundType = IdType.Invalid, StringTableIndex = strs.IndexOf(o.Name) });
                    }
                }
                w.Write(bnkTree);

                //End.
                w.Align(0x20);
                w.CloseBlock();

            } else {

                //No strings for you.
                foreach (var o in Sounds) {
                    o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                }
                foreach (var o in Players) {
                    o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                }
                foreach (var o in RawGroups) {
                    o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                }
                foreach (var o in Banks) {
                    o.ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF };
                }

            }

            //Pass 1.
            bool gotFileInfo = false;

            //Info block.
            w.InitBlock("INFO");

            //Files set up.
            foreach (var f in Files) {
                f.Groups = new List<int>();
                f.GroupIndices = new List<uint>();
            }

            //Group data.
            List<GroupInfo> groups = new List<GroupInfo>();
            int numExtraFiles = 0; //Groups with shared wave forms add a useless file?
            foreach (var g in Groups) {
                GroupInfo e = new GroupInfo();
                e.EntryNumber = -1; //Some quality Nintendo genius?
                if (g.IsExternal) { e.ExternalFilePath = g.ExternalPath; }
                e.Name = g.Name;
                e.ReadingStringId = new Id();
                e.Items = new List<GroupItem>();
                foreach (var f in g.Files) {
                    e.Items.Add(new GroupItem() { ReadingFileId = new Id() { Index = Files.IndexOf(f) } });
                    Files[Files.IndexOf(f)].Groups.Add(Groups.IndexOf(g));
                    Files[Files.IndexOf(f)].GroupIndices.Add((uint)(e.Items.Count - 1));
                }
                if (g.ShareWaveArchives) {
                    numExtraFiles++;
                }
                groups.Add(e);
            }

            //Last group.
            groups.Add(new GroupInfo() { EntryNumber = -1, ReadingStringId = new Id() { Type = IdType.Null, Index = 0xFFFFFF }, Items = new List<GroupItem>() });
            foreach (var f in Files) {
                if (f.Groups.Count == 0 && !f.External) {
                    f.Groups.Add(groups.Count - 1);
                    f.GroupIndices.Add((uint)groups.Last().Items.Count);
                    groups.Last().Items.Add(new GroupItem() { ReadingFileId = new Id() { Index = Files.IndexOf(f) } });
                }
            }

            //File data.
            List<FileEntry> files = new List<FileEntry>();
            foreach (var f in Files) {
                FileEntry e = new FileEntry();
                e.EntryNumber = -1; //Some quality Nintendo genius?
                if (f.External) { e.ExternalFilePath = f.FilePath; }
                e.FileInfo = new List<FileInfo>();
                foreach (var g in f.Groups) {
                    e.FileInfo.Add(new FileInfo() { GroupId = new Id() { Index = g }, Index = f.GroupIndices[f.Groups.IndexOf(g)] });
                }
                files.Add(e);
            }
            for (int i = 0; i < numExtraFiles; i++) {
                FileEntry e = new FileEntry();
                e.EntryNumber = -1; //Some quality Nintendo genius?
                e.FileInfo = new List<FileInfo>();
                files.Add(e);
            }

            //Save point.
            long infoPos = w.Position;

            //Set up info.
            writeInfo:
            if (!gotFileInfo) {

                //Sounds.
                foreach (var o in Sounds) { 
                
                    //Common.
                    //o.ReadingFileId = new Id() { Type = IdType.Invalid, Index = Files.IndexOf(o.SoundFile) };
                    if (o.Player == null) { w.Write(o.ReadingPlayerId); } else { o.ReadingPlayerId = new Id() { Type = IdType.Invalid, Index = Players.IndexOf(o.Player) }; }

                    //Specifics.
                    var seq = o as SequenceInfo;
                    var stm = o as StreamInfo;
                    var wsd = o as WaveSoundEntryInfo;

                    //Sequence.
                    if (seq != null) {
                        if (seq.Bank != null) {
                            seq.ReadingBankId = new Id() { Type = IdType.Invalid, Index = Banks.IndexOf(seq.Bank) };
                        }
                        if (seq.SequenceLabel != null && (seq.SoundFile.File as Sequence).Labels.ContainsKey(seq.SequenceLabel)) {
                            seq.DataOffset = (seq.SoundFile.File as Sequence).Labels[seq.SequenceLabel];
                        }
                    }

                    //Wsd.
                    else if (wsd != null) {
                        if (wsd.WaveSoundEntry != null) {
                            wsd.SubNumber = (wsd.SoundFile.File as WaveSoundData).Entries.IndexOf(wsd.WaveSoundEntry);
                        }
                    }

                }

                //Banks.
                foreach (var o in Banks) {
                    //o.ReadingFileId = new Id() { Type = IdType.Invalid, Index = Files.IndexOf(o.BankFile) };
                }

                //FILES???

                //GROUPS!!!

            }

            //Set up file info.
            else { 
             
                //FILES!!!

                //GROUPS???

            }

            //References.
            w.InitReference<RReference<object>>("SoundTblRef");
            w.InitReference<RReference<object>>("BankTblRef");
            w.InitReference<RReference<object>>("PlayerTblRef");
            w.InitReference<RReference<object>>("FileTblRef");
            w.InitReference<RReference<object>>("GroupTblRef");
            w.InitReference<RReference<object>>("ProjectInfoRef");

            //Sound info, Nintendo loves being difficult with versions.
            w.CloseReference("SoundTblRef");
            if (Header.Version < CommonSoundInfo) {

            } else {
                w.WriteTableWithReferences(Sounds);
            }

            //Wow, this turned out to look a lot simpler than I thought it would.
            w.CloseReference("BankTblRef");
            w.WriteTableWithReferences(Banks);
            w.CloseReference("PlayerTblRef");
            w.WriteTableWithReferences(Players);
            w.CloseReference("FileTblRef");
            w.WriteTableWithReferences(files);
            w.CloseReference("GroupTblRef");
            w.WriteTableWithReferences(groups);

            //Project info.
            w.CloseReference("ProjectInfoRef");
            w.Write(ProjectInfo);

            //End info block.
            w.Align(0x20);

            //Time to write the file information. TODO: LEGACY FORMATS!!!
            if (!gotFileInfo) {

                //Don't overwrite file header.
                w.Write(new byte[0x20]);

                //Set flag.
                gotFileInfo = true;

                //Keep track of files written.
                List<FileData> filesWritten = new List<FileData>();

                //Write each group.
                foreach (var g in Groups) {

                    //Adjust shared wave indices.
                    WaveArchive war = new WaveArchive();
                    List<string> md5sums = new List<string>();
                    if (g.ShareWaveArchives) {
                        foreach (var f in g.Files) {
                            if (f == null) { continue; }
                            if (f.WaveArchive != null) {
                                foreach (var v in f.WaveArchive.Waves) {
                                    if (!md5sums.Contains(v.Md5Sum)) {
                                        war.Waves.Add(v);
                                        md5sums.Add(v.Md5Sum);
                                    }
                                }
                                var bnk = f.File as Bank;
                                var wsd = f.File as WaveSoundData;
                                if (bnk != null) {
                                    foreach (var inst in bnk.Instruments) {
                                        foreach (var r in inst.Regions) {
                                            foreach (var v in r.VelocityRegions) {
                                                v.WaveId = war.Waves.IndexOf(war.Waves.Where(x => x.Md5Sum.Equals(f.WaveArchive.Waves[v.WaveId].Md5Sum)).FirstOrDefault());
                                            }
                                        }
                                    }
                                    f.WaveArchive = war;
                                } else if (wsd != null) {
                                    foreach (var e in wsd.Entries) {
                                        foreach (var n in e.NoteInfo) {
                                            n.WaveIndex = war.Waves.IndexOf(war.Waves.Where(x => x.Md5Sum.Equals(f.WaveArchive.Waves[n.WaveIndex].Md5Sum)).FirstOrDefault());
                                        }
                                    }
                                    f.WaveArchive = war;
                                }
                            }
                        }
                    }

                    //Fix offsets in the old mode. TODO!!!
                    foreach (var f in g.Files) {
                        if (f.WaveArchive != null && f.File != null) {
                            var bnk = f.File as Bank;
                            var wsd = f.File as WaveSoundData;
                            if (bnk != null) {

                            } else if (wsd != null) { 
                                
                            }
                        }
                    }

                    //Write each file.
                    foreach (var f in g.Files) {
                        w.Align(0x20);
                        if (f != null) { w.WriteFile(f.File); }
                    }

                    //Shared mode.
                    if (g.ShareWaveArchives) {

                        //Align.
                        w.Align(0x20);

                        //Old mode. TODO!!!

                        //New mode.
                        w.WriteFile(war);

                    }

                    //Don't share mode.
                    else {

                        //Write wave archives.
                        foreach (var f in g.Files) {
                            if (f == null) { continue; }
                            if (f.File as Bank != null) {
                                w.Align(0x20);
                                WriteWaveArchive(w, f.WaveArchive, f.File as Bank);
                            } else if (f.File as WaveSoundData != null) {
                                w.Align(0x20);
                                WriteWaveArchive(w, f.WaveArchive, f.File as WaveSoundData);
                            }
                        }

                    }
                
                }

                //I still need to figure out how to write the last group...

                //Hope you like info, write it again.
                w.Position = infoPos;
                goto writeInfo;

            }

            //Close block finally.
            w.CloseBlock();

            //File block.
            w.InitBlock("FILE");
            w.Position = w.Length;
            w.Align(0x20);
            w.CloseBlock();

            //End the file.
            w.CloseFile();

        }

        /// <summary>
        /// Get wave archives for a bank. To be used for sequence playback.
        /// </summary>
        /// <param name="bank">Bank to use.</param>
        /// <returns>Wave archives.</returns>
        public RiffWave[][] GetWaveArchives(BankInfo bank) {
            return new RiffWave[][] { bank.BankFile.WaveArchive.GetWaves() };
        }

        /// <summary>
        /// Sound reference.
        /// </summary>
        public class SoundReference : RReference<SoundInfo> {
            public override List<Type> DataTypes => new List<Type>() { null, typeof(SequenceInfo), typeof(StreamInfo), typeof(WaveSoundEntryInfo) };
        }

    }

}

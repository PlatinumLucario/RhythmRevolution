using GotaSequenceLib.Playback;
using GotaSoundIO.IO;
using RevolutionFileLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmRevolution {

    /// <summary>
    /// Main window.
    /// </summary>
    public class MainWindow : EditorBase {

        /// <summary>
        /// Revolution path.
        /// </summary>
        public static string RevolutionPath = Application.StartupPath;

        /// <summary>
        /// The sound archive.
        /// </summary>
        public SoundArchive SA => File as SoundArchive;

        /// <summary>
        /// Sound info pages.
        /// </summary>
        private Dictionary<string, TabPage> SoundInfoPages;

        /// <summary>
        /// Mixer.
        /// </summary>
        public Mixer Mixer = new Mixer();

        /// <summary>
        /// Player.
        /// </summary>
        public Player Player;

        /// <summary>
        /// Timer.
        /// </summary>
        public Timer Timer = new Timer();

        /// <summary>
        /// Position bar free.
        /// </summary>
        public bool PositionBarFree = true;

        /// <summary>
        /// Main window.
        /// </summary>
        public MainWindow() : base(typeof(SoundArchive), "Sound Archive", "sar", "Rhythm Revolution", null) {
            Init();
        }

        /// <summary>
        /// Open a main window from a file.
        /// </summary>
        /// <param name="fileToOpen">File to open.</param>
        public MainWindow(string fileToOpen) : base(typeof(SoundArchive), "Sound Archive", "sar", "Rhythm Revolution", fileToOpen, null) {
            Init();
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        public void Init() {

            //Window stuff.
            Icon = Properties.Resources.Icon;
            FormClosing += new FormClosingEventHandler(SAClosing);
            toolsToolStripMenuItem.Visible = true;
            seqImportModeBox.SelectedIndex = 0;

            //Sound info.
            SoundInfoPages = new Dictionary<string, TabPage>();
            SoundInfoPages.Add("soundPage3d", soundInfoTabs.TabPages["soundPage3d"]);
            SoundInfoPages.Add("sequencePage", soundInfoTabs.TabPages["sequencePage"]);
            SoundInfoPages.Add("streamPage", soundInfoTabs.TabPages["streamPage"]);
            SoundInfoPages.Add("wavePage", soundInfoTabs.TabPages["wavePage"]);

            //Player.
            playerHeapSizeBox.ValueChanged += PlayerHeapSizeChanged;
            playableSoundCount.ValueChanged += PlayerSoundCountChanged;

            //Sound player.
            Player = new Player(Mixer);
            kermalisPlayButton.Click += new EventHandler(PlayClick);
            kermalisPauseButton.Click += new EventHandler(PauseClick);
            kermalisStopButton.Click += new EventHandler(StopClick);
            kermalisVolumeSlider.ValueChanged += new EventHandler(VolumeChanged);
            kermalisLoopBox.CheckedChanged += new EventHandler(LoopChanged);
            kermalisPosition.MouseUp += new MouseEventHandler(PositionMouseUp);
            kermalisPosition.MouseDown += new MouseEventHandler(PositionMouseDown);
            tree.KeyPress += new KeyPressEventHandler(KeyPress);
            kermalisVolumeSlider.Value = 75;
            Mixer.Volume = .75f;
            Timer.Tick += PositionTick;
            Timer.Interval = 1000 / 30;
            Timer.Start();

        }

        /// <summary>
        /// Do info stuff.
        /// </summary>
        public override void DoInfoStuff() {

            //The base.
            base.DoInfoStuff();
            WritingInfo = true;

            //Index.
            int ind = tree.SelectedNode.Index;

            //Hide stuff.
            void HideStuff() {
                kermalisSoundPlayerPanel.Hide();
                indexPanel.Hide();
                fileIdPanel.Hide();
                kermalisSoundPlayerPanel.SendToBack();
                fileIdPanel.SendToBack();
                indexPanel.SendToBack();
            }

            //If file open.
            if (!FileOpen || File == null) {
                HideStuff();
                //if (Player != null) { StopClick(this, null); }
                return;
            }

            //Panel selected.
            bool panelSelected = false;

            //Parent is null.
            if (tree.SelectedNode.Parent == null) {

                //If settings.
                if (tree.SelectedNode == tree.Nodes["settings"]) {
                    HideStuff();
                    settingsPanel.BringToFront();
                    settingsPanel.Show();
                    writeNamesBox.Checked = SA.WriteSymbols;
                    status.Text = "Editing Settings.";
                    var e = SA.ProjectInfo;
                    maxSequences.Value = e.MaxSequences;
                    maxSequenceTracks.Value = e.MaxSequenceTracks;
                    maxStreams.Value = e.MaxStreams;
                    maxStreamChannels.Value = e.MaxStreamChannels;
                    maxStreamTracks.Value = e.MaxStreamTracks;
                    maxWaves.Value = e.MaxWaves;
                    maxWaveTracks.Value = e.MaxWaveTracks;
                    sarVersion.SelectedIndex = SA.Header.Version.Minor - 1;
                    bnkVersion.SelectedIndex = 0;
                    wsdVersion.SelectedIndex = 0;
                    if (SA.Banks.Count > 0) {
                        bnkVersion.SelectedIndex = SA.Banks[0].BankFile.File.Version.Minor;
                    }
                    if (SA.Sounds.Where(x => x as WaveSoundEntryInfo != null).Count() > 0) {
                        wsdVersion.SelectedIndex = (SA.Sounds.Where(x => x as WaveSoundEntryInfo != null).FirstOrDefault() as WaveSoundEntryInfo).SoundFile.File.Version.Minor;
                    }
                    panelSelected = true;
                }

            }

            //Child.
            else {

                //Panel selected.
                panelSelected = true;

                //Sound.
                if (tree.SelectedNode.Parent == tree.Nodes["sounds"]) {
                    kermalisSoundPlayerPanel.Show();
                    indexPanel.Show();
                    fileIdPanel.Show();
                    soundInfoPanel.BringToFront();
                    soundInfoPanel.Show();
                    itemIndexBox.Value = ind;
                    var e = SA.Sounds[ind];
                    ShowOrHideSoundPage("soundPage3d", e.Sound3d != null);
                    PopulatePlayerBox(soundInfoPlayerComboBox);
                    soundInfoPlayerComboBox.SelectedIndex = SA.Players.IndexOf(e.Player);
                    soundInfoPlayerBox.Value = SA.Players.IndexOf(e.Player);
                    soundInfoUse3d.Checked = e.Sound3d != null;
                    soundInfoVolumeBox.Value = e.Volume;
                    soundInfoPlayerPriorityBox.Value = e.PlayerPriority;
                    soundInfoRemoteFilterBox.Value = e.RemoteFilter;
                    soundInfoActorPlayerIdBox.Value = e.ActorPlayerId;
                    soundInfoUserParameter1Box.Value = e.UserParameters[0];
                    soundInfoUserParameter2Box.Value = e.UserParameters[1];
                    soundInfoPanModeBox.SelectedIndex = e.PanMode;
                    soundInfoPanCurveBox.SelectedIndex = e.PanCurve;
                    if (soundInfoUse3d.Checked) {
                        sound3dVolumeBox.Checked = !((e.Sound3d.Flags & (0b1 << 0)) > 0);
                        sound3dPanBox.Checked = !((e.Sound3d.Flags & (0b1 << 1)) > 0);
                        sound3dSurroundPanBox.Checked = !((e.Sound3d.Flags & (0b1 << 2)) > 0);
                        sound3dPriorityBox.Checked = !((e.Sound3d.Flags & (0b1 << 3)) > 0);
                        sound3dFilterBox.Checked = (e.Sound3d.Flags & (0b1 << 4)) > 0;
                        sound3dDecayComboBox.SelectedIndex = e.Sound3d.DecayCurve; //FIX THIS.
                        sound3dDecayRateBox.Value = e.Sound3d.DecayRatio;
                        sound3dDopplerFactorBox.Value = e.Sound3d.DopplerFactor;
                    }
                    FileData file = null;
                    switch (e.SoundType()) {

                        //Sequence.
                        case 1:
                            var seq = e as SequenceInfo;
                            soundInfoSoundTypeComboBox.SelectedIndex = 0;
                            ShowOrHideSoundPage("sequencePage", true);
                            ShowOrHideSoundPage("streamPage", false);
                            ShowOrHideSoundPage("wavePage", false);
                            file = seq.SoundFile;
                            seqStartComboBox.Items.Clear();
                            seqStartComboBox.Items.Add("Custom");
                            seqStartComboBox.SelectedIndex = 0;
                            foreach (var l in seq.SoundFile.FileS.Labels) {
                                seqStartComboBox.Items.Add(l.Key);
                                if (seq.DataOffset == l.Value) {
                                    seqStartComboBox.SelectedIndex = seqStartComboBox.Items.Count - 1;
                                }
                            }
                            seqStartBox.Value = seq.DataOffset;
                            seqBankComboBox.Items.Clear();
                            foreach (TreeNode n in tree.Nodes["banks"].Nodes) {
                                seqBankComboBox.Items.Add(n.Text);
                            }
                            seqBankComboBox.SelectedIndex = SA.Banks.IndexOf(seq.Bank);
                            seqBankBox.Value = SA.Banks.IndexOf(seq.Bank);
                            seqTrackFlags.Value = seq.TrackAllocation;
                            seqTrackChannelPriority.Value = seq.ChannelPriority;
                            seqReleasePriorityFix.Checked = seq.ReleasePriorityFix;
                            break;

                        //Stream.
                        case 2:
                            var stm = e as StreamInfo;
                            soundInfoSoundTypeComboBox.SelectedIndex = 1;
                            ShowOrHideSoundPage("sequencePage", false);
                            ShowOrHideSoundPage("streamPage", true);
                            ShowOrHideSoundPage("wavePage", false);
                            file = stm.SoundFile;
                            stmStartPosition.Value = stm.StartPosition;
                            stmTrackFlags.Value = stm.TrackAllocationFlags;
                            stmChannelFlags.Value = stm.ChannelAllocationCount;
                            break;

                        //WSD.
                        case 3:
                            var wsd = e as WaveSoundEntryInfo;
                            soundInfoSoundTypeComboBox.SelectedIndex = 2;
                            ShowOrHideSoundPage("sequencePage", false);
                            ShowOrHideSoundPage("streamPage", false);
                            ShowOrHideSoundPage("wavePage", true);
                            file = wsd.SoundFile;
                            wsdNumComboBox.Items.Clear();
                            for (int i = 0; i < wsd.SoundFile.FileS.Entries.Count; i++) {
                                wsdNumComboBox.Items.Add("Wave " + i);
                            }
                            wsdNumComboBox.SelectedIndex = wsd.SoundFile.FileS.Entries.IndexOf(wsd.WaveSoundEntry);
                            wsdNumBox.Value = wsd.SoundFile.FileS.Entries.IndexOf(wsd.WaveSoundEntry);
                            wsdTrackAllocationFlags.Value = wsd.TrackAllocation;
                            wsdChannelPriorityBox.Value = wsd.ChannelPriority;
                            wsdReleasePriorityBox.Checked = wsd.ReleasePriorityFix;
                            break;

                    }
                    fileIdBox.Value = SA.Files.IndexOf(file);
                    PopulateFileBox(fileIdComboBox);
                    fileIdComboBox.SelectedIndex = SA.Files.IndexOf(file);
                    status.Text = "[" + ind + "] " + e.Name + " Selected. " + GetFileString(file) + ".";
                }

                //Bank.
                else if (tree.SelectedNode.Parent == tree.Nodes["banks"]) {
                    kermalisSoundPlayerPanel.Hide();
                    indexPanel.Show();
                    fileIdPanel.Show();
                    blankPanel.BringToFront();
                    blankPanel.Show();
                    itemIndexBox.Value = ind;
                    var e = SA.Banks[ind];
                    fileIdBox.Value = SA.Files.IndexOf(e.BankFile);
                    PopulateFileBox(fileIdComboBox);
                    fileIdComboBox.SelectedIndex = SA.Files.IndexOf(e.BankFile);
                    status.Text = "[" + ind + "] " + e.Name + " Selected. " + GetFileString(e.BankFile) + ".";
                }

                //Player.
                else if (tree.SelectedNode.Parent == tree.Nodes["players"]) {
                    fileIdPanel.Hide();
                    kermalisSoundPlayerPanel.Hide();
                    playerPanel.BringToFront();
                    indexPanel.Show();
                    playerPanel.Show();
                    itemIndexBox.Value = ind;
                    var e = SA.Players[ind];
                    playerHeapSizeBox.Value = e.HeapSize;
                    playableSoundCount.Value = e.PlayableSoundCount;
                    status.Text = "[" + ind + "] " + e.Name + " Selected.";
                }

                //Group.
                else if (tree.SelectedNode.Parent == tree.Nodes["groups"]) {
                    fileIdPanel.Hide();
                    kermalisSoundPlayerPanel.Hide();
                    grpPanel.BringToFront();
                    indexPanel.Show();
                    grpPanel.Show();
                    var e = SA.Groups[ind];
                    grpShareWaveArchivesBox.Checked = e.ShareWaveArchives;
                    grpEntries.Rows.Clear();
                    var c = (grpEntries.Columns[0] as DataGridViewComboBoxColumn);
                    c.Items.Clear();
                    foreach (TreeNode f in tree.Nodes["files"].Nodes) {
                        c.Items.Add(f.Text);
                    }
                    foreach (var f in e.Files) {
                        grpEntries.Rows.Add(new DataGridViewRow());
                        (grpEntries.Rows[grpEntries.Rows.Count - 2].Cells[0] as DataGridViewComboBoxCell).Value = tree.Nodes["files"].Nodes[SA.Files.IndexOf(f)].Text;
                    }
                    status.Text = "[" + ind + "] " + e.Name + " Selected.";
                }

                //Files.
                else if (tree.SelectedNode.Parent == tree.Nodes["files"]) {
                    HideStuff();
                    noInfoPanel.BringToFront();
                    noInfoPanel.Show();
                    var e = SA.Files[tree.SelectedNode.Index];
                    status.Text = tree.SelectedNode.Text + " Selected. " + GetFileString(e) + ".";
                }

            }

            //No panel selected.
            if (!panelSelected) {
                HideStuff();
                noInfoPanel.BringToFront();
                noInfoPanel.Show();
                status.Text = "No Valid Info Selected!";
            }

            //Done.
            WritingInfo = false;

        }

        /// <summary>
        /// Update nodes.
        /// </summary>
        public override void UpdateNodes() {

            //Start update.
            BeginUpdateNodes();

            //Add nodes if node doesn't exist.
            if (tree.Nodes.Count == 1 ) {
                tree.Nodes.RemoveAt(0);
                tree.Nodes.Add("settings", "Settings", 1, 1);
                tree.Nodes.Add("sounds", "Sounds", 9, 9);
                tree.Nodes.Add("banks", "Instrument Banks", 4, 4);
                tree.Nodes.Add("players", "Players", 6, 6);
                tree.Nodes.Add("groups", "Groups", 7, 7);
                tree.Nodes.Add("files", "Files", 19, 19);
            }

            //File open and not null.
            if (FileOpen) {

                //Root menus.
                for (int i = 1; i < tree.Nodes.Count; i++) {
                    tree.Nodes[i].ContextMenuStrip = rootMenu;
                }

                //Load data.
                int num = 0;
                foreach (var e in SA.Sounds) {
                    int icon = GetSoundIcon(e);
                    tree.Nodes["sounds"].Nodes.Add("entry" + num, "[" + num + "] " + e.Name, icon, icon);
                    //tree.Nodes["sounds"].Nodes["entry" + e.Index].ContextMenuStrip = CreateMenuStrip(sarEntryMenu, new int[] { 0, 1, 4, 5, 6, 7 }, new EventHandler[] { new EventHandler(AddAbove), new EventHandler(AddBelow), new EventHandler(Replace), new EventHandler(Export), new EventHandler(Rename), new EventHandler(Delete) });
                    num++;
                }
                num = 0;
                foreach (var e in SA.Banks) {
                    tree.Nodes["banks"].Nodes.Add("entry" + num, "[" + num + "] " + e.Name, 4, 4);
                    //tree.Nodes["banks"].Nodes["entry" + e.Index].ContextMenuStrip = CreateMenuStrip(sarEntryMenu, new int[] { 0, 1, 4, 5, 6, 7 }, new EventHandler[] { new EventHandler(AddAbove), new EventHandler(AddBelow), new EventHandler(Replace), new EventHandler(Export), new EventHandler(Rename), new EventHandler(Delete) });
                    num++;
                }
                num = 0;
                foreach (var e in SA.Players) {
                    tree.Nodes["players"].Nodes.Add("entry" + num, "[" + num + "] " + e.Name, 6, 6);
                    //tree.Nodes["players"].Nodes["entry" + num].ContextMenuStrip = CreateMenuStrip(sarEntryMenu, new int[] { 0, 1, 6, 7 }, new EventHandler[] { new EventHandler(AddAbove), new EventHandler(AddBelow), new EventHandler(Rename), new EventHandler(Delete) });
                    num++;
                }
                num = 0;
                foreach (var e in SA.Groups) {
                    tree.Nodes["groups"].Nodes.Add("entry" + num, "[" + num + "] " + e.Name, 7, 7);
                    //tree.Nodes["groups"].Nodes["entry" + e.Index].ContextMenuStrip = CreateMenuStrip(sarEntryMenu, new int[] { 0, 1, 6, 7 }, new EventHandler[] { new EventHandler(AddAbove), new EventHandler(AddBelow), new EventHandler(Rename), new EventHandler(Delete) });
                    num++;
                }
                num = 0;
                foreach (var e in SA.Files) {
                    Tuple<string, int> ret = GetFileNameAndIcon(e);
                    string name = ret.Item1 ?? "File " + num;
                    int icon = ret.Item2;
                    tree.Nodes["files"].Nodes.Add("entry" + num, "[" + num + "] " + name, icon, icon);
                    //tree.Nodes["files"].Nodes["entry" + e.Index].ContextMenuStrip = CreateMenuStrip(sarEntryMenu, new int[] { 0, 1, 4, 5, 6, 7 }, new EventHandler[] { new EventHandler(AddAbove), new EventHandler(AddBelow), new EventHandler(Replace), new EventHandler(Export), new EventHandler(Rename), new EventHandler(Delete) });
                    num++;
                }

            } else {

                //Remove context menus.
                foreach (TreeNode n in tree.Nodes) {
                    n.ContextMenuStrip = null;
                }

            }

            //End update.
            EndUpdateNodes();

        }

        /// <summary>
        /// Get the sound icon.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private int GetSoundIcon(SoundInfo info) {
            switch (info.SoundType()) {
                case 1: //Seq.
                    return 2;
                case 2: //Stm.
                    return 9;
                case 3: //Wsd.
                    return 14;
            }
            return 0;
        }

        /// <summary>
        /// Get the name and icon for a file.
        /// </summary>
        /// <param name="file">File to get the name and icon of.</param>
        /// <returns>Name and icon.</returns>
        private Tuple<string, int> GetFileNameAndIcon(FileData file) {
            string name = null;
            int icon = 0;
            foreach (var s in SA.Sounds) {
                switch (s.SoundType()) {
                    case 1: //Seq.
                        if (file == (s as SequenceInfo).SoundFile) {
                            name = s.Name + ".brseq";
                            icon = 2;
                            goto ret;
                        }
                        break;
                    case 2: //Stm.
                        if (file == (s as StreamInfo).SoundFile) {
                            name = s.Name + ".brstm";
                            icon = 9;
                            goto ret;
                        }
                        break;
                    case 3: //Wsd.
                        if (file == (s as WaveSoundEntryInfo).SoundFile) {
                            name = s.Name + ".brwsd";
                            icon = 5;
                            goto ret;
                        }
                        break;
                }
            }
            foreach (var b in SA.Banks) {
                if (file == b.BankFile) {
                    name = b.Name + ".brbnk"; ;
                    icon = 4;
                    goto ret;
                }
            }
        ret:
            if (file.External) {
                name = file.FilePath;
            }
            return new Tuple<string, int>(name, icon);
        }

        /// <summary>
        /// Populate a file combo box.
        /// </summary>
        /// <param name="b">Combo box.</param>
        private void PopulateFileBox(ComboBox b) {
            b.Items.Clear();
            foreach (TreeNode n in tree.Nodes["files"].Nodes) {
                b.Items.Add(n.Text);
            }
        }

        /// <summary>
        /// Populate a bank combo box.
        /// </summary>
        /// <param name="m">Main window.</param>
        /// <param name="b">Combo box.</param>
        public static void PopulateBankBox(MainWindow m, ComboBox b) {
            b.Items.Clear();
            foreach (TreeNode n in m.tree.Nodes["banks"].Nodes) {
                b.Items.Add(n.Text);
            }
        }

        /// <summary>
        /// Populate a player combo box.
        /// </summary>
        /// <param name="b">Combo box.</param>
        private void PopulatePlayerBox(ComboBox b) {
            b.Items.Clear();
            foreach (TreeNode n in tree.Nodes["players"].Nodes) {
                b.Items.Add(n.Text);
            }
        }

        /// <summary>
        /// Get the amount of bytes for a file.
        /// </summary>
        /// <param name="f">The file.</param>
        /// <returns>The amount of bytes.</returns>
        public static string GetBytesSize(IOFile f) {
            long byteCount = f.Write().Length;
            string[] suf = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        /// <summary>
        /// Get the file string.
        /// </summary>
        /// <param name="file">File data.</param>
        /// <returns>File string.</returns>
        public static string GetFileString(FileData file) {
            string ret = "File Is At " + file.FilePath ?? "";
            if (!file.External) {
                ret = "File Is " + GetBytesSize(file.File); 
            }
            return ret;
        }

        /// <summary>
        /// Double click a node.
        /// </summary>
        public override void NodeMouseDoubleClick() {

            //Do base.
            base.NodeMouseDoubleClick();

            //Open file.
            if (tree.SelectedNode.Parent != null) {

                //Sound.
                if (tree.SelectedNode.Parent == tree.Nodes["sounds"]) {

                    //Type.
                    var e = SA.Sounds[tree.SelectedNode.Index];
                    var seq = e as SequenceInfo;
                    var wsd = e as WaveSoundEntryInfo;
                    var stm = e as StreamInfo;

                    //Sequence.
                    if (seq != null) {
                        SequenceEditor ed = new SequenceEditor(seq.SoundFile.GetFile(), this, e.Name);
                        ed.seqEditorBankComboBox.SelectedIndex = seq.Bank == null ? seq.ReadingBankId.Index : SA.Banks.IndexOf(seq.Bank);
                        ed.seqEditorBankBox.Value = seq.Bank == null ? seq.ReadingBankId.Index : SA.Banks.IndexOf(seq.Bank);
                        ed.Show();
                    }

                }

                //Sequence.
                else if (tree.SelectedNode.Parent == tree.Nodes["sequences"]) {
                    // var e = SA.Sequences.Where(x => x.Index == GetIdFromNode(tree.SelectedNode)).FirstOrDefault();
                    //SequenceEditor ed = new SequenceEditor(e.File, this, e.Name);
                    //SetBankIndex(SA, ed.seqEditorBankComboBox, e.Bank == null ? e.ReadingBankId : (uint)e.Bank.Index);
                    //ed.seqEditorBankBox.Value = e.Bank == null ? e.ReadingBankId : (uint)e.Bank.Index;
                    //ed.Show();
                }

                //Bank.
                else if (tree.SelectedNode.Parent == tree.Nodes["banks"]) {
                    /*var e = SA.Banks.Where(x => x.Index == GetIdFromNode(tree.SelectedNode)).FirstOrDefault();
                    BankEditor ed = new BankEditor(e.File, this, e.Name);
                    SetWaveArchiveIndex(SA, ed.war0ComboBox, e.WaveArchives[0] == null ? e.ReadingWave0Id : (ushort)e.WaveArchives[0].Index);
                    SetWaveArchiveIndex(SA, ed.war1ComboBox, e.WaveArchives[1] == null ? e.ReadingWave1Id : (ushort)e.WaveArchives[1].Index);
                    SetWaveArchiveIndex(SA, ed.war2ComboBox, e.WaveArchives[2] == null ? e.ReadingWave2Id : (ushort)e.WaveArchives[2].Index);
                    SetWaveArchiveIndex(SA, ed.war3ComboBox, e.WaveArchives[3] == null ? e.ReadingWave3Id : (ushort)e.WaveArchives[3].Index);
                    SetWaveArchiveIndex(SA, ed.war0Box, e.WaveArchives[0] == null ? e.ReadingWave0Id : (ushort)e.WaveArchives[0].Index);
                    SetWaveArchiveIndex(SA, ed.war1Box, e.WaveArchives[1] == null ? e.ReadingWave1Id : (ushort)e.WaveArchives[1].Index);
                    SetWaveArchiveIndex(SA, ed.war2Box, e.WaveArchives[2] == null ? e.ReadingWave2Id : (ushort)e.WaveArchives[2].Index);
                    SetWaveArchiveIndex(SA, ed.war3Box, e.WaveArchives[3] == null ? e.ReadingWave3Id : (ushort)e.WaveArchives[3].Index);
                    ed.LoadWaveArchives();
                    ed.Show();*/
                }

                //Wave archive.
                else if (tree.SelectedNode.Parent == tree.Nodes["waveArchives"]) {
                    //var e = SA.WaveArchives.Where(x => x.Index == GetIdFromNode(tree.SelectedNode)).FirstOrDefault();
                    //WaveArchiveEditor ed = new WaveArchiveEditor(e.File, this, e.Name);
                    //ed.Show();
                }

                //Stream.
                else if (tree.SelectedNode.Parent == tree.Nodes["streams"]) {
                    //var s = SA.Streams.Where(x => x.Index == GetIdFromNode(tree.SelectedNode)).FirstOrDefault();
                    //RiffWave r = new RiffWave();
                    //r.FromOtherStreamFile(s.File);
                    //r.Write(MainWindow.NitroPath + "/" + "tmpStream" + StreamTempCount++ + ".wav");
                    //StreamPlayer p = new StreamPlayer(this, MainWindow.NitroPath + "/" + "tmpStream" + (StreamTempCount - 1) + ".wav", s.Name);
                    //p.Show();
                }

            }

        }

        //Sound info.
        #region SoundInfo

        /// <summary>
        /// Show or hide a page.
        /// </summary>
        /// <param name="page">Page.</param>
        /// <param name="show">To show or not.</param>
        private void ShowOrHideSoundPage(string page, bool show) {
            if (show) {
                if (!soundInfoTabs.TabPages.ContainsKey(page)) {
                    soundInfoTabs.TabPages.Add(SoundInfoPages[page]);
                }
            } else {
                if (soundInfoTabs.TabPages.ContainsKey(page)) {
                    soundInfoTabs.TabPages.Remove(SoundInfoPages[page]);
                }
            }
        }

        #endregion

        //Player info.
        #region PlayerInfo

        /// <summary>
        /// Player changed.
        /// </summary>
        public void PlayerSoundCountChanged(object sender, EventArgs e) {
            if (FileOpen && !WritingInfo) {
                SA.Players[tree.SelectedNode.Index].PlayableSoundCount = (byte)playableSoundCount.Value;
            }
        }

        /// <summary>
        /// Player changed.
        /// </summary>
        public void PlayerHeapSizeChanged(object sender, EventArgs e) {
            if (FileOpen && !WritingInfo) {
                SA.Players[tree.SelectedNode.Index].HeapSize = (uint)playerHeapSizeBox.Value;
            }
        }

        #endregion

        //Player control.
        #region PlayerControl

        /// <summary>
        /// Play click.
        /// </summary>
        public void PlayClick(object sender, EventArgs e) {
            PlaySequence(SA.Sounds[tree.SelectedNode.Index] as SequenceInfo);
        }

        /// <summary>
        /// Position tick.
        /// </summary>
        public void PositionTick(object sender, EventArgs e) {
            if (Player != null && PositionBarFree) {
                kermalisPosition.Value = Player.GetCurrentPosition() > kermalisPosition.Maximum ? kermalisPosition.Maximum : (int)Player.GetCurrentPosition();
            }
        }

        /// <summary>
        /// Mouse down.
        /// </summary>
        public void PositionMouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                PositionBarFree = false;
            }
        }

        /// <summary>
        /// Mouse up.
        /// </summary>
        public void PositionMouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left && Player != null && Player.Events != null) {
                Player.SetCurrentPosition(kermalisPosition.Value);
                PositionBarFree = true;
            }
        }

        /// <summary>
        /// Pause click.
        /// </summary>
        public void PauseClick(object sender, EventArgs e) {
            Player.Pause();
        }

        /// <summary>
        /// Stop click.
        /// </summary>
        public void StopClick(object sender, EventArgs e) {
            Player.Stop();
        }

        /// <summary>
        /// Volume changed.
        /// </summary>
        public void VolumeChanged(object sender, EventArgs e) {
            Mixer.Volume = kermalisVolumeSlider.Value / 100f;
        }

        /// <summary>
        /// Loop changed.
        /// </summary>
        public void LoopChanged(object sender, EventArgs e) {
            Player.NumLoops = kermalisLoopBox.Checked ? 0xFFFFFFFF : 0;
        }

        /// <summary>
        /// Closing.
        /// </summary>
        public void SAClosing(object sender, FormClosingEventArgs e) {
            Player.Stop();
            Player.Dispose();
            Mixer.Dispose();
            Timer.Stop();
            Environment.Exit(Environment.ExitCode);
        }

        /// <summary>
        /// Key press.
        /// </summary>
        public void KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == ' ' && tree.SelectedNode.Parent != null) {
                if (tree.SelectedNode.Parent.Name == "sounds") {
                    PlayClick(sender, e);
                }
            }
        }

        #endregion

        //Sound playback.
        #region SoundPlayback

        /// <summary>
        /// Play a sequence.
        /// </summary>
        /// <param name="seq">Sequence to play.</param>
        public void PlaySequence(SequenceInfo seq) {
            if (seq.Bank == null) { MessageBox.Show("No valid bank is linked!"); return; }
            Player.PrepareForSong(new PlayableBank[] { seq.Bank.BankFile.GetFile() }, SA.GetWaveArchives(seq.Bank));
            var file = seq.SoundFile.GetFile();
            file.ReadCommandData();
            Player.LoadSong(file.Commands, file.ConvertOffset(seq.DataOffset));
            kermalisPosition.Maximum = (int)Player.MaxTicks;
            kermalisPosition.TickFrequency = kermalisPosition.Maximum / 10;
            kermalisPosition.LargeChange = kermalisPosition.Maximum / 20;
            Player.Play();
        }

        #endregion

    }

}

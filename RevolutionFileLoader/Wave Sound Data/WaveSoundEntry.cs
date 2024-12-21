using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Wave sound entry.
    /// </summary>
    public class WaveSoundEntry : IReadable, IWriteable {
        
        /// <summary>
        /// Info.
        /// </summary>
        public WaveSoundInfo Info;

        /// <summary>
        /// Tracks.
        /// </summary>
        public List<List<NoteEvent>> Tracks = null;

        /// <summary>
        /// Note Info.
        /// </summary>
        public List<NoteInfo> NoteInfo = null;

        /// <summary>
        /// Read the entry.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {

            //Read data.
            Info = r.Read<RReference<WaveSoundInfo>>().Data;
            var tracks = r.Read<RReference<Table<RReference<RReference<Table<RReference<NoteEvent>>>>>>>().Data;
            if (tracks != null) {
                var rawTracks = tracks.Select(x => x.Data);
                Tracks = new List<List<NoteEvent>>();
                foreach (var t in rawTracks) {
                    if (t.Data == null) {
                        Tracks.Add(null);
                    } else {
                        Tracks.Add(t.Data.Select(x => x.Data).ToList());
                    }
                }
            } else {
                Tracks = null;
            }
            var noteInfos = r.Read<RReference<Table<RReference<NoteInfo>>>>().Data;
            if (noteInfos == null) {
                NoteInfo = null;
            } else {
                NoteInfo = noteInfos.Select(x => x.Data).ToList();
            }

        }

        /// <summary>
        /// Write the entry.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {

            //Prepare references.
            RReference<WaveSoundInfo> infoRef = new RReference<WaveSoundInfo>() { Data = Info };
            infoRef.InitWrite(w);
            w.InitReference<RReference<object>>("tracksTableRef"); //tracks.InitWrite(w);
            w.InitReference<RReference<object>>("notesTableRef"); //noteInfo.InitWrite(w);
            infoRef.WriteData(w);

            //Tracks.
            if (Tracks != null) {
                w.CloseReference("tracksTableRef");
                w.Write((uint)Tracks.Count);
                for (int i = 0; i < Tracks.Count; i++) {
                    w.InitReference<RReference<object>>("track" + i);
                }
                for (int i = 0; i < Tracks.Count; i++) {
                    if (Tracks[i] != null) {
                        w.CloseReference("track" + i);
                        w.InitReference<RReference<object>>("eventTableRef");
                        w.CloseReference("eventTableRef");
                        w.Write((uint)Tracks[i].Count);
                        for (int j = 0; j < Tracks[i].Count; j++) {
                            w.InitReference<RReference<object>>("event" + i);
                        }
                        for (int j = 0; j < Tracks[i].Count; j++) {
                            if (Tracks[i][j] != null) {
                                w.CloseReference("event" + i);
                                w.Write(Tracks[i][j]);
                            } else {
                                w.CloseReference("event" + i, 0, false, -1);
                            }
                        }
                    } else {
                        w.CloseReference("track" + i, 0, false, -1);
                    }
                }
            } else {
                w.CloseReference("tracksTableRef", 0, false, -1);
            }

            //Note events.
            if (NoteInfo != null) {
                w.CloseReference("notesTableRef");
                w.Write((uint)NoteInfo.Count);
                for (int i = 0; i < NoteInfo.Count; i++) {
                    w.InitReference<RReference<object>>("note" + i);
                }
                for (int i = 0; i < NoteInfo.Count; i++) {
                    if (NoteInfo[i] != null) {
                        w.CloseReference("note" + i);
                        w.Write(NoteInfo[i]);
                    } else {
                        w.CloseReference("note" + i, 0, false, -1);
                    }
                }
            } else {
                w.CloseReference("notesTableRef", 0, false, -1);
            }

        }

    }

}

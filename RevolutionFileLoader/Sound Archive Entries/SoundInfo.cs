using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Shared sound info.
    /// </summary>
    public abstract class SoundInfo : IReadable, IWriteable {

        /// <summary>
        /// Name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Player.
        /// </summary>
        public PlayerInfo Player;

        /// <summary>
        /// Sound 3d info.
        /// </summary>
        public Sound3dInfo Sound3d;

        /// <summary>
        /// Volume.
        /// </summary>
        public byte Volume = 64;

        /// <summary>
        /// Player priority.
        /// </summary>
        public byte PlayerPriority = 64;

        /// <summary>
        /// Sound type.
        /// </summary>
        /// <returns>The sound type.</returns>
        public virtual byte SoundType() => 0;

        /// <summary>
        /// Remote filter.
        /// </summary>
        public byte RemoteFilter;

        /// <summary>
        /// User parameters.
        /// </summary>
        public uint[] UserParameters;

        /// <summary>
        /// Pan mode.
        /// </summary>
        public byte PanMode;

        /// <summary>
        /// Pan curve.
        /// </summary>
        public byte PanCurve;

        /// <summary>
        /// Actor player Id.
        /// </summary>
        public byte ActorPlayerId;

        /// <summary>
        /// String Id when reading.
        /// </summary>
        public Id ReadingStringId;

        /// <summary>
        /// File Id when reading.
        /// </summary>
        public Id ReadingFileId;

        /// <summary>
        /// Player Id when reading.
        /// </summary>
        public Id ReadingPlayerId;

        /// <summary>
        /// Supports pan stuff.
        /// </summary>
        public bool SupportsPanStuff = true;

        /// <summary>
        /// Read the sound info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public abstract void Read(FileReader r);

        /// <summary>
        /// Write the sound info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public abstract void Write(FileWriter w);

        /// <summary>
        /// Get the sound info.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <returns>The sound info.</returns>
        public static SoundInfo GetSoundInfo(FileReader r) {
            SoundInfo s;
            r.Position += 22;
            switch (r.ReadByte()) {
                case 1:
                    s = new SequenceInfo();
                    break;
                case 2:
                    s = new StreamInfo();
                    break;
                default:
                    s = new WaveSoundEntryInfo();
                    break;
            }
            r.Position -= 23;
            s.Read(r);
            return s;
        }

        /// <summary>
        /// Read the sound info.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void ReadSoundInfo(FileReader r) {
            ReadingStringId = r.Read<Id>();
            ReadingFileId = r.Read<Id>();
            ReadingPlayerId = r.Read<Id>();
            r.OpenReference<RReference<object>>("3dRef");
            Volume = r.ReadByte();
            PlayerPriority = r.ReadByte();
            r.ReadByte();
            RemoteFilter = r.ReadByte();
            r.OpenReference<RReference<object>>("SoundDet");
            UserParameters = r.ReadUInt32s(2);
            PanMode = r.ReadByte();
            PanCurve = r.ReadByte();
            ActorPlayerId = r.ReadByte();
            r.ReadByte();
            if (!r.ReferenceNull("3dRef")) {
                r.JumpToReference("3dRef");
                Sound3d = r.Read<Sound3dInfo>();
            }
            if (!r.ReferenceNull("SoundDet")) {
                r.JumpToReference("SoundDet");
            }
        }

        /// <summary>
        /// Write the sound info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void WriteSoundInfo(FileWriter w) {
            w.Write(ReadingStringId);
            w.Write(ReadingFileId);
            w.Write(ReadingPlayerId);
            w.InitReference<RReference<object>>("3dRef");
            w.Write(Volume);
            w.Write(PlayerPriority);
            w.Write(SoundType());
            w.Write(RemoteFilter);
            w.InitReference<RReference<object>>("SoundDet");
            w.Write(UserParameters);
            w.Write(SupportsPanStuff ? PanMode : (byte)1);
            w.Write(SupportsPanStuff ? PanCurve : (byte)0);
            w.Write(ActorPlayerId);
            w.Write((byte)0);
            w.CloseReference("SoundDet", SoundType());
        }

        /// <summary>
        /// Write 3d sound info.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void WriteSoundInfo3D(FileWriter w) {
            if (Sound3d == null) {
                w.CloseReference("3dRef", 0, false, 0);
            } else {
                w.CloseReference("3dRef");
                w.Write(Sound3d);
            }
        }

    }

}

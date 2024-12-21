using GotaSequenceLib;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// Note parameter.
    /// </summary>
    public class NoteParameter : IReadable, IWriteable {

        /// <summary>
        /// Minimum velocity.
        /// </summary>
        public byte MinVelocity;

        /// <summary>
        /// Maximum velocity.
        /// </summary>
        public byte MaxVelocity;

        /// <summary>
        /// Wave id.
        /// </summary>
        public int WaveId;

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
        /// Release.
        /// </summary>
        public byte Release = 127;

        /// <summary>
        /// Hold.
        /// </summary>
        public byte Hold;

        /// <summary>
        /// Percussion mode.
        /// </summary>
        public bool PercussionMode;

        /// <summary>
        /// Key group.
        /// </summary>
        public byte KeyGroup;

        /// <summary>
        /// Original key.
        /// </summary>
        public Notes OriginalKey = Notes.cn4;

        /// <summary>
        /// Volume.
        /// </summary>
        public byte Volume = 0x7F;

        /// <summary>
        /// Pan.
        /// </summary>
        public byte Pan = 60;

        /// <summary>
        /// Tune.
        /// </summary>
        public float Tune = 1f;

        /// <summary>
        /// Read the note parameter.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            WaveId = r.ReadInt32();
            Attack = r.ReadByte();
            Decay = r.ReadByte();
            Sustain = r.ReadByte();
            Release = r.ReadByte();
            Hold = r.ReadByte();
            if (r.ReadByte() != 0) { throw new Exception("Uhhhh this a weird way to reference waves, please contact Gota about this file."); }
            PercussionMode = r.ReadBoolean();
            KeyGroup = r.ReadByte();
            OriginalKey = (Notes)r.ReadByte();
            Volume = r.ReadByte();
            Pan = r.ReadByte();
            r.ReadByte();
            Tune = r.ReadSingle();
            r.ReadUInt64s(3);
            r.ReadUInt32();
        }

        /// <summary>
        /// Write the note parameter.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            w.Write(WaveId);
            w.Write(Attack);
            w.Write(Decay);
            w.Write(Sustain);
            w.Write(Release);
            w.Write(Hold);
            w.Write((byte)0);
            w.Write(PercussionMode);
            w.Write(KeyGroup);
            w.Write((byte)OriginalKey);
            w.Write(Volume);
            w.Write(Pan);
            w.Write((byte)0);
            w.Write(Tune);
            w.Write(new UInt64[3]);
            w.Write((uint)0);
        }

        /// <summary>
        /// If this equals another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>If the objects are equal.</returns>
        public override bool Equals(object obj) {
            var n = obj as NoteParameter;
            if (n == null) {
                return false;
            }
            return WaveId == n.WaveId && Attack == n.Attack && Decay == n.Decay && Sustain == n.Sustain && Release == n.Release && Hold == n.Hold && PercussionMode == n.PercussionMode && KeyGroup == n.KeyGroup && OriginalKey == n.OriginalKey && Volume == n.Volume && Pan == n.Pan && Tune == n.Tune;
        }

        /// <summary>
        /// The hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() {
            return WaveId.GetHashCode() * Attack.GetHashCode() * Decay.GetHashCode() * Sustain.GetHashCode() * Release.GetHashCode() * Hold.GetHashCode() * PercussionMode.GetHashCode() * KeyGroup.GetHashCode() * OriginalKey.GetHashCode() * Volume.GetHashCode() * Pan.GetHashCode() * Tune.GetHashCode();
        }

    }

}

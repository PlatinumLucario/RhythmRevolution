using GotaSequenceLib;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// An instrument.
    /// </summary>
    public abstract class KeyRegion : IReadable, IWriteable {

        /// <summary>
        /// Key region minimum key.
        /// </summary>
        public byte MinKey;

        /// <summary>
        /// Key region maximum key.
        /// </summary>
        public byte MaxKey;

        /// <summary>
        /// Regions.
        /// </summary>
        public List<NoteParameter> VelocityRegions = new List<NoteParameter>();

        /// <summary>
        /// Null regions.
        /// </summary>
        public List<Tuple<byte, byte>> NullRegions = new List<Tuple<byte, byte>>();

        /// <summary>
        /// Read the key region.
        /// </summary>
        /// <param name="r">The reader.</param>
        public abstract void Read(FileReader r);

        /// <summary>
        /// Write the key region.
        /// </summary>
        /// <param name="w">The writer.</param>
        public abstract void Write(FileWriter w);

        /// <summary>
        /// Region type.
        /// </summary>
        /// <returns>The reference type.</returns>
        public abstract RegionType ReferenceType();

        /// <summary>
        /// If the key region equals another.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>If the objects are equal.</returns>
        public override bool Equals(object obj) {

            //As key region.
            var k = obj as KeyRegion;
            if (k == null) {
                return false;
            }

            //Compare counts.
            if (VelocityRegions.Count != k.VelocityRegions.Count || NullRegions.Count != k.NullRegions.Count) {
                return false;
            }

            //Compare each entry.
            for (int i = 0; i < VelocityRegions.Count; i++) {
                if (!VelocityRegions[i].Equals(k.VelocityRegions[i])) {
                    return false;
                }
            }
            for (int i = 0; i < NullRegions.Count; i++) {
                if (NullRegions[i].Item1 != k.NullRegions[i].Item1 || NullRegions[i].Item2 != k.NullRegions[i].Item2) {
                    return false;
                }
            }

            //They match.
            return true;

        }

        /// <summary>
        /// Get the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() {
            int hash = VelocityRegions.Count * NullRegions.Count;
            foreach (var v in VelocityRegions) {
                hash *= v.GetHashCode();
            }
            foreach (var n in NullRegions) {
                hash *= n.Item1 * n.Item2;
            }
            return hash;
        }

        /// <summary>
        /// Get the note parameter from a velocity.
        /// </summary>
        /// <param name="velocity">Velocity.</param>
        /// <returns>Note parameter.</returns>
        public NoteParameter GetNoteParameter(byte velocity) {

            //Find matching.
            for (int i = 0; i < VelocityRegions.Count; i++) {
                if (velocity >= VelocityRegions[i].MinVelocity && velocity <= VelocityRegions[i].MaxVelocity) {
                    return VelocityRegions[i];
                }
            }

            //Default.
            return null;

        }

    }

    /// <summary>
    /// Region type.
    /// </summary>
    public enum RegionType : byte {
        Invalid = 0,
        Direct = 1,
        Range = 2,
        Index = 3,
        Null = 4 //Unused but planned?
    }

    /// <summary>
    /// Velocity region reference.
    /// </summary>
    public class VelocityRegionRef : RReference<NoteParameter> {
        public override List<Type> DataTypes => new List<Type>() { null, typeof(NoteParameter) };
    }

}

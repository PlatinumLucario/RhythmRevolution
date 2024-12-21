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
    public abstract class Instrument : IReadable, IWriteable {

        /// <summary>
        /// Instrument index.
        /// </summary>
        public int Index;

        /// <summary>
        /// Regions.
        /// </summary>
        public List<KeyRegion> Regions = new List<KeyRegion>();

        /// <summary>
        /// Null regions.
        /// </summary>
        public List<Tuple<byte, byte>> NullRegions = new List<Tuple<byte, byte>>();

        /// <summary>
        /// Read the instrument.
        /// </summary>
        /// <param name="r">The reader.</param>
        public abstract void Read(FileReader r);

        /// <summary>
        /// Write the instrument.
        /// </summary>
        /// <param name="w">The writer.</param>
        public abstract void Write(FileWriter w);

        /// <summary>
        /// Reference type.
        /// </summary>
        /// <returns>The reference type.</returns>
        public abstract InstrumentType ReferenceType();

    }

    /// <summary>
    /// Instrument type.
    /// </summary>
    public enum InstrumentType : byte {
        Invalid = 0,
        Direct = 1,
        Range = 2,
        Index = 3,
        Null = 4 //Unused but planned?
    }

    /// <summary>
    /// Key region reference.
    /// </summary>
    public class KeyRegionRef : RReference<KeyRegion> {
        public override List<Type> DataTypes => new List<Type>() { null, typeof(DirectKeyRegion), typeof(RangeKeyRegion), typeof(IndexKeyRegion) };
    }

}

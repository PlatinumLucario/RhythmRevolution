using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// A direct instrument.
    /// </summary>
    public class DirectInstrument : Instrument {

        /// <summary>
        /// Read the instrument.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            Regions = new List<KeyRegion>();
            Regions.Add(r.Read<DirectKeyRegion>());
            Regions[0].MinKey = 0;
            Regions[0].MaxKey = 0x7F;
        }

        /// <summary>
        /// Write the instrument.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            if (Regions[0] as DirectKeyRegion == null) { throw new Exception("A direct instrument must contain a direct key region!"); }
            w.Write(Regions[0]);
        }

        /// <summary>
        /// Direct type.
        /// </summary>
        /// <returns>The type.</returns>
        public override InstrumentType ReferenceType() => InstrumentType.Direct;

    }

}

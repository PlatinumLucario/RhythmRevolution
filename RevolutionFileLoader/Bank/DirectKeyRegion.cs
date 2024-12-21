using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// A direct key region.
    /// </summary>
    public class DirectKeyRegion : KeyRegion {

        /// <summary>
        /// Read the instrument.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {
            VelocityRegions = new List<NoteParameter>();
            VelocityRegions.Add(r.Read<NoteParameter>());
            VelocityRegions[0].MinVelocity = 0;
            VelocityRegions[0].MaxVelocity = 0x7F;
        }

        /// <summary>
        /// Write the instrument.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {
            w.Write(VelocityRegions[0]);
        }

        /// <summary>
        /// Direct type.
        /// </summary>
        /// <returns>The type.</returns>
        public override RegionType ReferenceType() => RegionType.Direct;

    }

}

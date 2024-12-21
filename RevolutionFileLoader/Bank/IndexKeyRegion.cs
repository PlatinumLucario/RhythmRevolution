using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// Index key region.
    /// </summary>
    public class IndexKeyRegion : KeyRegion {

        /// <summary>
        /// Read the key region.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read data.
            byte min = r.ReadByte();
            byte max = r.ReadByte();
            int num = max - min + 1;
            r.ReadUInt16();
            VelocityRegions = new List<NoteParameter>();
            NullRegions = new List<Tuple<byte, byte>>();
            for (int i = 0; i < num; i++) {
                VelocityRegions.Add(r.Read<VelocityRegionRef>().Data);
                if (VelocityRegions.Last() != null) {
                    VelocityRegions.Last().MinVelocity = (byte)(min + i);
                    VelocityRegions.Last().MaxVelocity = (byte)(min + i);
                    if (VelocityRegions.Count > 1) {
                        if (VelocityRegions.Last().Equals(VelocityRegions[VelocityRegions.Count - 2])) {
                            VelocityRegions.RemoveAt(VelocityRegions.Count - 2);
                            VelocityRegions.Last().MinVelocity--;
                        }
                    }
                } else {
                    NullRegions.Add(new Tuple<byte, byte>((byte)(min + i), (byte)(min + i)));
                    VelocityRegions.Remove(VelocityRegions.Last());
                }
            }

        }

        /// <summary>
        /// Write the instrument.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Get regions.
            VelocityRegions = VelocityRegions.OrderBy(x => x.MinVelocity).ToList();
            NullRegions = NullRegions.OrderBy(x => x.Item1).ToList();
            List<VelocityRegionRef> regs = new List<VelocityRegionRef>();
            byte min = 0;
            byte max = 127;
            if (NullRegions.Count > 0) {
                min = NullRegions[0].Item1;
                max = NullRegions.Last().Item2;
            }
            if (VelocityRegions.Count > 0 && VelocityRegions[0].MinVelocity < min) {
                min = VelocityRegions[0].MinVelocity;
            }
            if (VelocityRegions.Count > 0 && VelocityRegions.Last().MaxVelocity > max) {
                max = VelocityRegions.Last().MaxVelocity;
            }
            w.Write(min);
            w.Write(max);
            w.Write((ushort)0);
            for (int i = min; i <= max; i++) {
                NoteParameter data = null;
                var reg = VelocityRegions.Where(x => x.MinVelocity <= i && x.MaxVelocity >= i).FirstOrDefault();
                if (reg != null) {
                    data = reg;
                }
                regs.Add(new VelocityRegionRef() { Data = data });
                regs.Last().InitWrite(w);
            }
            foreach (var r in regs) {
                r.WriteData(w);
            }

        }

        /// <summary>
        /// Index key region.
        /// </summary>
        /// <returns>The key region type.</returns>
        public override RegionType ReferenceType() => RegionType.Index;

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// Index instrument.
    /// </summary>
    public class IndexInstrument : Instrument {

        /// <summary>
        /// Read the instrument.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read data.
            byte min = r.ReadByte();
            byte max = r.ReadByte();
            int num = max - min + 1;
            r.ReadUInt16();
            Regions = new List<KeyRegion>();
            NullRegions = new List<Tuple<byte, byte>>();
            for (int i = 0; i < num; i++) {
                Regions.Add(r.Read<KeyRegionRef>().Data);
                if (Regions.Last() != null) {
                    Regions.Last().MinKey = (byte)(min + i);
                    Regions.Last().MaxKey = (byte)(min + i);
                    if (Regions.Count > 1) {
                        if (Regions.Last().Equals(Regions[Regions.Count - 2])) {
                            Regions.RemoveAt(Regions.Count - 2);
                            Regions.Last().MinKey--;
                        }
                    }
                } else {
                    NullRegions.Add(new Tuple<byte, byte>((byte)(min + i), (byte)(min + i)));
                    Regions.Remove(Regions.Last());
                }
            }

        }

        /// <summary>
        /// Write the instrument.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Get regions.
            Regions = Regions.OrderBy(x => x.MinKey).ToList();
            NullRegions = NullRegions.OrderBy(x => x.Item1).ToList();
            List<KeyRegionRef> regs = new List<KeyRegionRef>();
            byte min = 0;
            byte max = 127;
            if (NullRegions.Count > 0) {
                min = NullRegions[0].Item1;
                max = NullRegions.Last().Item2;
            }
            if (Regions.Count > 0 && Regions[0].MinKey < min) {
                min = Regions[0].MinKey;
            }
            if (Regions.Count > 0 && Regions.Last().MaxKey > max) {
                max = Regions.Last().MaxKey;
            }
            w.Write(min);
            w.Write(max);
            w.Write((ushort)0);
            for (int i = min; i <= max; i++) {
                KeyRegion data = null;
                var reg = Regions.Where(x => x.MinKey <= i && x.MaxKey >= i).FirstOrDefault();
                if (reg != null) {
                    data = reg;
                }
                regs.Add(new KeyRegionRef() { Data = data });
                regs.Last().InitWrite(w);
            }
            foreach (var r in regs) {
                r.WriteData(w);
            }

        }

        /// <summary>
        /// Index instrument.
        /// </summary>
        /// <returns>The instrument type.</returns>
        public override InstrumentType ReferenceType() => InstrumentType.Index;

    }

}

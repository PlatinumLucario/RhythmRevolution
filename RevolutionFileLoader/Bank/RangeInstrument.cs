using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace RevolutionFileLoader {

    /// <summary>
    /// Range instrument.
    /// </summary>
    public class RangeInstrument : Instrument {

        /// <summary>
        /// Read the instrument.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r) {

            //Read data.
            byte numInsts = r.ReadByte();
            byte[] regions = r.ReadBytes(numInsts);
            r.Align(0x4);

            //Get regions.
            byte last = 0;
            Regions = new List<KeyRegion>();
            NullRegions = new List<Tuple<byte, byte>>();
            for (int i = 0; i < numInsts; i++) {
                Regions.Add(r.Read<KeyRegionRef>().Data);
                if (Regions[i] != null) {
                    Regions[i].MinKey = last;
                    Regions[i].MaxKey = regions[i];
                } else {
                    NullRegions.Add(new Tuple<byte, byte>(last, regions[i]));
                }
                last = (byte)(regions[i] + 1);
            }
            Regions = Regions.Where(x => x != null).ToList();

        }

        /// <summary>
        /// Write the instrument.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w) {

            //Set stuff up.
            Regions = Regions.OrderBy(x => x.MinKey).ToList();
            NullRegions = NullRegions.OrderBy(x => x.Item1).ToList();

            //Minimum and maximum ranges.
            List<Tuple<byte, byte>> minMaxes = new List<Tuple<byte, byte>>();
            foreach (var r in Regions) {
                minMaxes.Add(new Tuple<byte, byte>(r.MinKey, r.MaxKey));
            }

            //Get regions.
            List<int> nullRegions = new List<int>();
            Dictionary<int, int> newRegionIndices = new Dictionary<int, int>();
            List<byte> regions = new List<byte>();
            byte last = 0;
            for (int i = 0; i < minMaxes.Count; i++) {
                if (minMaxes[i].Item1 != last) {
                    nullRegions.Add((byte)regions.Count);
                    var nullReg = NullRegions.Where(x => x.Item1 == last).FirstOrDefault();
                    if (nullReg != null) {
                        regions.Add(nullReg.Item2);
                        last = (byte)(nullReg.Item2 + 1);
                    } else {
                        regions.Add(127);
                        last = 128;
                    }
                } else {
                    newRegionIndices.Add(regions.Count, i);
                    regions.Add(minMaxes[i].Item2);
                    last = (byte)(minMaxes[i].Item2 + 1);
                }
            }

            //Add regions.
            w.Write((byte)regions.Count);
            w.Write(regions.ToArray());
            w.Align(0x4);
            List<KeyRegionRef> refs = new List<KeyRegionRef>();
            for (int i = 0; i < regions.Count; i++) {
                refs.Add(new KeyRegionRef());
                refs.Last().InitWrite(w);
                if (!nullRegions.Contains(i)) {
                    refs.Last().Data = Regions[newRegionIndices[i]];
                }
            }
            for (int i = 0; i < refs.Count; i++) {
                refs[i].WriteData(w);
            }

        }

        /// <summary>
        /// Range instrument.
        /// </summary>
        /// <returns>The instrument type.</returns>
        public override InstrumentType ReferenceType() => InstrumentType.Range;

    }

}

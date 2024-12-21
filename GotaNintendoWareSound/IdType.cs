using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare {
    
    /// <summary>
    /// Contains an Id and a type.
    /// </summary>
    public class IdType : IReadable, IWriteable {

        /// <summary>
        /// Type of item.
        /// </summary>
        public byte Type;

        /// <summary>
        /// Id.
        /// </summary>
        public Int24 Id;

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(FileReader r) {
            uint val = r.ReadUInt32();
            Type = (byte)((val & 0xFF000000) >> 24);
            int id = (int)(val & 0xFFFFFF);
            Id = (Int24)((val & 0x800000) > 0 ? (Int24.MinValue + (id & 0x7FFFFF)) : id);
        }

        /// <summary>
        /// Write the item.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(FileWriter w) {
            uint ret = 0;
            
            w.Write(ret);
        }

    }

}

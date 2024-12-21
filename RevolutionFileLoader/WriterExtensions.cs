using GotaSoundIO;
using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {
    
    /// <summary>
    /// Writer extensions.
    /// </summary>
    public static class WriterExtensions {

        /// <summary>
        /// Table depth.
        /// </summary>
        private static int TableDepth = 0;

        /// <summary>
        /// Write a table with references.
        /// </summary>
        /// <typeparam name="T">Type to write.</typeparam>
        /// <param name="table">List of items.</param>
        public static void WriteTableWithReferences<T>(this FileWriter w, List<T> table) where T : IWriteable {

            //Write stuff.
            TableDepth++;
            w.Write((uint)table.Count);
            for (int i = 0; i < table.Count; i++) {
                w.InitReference<RReference<object>>("T" + TableDepth + "I" + i);
            }
            for (int i = 0; i < table.Count; i++) {
                if (table[i] == null) {
                    w.CloseReference("T" + TableDepth + "I" + i, 0, false, 0);
                } else {
                    w.CloseReference("T" + TableDepth + "I" + i);
                    w.Write(table[i]);
                }
            }
            TableDepth--;

        }

        /// <summary>
        /// Write a table with offsets.
        /// </summary>
        /// <typeparam name="T">Type to write.</typeparam>
        /// <param name="table">List of items.</param>
        public static void WriteTableWithOffsets<T>(this FileWriter w, List<T> table) where T : IWriteable {

            //Write stuff.
            TableDepth++;
            w.Write((uint)table.Count);
            for (int i = 0; i < table.Count; i++) {
                w.InitOffset("T" + TableDepth + "I" + i);
            }
            for (int i = 0; i < table.Count; i++) {
                if (table[i] == null) {
                    w.CloseOffset("T" + TableDepth + "I" + i, false, 0);
                } else {
                    w.CloseOffset("T" + TableDepth + "I" + i);
                    w.Write(table[i]);
                }
            }
            TableDepth--;

        }

    }

}

using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare {

    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions {

        /// <summary>
        /// Read a platform changeable item.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="reader">File reader.</param>
        /// <param name="platform">Platform.</param>
        /// <returns>Item with the correct platform.</returns>
        public static T Read<T>(this FileReader reader, Platform platform) where T : IPlatformChangeable {
            T ret = Activator.CreateInstance<T>();
            ret.Read(reader, platform);
            return ret;
        }

        /// <summary>
        /// Read a platform changeable file.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="reader">File reader.</param>
        /// <param name="platform">Platform.</param>
        /// <returns>Item with the correct platform.</returns>
        public static T ReadChangeableFile<T>(this FileReader reader, Platform platform) where T : IOFile, IPlatformChangeable {
            FileReader r = new FileReader(reader.BaseStream);
            r.Position = reader.Position;
            r.CurrentOffset = r.Position;
            r.FileOffset = r.Position;
            T f = r.Read<T>(platform);
            reader.Position = r.Position;
            return f;
        }

        /// <summary>
        /// Write a platform changeable item.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="writer">Writer.</param>
        /// <param name="item">Item.</param>
        /// <param name="platform">Platform.</param>
        public static void Write<T>(this FileWriter writer, T item, Platform platform) where T : IPlatformChangeable {
            item.Write(writer, platform);
        }

        /// <summary>
        /// Write a platform changeable file.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="writer">Writer.</param>
        /// <param name="item">Item.</param>
        /// <param name="platform">Platform.</param>
        public static void WriteChangeableFile<T>(this FileWriter writer, T item, Platform platform) where T : IOFile, IPlatformChangeable {
            using (MemoryStream o = new MemoryStream()) {
                using (FileWriter w = new FileWriter(o)) {
                    item.Write(w, platform);
                    writer.Write(o.ToArray());
                }
            }
        }

    }

}

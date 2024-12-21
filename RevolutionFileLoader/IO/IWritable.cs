using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// A readable item.
    /// </summary>
    public interface IWritable {

        /// <summary>
        /// Write the item.
        /// </summary>
        /// <param name="w">The file writer.</param>
        void Write(FileWriter w);

    }

}

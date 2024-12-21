using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare {
    
    /// <summary>
    /// A sound archive item.
    /// </summary>
    public interface ISoundArchiveItem : IPlatformChangeable {

        /// <summary>
        /// Name of the item.
        /// </summary>
        int NameId { get; set; }

        /// <summary>
        /// Item Id.
        /// </summary>
        int Id { get; set; }

    }

}

﻿using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Wave item.
    /// </summary>
    public class WaveItem : SoundItem {

        /// <summary>
        /// Read the item.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="platform">Platform.</param>
        public override void Read(FileReader r, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {

            }

            //Revolution.
            else if (platform == Platform.Revolution) {

            }

            //CTR, Cafe, NX.
            else {

            }

        }

        /// <summary>
        /// Write the platform.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="platform">Platform.</param>
        public override void Write(FileWriter w, Platform platform) {

            //Nitro.
            if (platform == Platform.Nitro) {

            }

            //Revolution.
            else if (platform == Platform.Revolution) {

            }

            //CTR, Cafe, NX.
            else {

            }

        }

    }

}

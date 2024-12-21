using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionFileLoader {

    /// <summary>
    /// A streamed audio file.
    /// </summary>
    public abstract class StreamedAudioFile : SoundFile {

        /// <summary>
        /// Get the supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public abstract Type[] SupportedEncodings();

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public StreamedAudioFile() {}

        /// <summary>
        /// Read an audio file from a file path.
        /// </summary>
        /// <param name="filePath">The file to read.</param>
        public StreamedAudioFile(string filePath) : base(filePath) {}

        /// <summary>
        /// Channels.
        /// </summary>
        public List<AudioEncoding> Channels = new List<AudioEncoding>();

        /// <summary>
        /// If the audio loops.
        /// </summary>
        public bool Loops;

        /// <summary>
        /// Start loop.
        /// </summary>
        public uint LoopStart;

        /// <summary>
        /// Ending loop.
        /// </summary>
        public uint LoopEnd;

        /// <summary>
        /// The sample rate of the audio file.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Tracks.
        /// </summary>
        public List<TrackData> Tracks = new List<TrackData>();

        /// <summary>
        /// Track data.
        /// </summary>
        public class TrackData {

            /// <summary>
            /// Volume.
            /// </summary>
            public byte Volume = 127;

            /// <summary>
            /// Pan.
            /// </summary>
            public byte Pan = 0x40;

            /// <summary>
            /// Channels.
            /// </summary>
            public List<int> Channels = new List<int>();

        }

        /// <summary>
        /// When converting from another streamed audio file.
        /// </summary>
        public abstract void OnConversion();

        /// <summary>
        /// Convert from another streamed audio file.
        /// </summary>
        /// <param name="other">The other file converted from.</param>
        public void FromOtherStreamFile(StreamedAudioFile other) {

            //Set data.

            //If any of the encodings match then that's good.
            var e = SupportedEncodings();
            foreach (var t in other.SupportedEncodings()) {
                foreach (var f in e) {
                    if (f.Equals(t)) {
                        OnConversion();
                        return;
                    }
                }
            }

            //By default convert from PCM16.
            Channels = AudioEncoding.ConvertChannels(typeof(PCM16), other.Channels);

            //Activate on conversion hook.
            OnConversion();

        }

    }

}

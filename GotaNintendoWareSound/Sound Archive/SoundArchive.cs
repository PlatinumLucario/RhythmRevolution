using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {

    /// <summary>
    /// Sound archive.
    /// </summary>
    public abstract class SoundArchive : IOFile {

        /// <summary>
        /// Symbols.
        /// </summary>
        public List<string> Symbols = null;

        /// <summary>
        /// Sounds.
        /// </summary>
        public List<SoundItem> Sounds = new List<SoundItem>();

        /// <summary>
        /// Wave sound sets.
        /// </summary>
        public List<WaveSoundSetItem> WaveSoundSets = new List<WaveSoundSetItem>();

        /// <summary>
        /// Sequence archives.
        /// </summary>
        public List<SequenceArchiveItem> SequenceArchives = new List<SequenceArchiveItem>();

        /// <summary>
        /// Banks.
        /// </summary>
        public List<BankItem> Banks = new List<BankItem>();

        /// <summary>
        /// Wave archives.
        /// </summary>
        public List<WaveArchiveItem> WaveArchives = new List<WaveArchiveItem>();

        /// <summary>
        /// Players.
        /// </summary>
        public List<PlayerItem> Players = new List<PlayerItem>();

        /// <summary>
        /// Groups.
        /// </summary>
        public List<GroupItem> Groups = new List<GroupItem>();

        /// <summary>
        /// Stream players.
        /// </summary>
        public List<StreamPlayerItem> StreamPlayers = new List<StreamPlayerItem>();

        /// <summary>
        /// Files.
        /// </summary>
        public List<FileItem> Files = new List<FileItem>();

        /// <summary>
        /// Project info.
        /// </summary>
        public ProjectInfo ProjectInfo = new ProjectInfo();

        /// <summary>
        /// Convert the sound archive to another platform.
        /// </summary>
        /// <param name="platform">Platform to convert to.</param>
        /// <returns>Converted sound archive.</returns>
        public abstract SoundArchive Convert(Platform platform);

    }

}

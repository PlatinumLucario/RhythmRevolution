using GotaSequenceLib;
using GotaSoundIO.IO;
using GotaSoundIO.Sound;
using RevolutionFileLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmRevolution {

    /// <summary>
    /// Main program.
    /// </summary>
    static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {

            //var s = new SoundArchive("sample.brsar");
            //s.Write("sampleOUT.brsar");

            /*RevolutionStream s = new RevolutionStream("e6.brstm");
            RiffWave w = new RiffWave();
            w.FromOtherStreamFile(s);
            w.Write("e6_OUT.wav");*/

            //Sequence s = new Sequence("sample2.brseq");
            //File.WriteAllLines("sample2.rseq", s.ToText());
            //s.SaveMIDI("twinkle_song.mid");

            //var s = new Sequence();
            //s.FromMIDI("City.mid");
            //s.Name = "City.rseq";
            //File.WriteAllLines("City.rseq", s.ToText());

            //RevolutionWave w = new RevolutionWave("test.brwav");
            //SoundArchive s = new SoundArchive("sample.brsar");
            //SoundArchive s = new SoundArchive("wii_mj2d_sound.brsar");
            /*List<string> t = File.ReadAllLines("sample.rseq").ToList();
            SequenceFile s = new Sequence();
            s.FromText(t);
            s.WriteCommandData();
            s.Write("sampleOUT.brseq");*/

            //Run main window.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

        }
    }
}

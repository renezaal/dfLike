using DfLike.Map;
using DfLike.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DfLike
{
    static class DwarfishBliss
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            GUIThread = new Thread(new ThreadStart(RunGUI));
            GUIThread.SetApartmentState(ApartmentState.STA);
            GUIThread.Start();

            ModLoader.Reload();

            while (GUIThread.IsAlive)
            {
                MainLoop();
            }

            Debug.WriteLine("Main thread ended.");
        }

        static Thread GUIThread;
        static Thread WorldThread;
        static Thread MapThread;
        static Thread DwarfAIThread;
        static Thread CreatureAIThread;
        static Thread BattleThread;

        static void RunGUI()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
            Debug.WriteLine("GUI thread ended.");
        }

        static void MainLoop()
        {
                Thread.Sleep(1000);
        }
    }
}

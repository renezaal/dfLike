using DfLike.Mods;
using System;
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

            Console.WriteLine("Main thread ended.");
        }

        static Thread GUIThread;        // screen updates; ON 100% of the time
        static Thread WorldThread;      // world events like draught, volcanoes and tsunami's, but also civilization events like alliances, wars, assassinations and revolts; ON 5% of the time
        static Thread MapThread;        // items ticking, fluids, map events, block updates, plant growth, seasonal changes; ON 50% of the time
        static Thread DwarfAIThread;    // pathfinding, decision making, hunger, pregnancy, thirst, body temperature, health, sickness, etc...; ON 100% of the time
        static Thread CreatureAIThread; // simple (short range) pathfinding, decision making, hunger, pregnancy, thirst, body temperature; ON 50% of the time
        static Thread BattleThread;     // fighting, applying wounds, removing limbs; ON 100% of the time when there is at least one fight
        // the threaded model is based around 4+ logical CPU cores. When more than 4 cores are available, threads like the Dwarf AI thread should yield their work to multiple child threads. 
        // threads that should be the easiest to subdivide are the AI threads, with the focus on the pathfinding. 

        // the percentages are indications of the time a thread should spend not idle when taxed to full capacity.
        // the limits are introduced to divide the processor time among the threads in an equal priority environment. 

        // not listed above is the main thread, this is the thread that manages the game logic itself such as game ticks, constructing and disposing resources and acting as message pump between threads where needed.


        static void RunGUI()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
            Console.WriteLine("GUI thread ended.");
        }

        static void MainLoop()
        {
            Thread.Sleep(1000);
        }
    }
}

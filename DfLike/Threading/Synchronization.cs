using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DfLike.Threading
{
    public static class Synchronization
    {
        private static Thread _managingThread = null;
        public static Tick GameTick { get; private set; }
        public static Tick MapTick { get; private set; }
        public static Tick WorldTick { get; private set; }
        internal static void SetManagingThread()
        {
            if (_managingThread != null)
            {
                return;
            }
            _managingThread = Thread.CurrentThread;

            DateTime start = DateTime.Now;

            GameSpeed = 1;

            GameTick = new Tick(0, start, start.AddSeconds(0.1));
            MapTick = new Tick(0, start, start.AddSeconds(0.2));
            WorldTick = new Tick(0, start, start.AddSeconds(30));
        }

        public static double GameSpeed { get; set; }

        internal static void AdvanceTick()
        {
            if (Thread.CurrentThread != _managingThread || GameSpeed <= 0)
            {
                return;
            }

            DateTime checkTime = DateTime.Now;

            GameTick = advanceTick(GameTick, 0.1, checkTime);
            MapTick = advanceTick(MapTick, 0.2, checkTime);
            WorldTick = advanceTick(WorldTick, 30, checkTime);
        }

        private static Tick advanceTick(Tick tick, double tickDuration, DateTime checkTime)
        {
            if (tick.ScheduledEnd > checkTime)
            {
                return tick;
            }
            return new Tick(tick.Number + 1, checkTime, checkTime.AddSeconds(tickDuration / GameSpeed));
        }
    }
}

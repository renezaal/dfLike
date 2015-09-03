using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.Threading
{
   public class Tick
    {
       internal Tick(ulong nr, DateTime start, DateTime scheduledEnd) {
           Number = nr;
           Start = start;
           ScheduledEnd = scheduledEnd;
       }
       public ulong Number { get; private set; }
       public DateTime Start { get; private set; }
       public DateTime ScheduledEnd { get; private set; }
    }
}

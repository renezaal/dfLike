using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.World
{
    public class World
    {
        public World(ulong size) { }

        // the seed used for the generation of this world
        private string _stringSeed;
        public string Seed
        {
            get { return _stringSeed; }
            private set
            {
                _stringSeed = value;
                _seed = _stringSeed.GetHashCode();
            }
        }
        private int _seed;


        // works with cubic chunks
        // chunks can be kept active by external classes
        // chunks can be loaded by external classes
        // chunks not kept active are loaded as needed using a predictive model and the actual view
        // chunks are kept loaded for as long as possible until the tickrate is compromised or the memory becomes too full
        // the memory is too full when more than 80% of the physical memory is taken and at least 60% of the taken memory is taken by this application
        // when a not active chunk is being saved, it goes into read-only mode, saves fully to disk, then is either unloaded or made writeable depending on usage. An actively used chunk will not unload unless there is a dire need for free memory. 
    }
}

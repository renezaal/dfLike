using DfLike.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.Map
{
    public abstract class DwarfMapDefinition : IModBase
    {
        public void Test(string input) { Console.WriteLine("Mapdefinition received something: {0}", input); }
        public abstract string GetMapDefinitionName();

        public abstract string Author { get; }
        public abstract string ModName { get; }
        public abstract uint VersionNumber { get; }
        public abstract string Version { get; }
    }
}

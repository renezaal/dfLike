using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.Mods
{
    interface IModBase
    {
        string Author{get;}
        string ModName { get; }
        uint VersionNumber { get; }
        string Version { get; }
    }
}

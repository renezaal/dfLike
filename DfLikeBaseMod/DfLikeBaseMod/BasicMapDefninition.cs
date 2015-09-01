using DfLike.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLikeBaseMod
{
    /// <summary>
    /// The basic map for the game.
    /// At this point no more than an example mod. 
    /// </summary>
    public class BasicMapDefninition:DwarfMapDefinition
    {

        public override string GetMapDefinitionName()
        {
            return "Basic map definition";
        }

        public override string Author
        {
            get { return "Epiphaner"; }
        }

        public override string ModName
        {
            get { return "Dwarf Fortress Like base mod"; }
        }

        public override uint VersionNumber
        {
            get { return 1; }
        }

        public override string Version
        {
            get { return "V0.0001 or alpha1 or whatever you want to call this"; }
        }
    }
}

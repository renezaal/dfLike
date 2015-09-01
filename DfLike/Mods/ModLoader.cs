using DfLike.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.Mods
{
    static class ModLoader
    {
        // all map instances loaded from the mods folder
        // map instances are not actual maps but 
        private static DwarfMapDefinition[] _mapDefinitions = new DwarfMapDefinition[0];
        internal static List<DwarfMapDefinition> GetMapDefinitions() { return new List<DwarfMapDefinition>(_mapDefinitions); }
        internal static int NumberOfLoadedMods { get; private set; }
        internal static void Reload()
        {
            NumberOfLoadedMods = 0;
            Console.WriteLine("Loading mods");
            // prepare lists for the different kinds of object we'll encounter in the folder
            List<DwarfMapDefinition> mapDefinitions = new List<DwarfMapDefinition>();
            // get the path for the mods directory
            string modsDirectoryPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Mods");
            // get the paths of the potential mods
            string[] files = Directory.GetFiles(modsDirectoryPath, "*.dll");

            // check each potential mod
            foreach (string file in files)
            {
                // load the assembly
                Assembly DLL = Assembly.LoadFile(file);

                // go through the public types in the assembly
                foreach (Type type in DLL.GetExportedTypes())
                {
                    try
                    {
                        // if the type can be cast to a map, create a new map instance based on that type
                        if (typeof(DwarfMapDefinition).IsAssignableFrom(type))
                        {
                            DwarfMapDefinition mapDefinition = Activator.CreateInstance(type) as DwarfMapDefinition;
                            if (mapDefinition != null)
                            {
                                // each map definition counts as a mod
                                NumberOfLoadedMods++;
                                Console.WriteLine("Loaded: {0}\nAuthor: {1}\nVersion: {2}", mapDefinition.ModName, mapDefinition.Author, mapDefinition.Version);
                                // if it succeeded, add it to the list of maps
                                mapDefinitions.Add(mapDefinition);
                            }
                        }

                    }
                    catch (Exception)
                    {
                        // nothing bad should happen when a type can not get instantiated or has other problems...
                    }

                }
            }

            // save the acquired maps in the local array
            _mapDefinitions = mapDefinitions.ToArray();
            // report the number of loaded mods
            Console.WriteLine("Total mods loaded: {0}", NumberOfLoadedMods);
        }
    }
}

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
        // list of all loaded mods
        // the key in the outermost dictionary is the author
        // the list under each namespace is the mod name
        private static Dictionary<string, List<string>> _loadedMods = new Dictionary<string, List<string>>();
        // list of instances of the loaded mods
        // the keys are <author>.<mod name>
        private static Dictionary<string, IModBase> _loadedModInstances = new Dictionary<string, IModBase>();
        // all map instances loaded from the mods folder
        // map instances are not actual maps but definitions of maps, each instance can contain a map
        private static Dictionary<string, string> _loadedModFolderPaths = new Dictionary<string, string>();
        private static MapDefinition[] _mapDefinitions = new MapDefinition[0];
        internal static List<MapDefinition> GetMapDefinitions() { return new List<MapDefinition>(_mapDefinitions); }
        internal static int NumberOfLoadedMods { get; private set; }
        internal static void Reload()
        {
            NumberOfLoadedMods = 0;
            Console.WriteLine("Loading mods");
            // prepare lists for the different kinds of object we'll encounter in the folder
            List<MapDefinition> mapDefinitions = new List<MapDefinition>();
            // get the path for the mods directory
            string modsDirectoryPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Mods");
            // get the paths of the potential mods
            string[] files = Directory.EnumerateFiles(modsDirectoryPath, "*.dll", SearchOption.AllDirectories).ToArray();
            // check each potential mod
            foreach (string file in files)
            {
                Console.WriteLine("Trying to load mods from {0}", file);

                // load the assembly
                Assembly DLL = Assembly.LoadFile(file);
                int containedMods=0;
                // go through the public types in the assembly
                foreach (Type type in DLL.GetExportedTypes())
                {
                    if (!typeof(IModBase).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    containedMods++;

                    try
                    {
                        // if the type can be cast to a map, create a new map instance based on that type
                        if (typeof(MapDefinition).IsAssignableFrom(type))
                        {
                            MapDefinition mapDefinition = Activator.CreateInstance(type) as MapDefinition;
                            if (mapDefinition != null)
                            {
                                // each map definition counts as a mod
                                loadMod(mapDefinition, file);
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

                Console.WriteLine("The file {0} contained {1} mods", Path.GetFileName(file), containedMods);

            }

            // save the acquired maps in the local array
            _mapDefinitions = mapDefinitions.ToArray();
            // report the number of loaded mods
            Console.WriteLine("Total mods loaded: {0}", NumberOfLoadedMods);
        }

        private static void loadMod(IModBase mod, string filePath)
        {
            if (mod == null) { return; }

            NumberOfLoadedMods++;

            if (!_loadedMods.ContainsKey(mod.Author)) { _loadedMods[mod.Author] = new List<string>(); }
            _loadedMods[mod.Author].Add(mod.ModName);
            _loadedModInstances[getModKey(mod)] = mod;
            _loadedModFolderPaths[getModKey(mod)] = Path.GetDirectoryName(filePath);

            Console.WriteLine("Loaded mod: {0}",mod.ModName);
            Console.WriteLine("Author: {0}",mod.Author);
            Console.WriteLine("Version: {0}",mod.Version);

        }

        private static string getModKey(IModBase mod) { return mod == null ? null : String.Format("{0}.{1}", mod.Author, mod.ModName); }
        private static string getModKey(string author, string modName) { return String.IsNullOrWhiteSpace(author) || String.IsNullOrWhiteSpace(modName) ? null : String.Format("{0}.{1}", author, modName); }
    }
}

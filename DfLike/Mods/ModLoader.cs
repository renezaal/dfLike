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
        // the key in the outermost dictionary is the namespace
        // the list under each namespace is the class name
        private static Dictionary<string, List<string>> _loadedMods = new Dictionary<string, List<string>>();
        // list of instances of the loaded mods
        // the keys are <namespace>.<class name>
        private static Dictionary<string, IModBase> _loadedModInstances = new Dictionary<string, IModBase>();
        // all map instances loaded from the mods folder
        // map instances are not actual maps but definitions of maps, each instance can contain a map
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
            string[] files = Directory.GetFiles(modsDirectoryPath, "*.dll");

            // check each potential mod
            foreach (string file in files)
            {
                // load the assembly
                Assembly DLL = Assembly.LoadFile(file);

                // go through the public types in the assembly
                foreach (Type type in DLL.GetExportedTypes())
                {
                    //Console.WriteLine("AssemblyQualifiedName: {0}", type.AssemblyQualifiedName);
                    //Console.WriteLine("DeclaringMethod.Name: {0}",type.IsGenericParameter? type.DeclaringMethod.Name:"Type is not generic parameter");
                    //Console.WriteLine("FullName: {0}", type.FullName);
                    //Console.WriteLine("GUID: {0}", type.GUID);
                    //Console.WriteLine("Module.FullyQualifiedName: {0}", type.Module.FullyQualifiedName);
                    //Console.WriteLine("Name: {0}", type.Name);
                    //Console.WriteLine("Namespace: {0}", type.Namespace);
                    try
                    {
                        // if the type can be cast to a map, create a new map instance based on that type
                        if (typeof(MapDefinition).IsAssignableFrom(type))
                        {
                            MapDefinition mapDefinition = Activator.CreateInstance(type) as MapDefinition;
                            if (mapDefinition != null)
                            {
                                // each map definition counts as a mod
                                loadMod(mapDefinition);
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

        private static void loadMod(IModBase mod)
        {
            if (mod == null) { return; }

            NumberOfLoadedMods++;

            if (!_loadedMods.ContainsKey(mod.Author)) { _loadedMods[mod.Author] = new List<string>(); }
            _loadedMods[mod.Author].Add(mod.ModName);
            _loadedModInstances[String.Format("{0}.{1}", mod.Author, mod.ModName)] = mod;
            Console.WriteLine("Loaded: {0}\nAuthor: {1}\nVersion: {2}", mod.ModName, mod.Author, mod.Version);

        }
    }
}

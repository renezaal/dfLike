using DfLike.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.Map
{
    public abstract class MapDefinition : IModBase
    {
        public abstract string GetMapDefinitionName();

        public abstract string Author { get; }
        public abstract string ModName { get; }
        public abstract uint VersionNumber { get; }
        public abstract string Version { get; }

        private Block[, ,] _map;
        public void Initialize(int x, int y, int z)
        {
            _map = new Block[x, y, z];
        }
        public Block GetBlock(int x, int y, int z)
        {
            Block block = _map[x, y, z];
            if (block==null)
            {
                block = GenerateBlock(x, y, z);
                _map[x, y, z] = block;
            }
            return block;
        }
        public int MapSeed { get; protected internal set; }
        protected abstract Block GenerateBlock(int x, int y, int z);
    }
}

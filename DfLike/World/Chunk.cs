using DfLike.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.World
{
    public class Chunk
    {
        private const short CHUNKSIZE = 16 * 16 * 16;

        private void bla()
        {
            short foo;
            foo = 0x0FFF;
            byte x = (byte)((foo >> 8) & 0xF);
            byte y = (byte)((foo >> 4) & 0xF);
            byte z = (byte)(foo & 0xF);
        }
        private ulong _xBase, _yBase, _zBase;
        private Block[] _blocks = new Block[CHUNKSIZE];
        public Block GetBlock(ulong x, ulong y, ulong z)
        {
            int index = GlobalCoordinatesToBlockIndex(x, y, z, _xBase, _yBase, _zBase);
            Block block = _blocks[index];
            if (block != null) { return block; }

            // handle block generation
            return _blocks[index];
        }

        private static int GlobalCoordinatesToBlockIndex(ulong globalX, ulong globalY, ulong globalZ, ulong zeroX, ulong zeroY, ulong zeroZ)
        {
            return (int)((((globalX - zeroX) & 0x0F) << 8) | (((globalY - zeroY) & 0x0F) << 4) | ((globalZ - zeroZ) & 0x0F));
        }
        private static int GlobalCoordinatesToBlockIndex(Coordinates global, Coordinates chunkZero)
        {
            return GlobalCoordinatesToBlockIndex(global.X, global.Y, global.Z, chunkZero.X, chunkZero.Y, chunkZero.Z);
        }
        private static Coordinates BlockIndexToGlobalCoordinates(int index, Coordinates chunkZero)
        {
            Coordinates global = new Coordinates();
            global.X = chunkZero.X + (ulong)(index & 0x0F00);
            global.Y = chunkZero.Y + (ulong)(index & 0x00F0);
            global.Z = chunkZero.Z + (ulong)(index & 0x000F);
            return global;
        }

        struct Coordinates
        {
            public ulong X;
            public ulong Y;
            public ulong Z;
        }

    }

}

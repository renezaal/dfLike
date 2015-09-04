using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.World
{
    [Serializable]
    public abstract class Block : ISerializable
    {


        private uint _bits;

        public uint Transparency { get { return GetInteger(_bits, 0, 7); } set { SetInteger(_bits, 0, value, 7); } }
        public bool IsLiquid { get { return GetBoolean(_bits, 3); } set { SetBoolean(_bits, 3, value); } }
        public bool IsSuffocating { get { return GetBoolean(_bits, 4); } set { SetBoolean(_bits, 4, value); } }
        public bool BlocksLiquid { get { return GetBoolean(_bits, 5); } set { SetBoolean(_bits, 5, value); } }

        private static uint GetInteger(uint bits, int position, uint valueMask)
        {
            return (bits >> position) & valueMask;
        }
        private static void SetInteger(uint bits, int position, uint value, uint valueMask)
        {
            if (value > valueMask) { value = valueMask; }
            bits &= ~(valueMask << position);
            bits |= (value << position);
        }
        private static bool GetBoolean(uint bits, int position)
        {
            return GetInteger(bits, position, 1) == 1;
        }
        private static void SetBoolean(uint bits, int position, bool value)
        {
            SetInteger(bits, position, (uint)(value ? 1 : 0), 1);
        }

        public Block(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}

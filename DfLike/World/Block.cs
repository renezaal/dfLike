using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.World
{
    [Serializable]
    public sealed class Block : ISerializable
    {
        #region custom values
        // Contains all integers and smaller values set by mods
        private uint[] _customBits;

        // contain larger values that are set by mods
        private ConcurrentDictionary<int, float> _customFloats;
        private ConcurrentDictionary<int, string> _customStrings;

        #region custom value constructs
        public struct ReservedSpacePointer
        {
            internal ReservedSpacePointer(int index, int position, int bits)
            { Index = index; Position = position; Bits = bits; Mask = CreateFullMask((uint)bits); }
            internal readonly int Index;
            internal readonly int Position;
            internal readonly int Bits;
            internal readonly uint Mask;
        }
        public struct BoolFieldKey
        {
            public BoolFieldKey(ReservedSpacePointer space)
            { Index = space.Index; Position = space.Position; }
            public readonly int Index;
            public readonly int Position;
        }
        public struct UIntFieldKey
        {
            public UIntFieldKey(ReservedSpacePointer space)
            { Index = space.Index; Position = space.Position; Bits = space.Bits; Mask = space.Mask; }
            public readonly int Index;
            public readonly int Position;
            public readonly int Bits;
            public readonly uint Mask;
        }
        public struct UIntMinimumFieldKey
        {
            public UIntMinimumFieldKey(ReservedSpacePointer space, uint offset)
            { Index = space.Index; Position = space.Position; Bits = space.Bits; Mask = space.Mask; Offset = offset; }
            public readonly int Index;
            public readonly int Position;
            public readonly int Bits;
            public readonly uint Mask;
            public readonly uint Offset;
        }
        public struct IntFieldKey
        {
            public IntFieldKey(ReservedSpacePointer space)
            { Index = space.Index; Position = space.Index; Bits = space.Bits; Mask = space.Mask; }
            public readonly int Index;
            public readonly int Position;
            public readonly int Bits;
            public readonly uint Mask;
        }
        public struct IntMinimumFieldKey
        {
            public IntMinimumFieldKey(ReservedSpacePointer space, int offset)
            { Index = space.Index; Position = space.Position; Bits = space.Bits; Mask = space.Mask; Offset = offset; }
            public readonly int Index;
            public readonly int Position;
            public readonly int Bits;
            public readonly uint Mask;
            public readonly int Offset;
        }
        public struct FloatFieldKey
        {
            public FloatFieldKey(int index) { Index = index; }
            public readonly int Index;
        }
        public struct StringFieldKey
        {
            public StringFieldKey(int index) { Index = index; }
            public readonly int Index;
        }
        #endregion
        #region field creators
        private static readonly List<int> _positionPerIndex = new List<int>();
        private static volatile int _floatFields = 0;
        private static volatile int _stringFields = 0;

        private static readonly object _reserveSpaceLock = new object();
        private static ReservedSpacePointer reserveSpace(uint maxValue)
        {
            int bits = GetNumberOfBitsRequiredForIntegerValue(maxValue);
            lock (_reserveSpaceLock)
            {
                int index = 0;
                int position = 32;
                int length = _positionPerIndex.Count;
                while (index < length && position + bits > 31)
                {
                    position = _positionPerIndex[index];
                    index++;
                }
                if (position + bits > 31)
                {
                    _positionPerIndex.Add(0);
                }
                return new ReservedSpacePointer(index, position, bits);
            }
        }

        public static BoolFieldKey CreateCustomBoolField()
        {
            ReservedSpacePointer space = reserveSpace(1);
            return new BoolFieldKey(space);
        }

        public static UIntFieldKey CreateCustomUIntField(uint maxValue)
        {
            if (maxValue == 0)
            {
                return new UIntFieldKey(new ReservedSpacePointer(-1, 0, 0));
            }
            ReservedSpacePointer space = reserveSpace(maxValue);
            return new UIntFieldKey(space);
        }

        public static UIntMinimumFieldKey CreateCustomUIntField(uint minValue, uint maxValue)
        {
            CheckMinMax(minValue, maxValue);
            if (minValue == maxValue)
            {
                return new UIntMinimumFieldKey(new ReservedSpacePointer(-1, 0, 0), minValue);
            }
            uint range = maxValue - minValue;
            ReservedSpacePointer space = reserveSpace(range);
            return new UIntMinimumFieldKey(space, minValue);
        }

        public static IntFieldKey CreateCustomIntField(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new Exception("The maximum value can not be lower than 0 if no minimum is specified. ");
            }
            if (maxValue == 0)
            {
                return new IntFieldKey(new ReservedSpacePointer(-1, 0, 0));
            }
            ReservedSpacePointer space = reserveSpace((uint)maxValue);
            return new IntFieldKey(space);
        }

        public static IntMinimumFieldKey CreateCustomIntField(int minValue, int maxValue)
        {
            CheckMinMax(minValue, maxValue);
            if (minValue == maxValue)
            {
                return new IntMinimumFieldKey(new ReservedSpacePointer(-1, 0, 0), minValue);
            }
            long min = minValue;
            long max = maxValue;
            uint range = (uint)(max - min);
            ReservedSpacePointer space = reserveSpace(range);
            return new IntMinimumFieldKey(space, minValue);
        }

        private static readonly object _createCustomFloatFieldLock = new object();
        public static FloatFieldKey CreateCustomFloatField()
        {
            lock (_createCustomFloatFieldLock)
            {
                int key = _floatFields;
                _floatFields++;
                return new FloatFieldKey(key);
            }
        }

        private static readonly object _createCustomStringFieldLock = new object();
        public static StringFieldKey CreateCustomStringField()
        {
            lock (_createCustomStringFieldLock)
            {
                int key = _stringFields;
                _stringFields++;
                return new StringFieldKey(key);
            }
        }
        #endregion
        #region custom value getters
        public bool GetCustomValue(BoolFieldKey key)
        {
            ArrayChecker(ref _customBits, key.Index);
            return GetBoolean(_customBits[key.Index], key.Position);
        }
        public uint GetCustomValue(UIntFieldKey key)
        {
            if (key.Index < 0)
            {
                return 0;
            }
            ArrayChecker(ref _customBits, key.Index);
            return GetInteger(_customBits[key.Index], key.Position, key.Mask);
        }
        public uint GetCustomValue(UIntMinimumFieldKey key)
        {
            if (key.Index < 0)
            {
                return key.Offset;
            }
            ArrayChecker(ref _customBits, key.Index);
            return GetInteger(_customBits[key.Index], key.Position, key.Mask) + key.Offset;
        }
        public int GetCustomValue(IntFieldKey key)
        {
            if (key.Index < 0)
            {
                return 0;
            }
            ArrayChecker(ref _customBits, key.Index);
            return (int)GetInteger(_customBits[key.Index], key.Position, key.Mask);
        }
        public int GetCustomValue(IntMinimumFieldKey key)
        {
            if (key.Index < 0)
            {
                return key.Offset;
            }
            ArrayChecker(ref _customBits, key.Index);
            long storedValue = GetInteger(_customBits[key.Index], key.Position, key.Mask);
            long correctedValue = storedValue + key.Offset;
            return (int)correctedValue;
        }
        public float GetCustomValue(FloatFieldKey key)
        {
            float retVal;
            _customFloats.TryGetValue(key.Index, out retVal);
            return retVal;
        }
        public string GetCustomValue(StringFieldKey key)
        {
            string retVal;
            _customStrings.TryGetValue(key.Index, out retVal);
            return retVal;
        }
        #endregion
        #region custom value setters
        public void SetCustomValue(BoolFieldKey key, bool value)
        {
            if (key.Index < 0) { return; }
            ArrayChecker(ref _customBits, key.Index);
            SetBoolean(ref _customBits[key.Index], key.Position, value);
        }

        public void SetCustomValue(UIntFieldKey key, uint value)
        {
            if (key.Index < 0) { return; }
            ArrayChecker(ref _customBits, key.Index);
            SetInteger(ref _customBits[key.Index], key.Position, key.Mask, value);
        }

        public void SetCustomValue(UIntMinimumFieldKey key, uint value)
        {
            if (key.Index < 0) { return; }
            if (key.Offset > value) { throw new Exception("The value can not be lower than the specified minimum value. "); }
            ArrayChecker(ref _customBits, key.Index);
            SetInteger(ref _customBits[key.Index], key.Position, key.Mask, value - key.Offset);
        }

        public void SetCustomValue(IntFieldKey key, int value)
        {
            if (key.Index < 0) { return; }
            if (value < 0) { throw new Exception("The value can not be lower than 0. "); }
            ArrayChecker(ref _customBits, key.Index);
            SetInteger(ref _customBits[key.Index], key.Position, key.Mask, (uint)value);
        }

        public void SetCustomValue(IntMinimumFieldKey key, int value)
        {
            if (key.Index < 0) { return; }
            if (key.Offset > value) { throw new Exception("The value can not be lower than the specified minimum value. "); }
            ArrayChecker(ref _customBits, key.Index);
            long givenValue = value;
            long correctedValue = givenValue - key.Offset;
            SetInteger(ref _customBits[key.Index], key.Position, key.Mask, (uint)correctedValue);
        }

        public void SetCustomValue(FloatFieldKey key, float value)
        {
            _customFloats[key.Index] = value;
        }

        public void SetCustomValue(StringFieldKey key, string value)
        {
            _customStrings[key.Index] = value;
        }
        #endregion
        #endregion
        #region built-in values
        // Contains the small integer values of this block as well as boolean values. Has 32 bits (0..31)
        private uint _bits;
        public uint Transparency { get { return GetInteger(_bits, 0, 7); } set { SetInteger(ref _bits, 0, 7, value); } }    //00,01,02
        public bool IsLiquid { get { return GetBoolean(_bits, 3); } set { SetBoolean(ref _bits, 3, value); } }              //03
        public bool IsSuffocating { get { return GetBoolean(_bits, 4); } set { SetBoolean(ref _bits, 4, value); } }         //04
        public uint LiquidLevel { get { return GetInteger(_bits, 5, 7); } set { SetInteger(ref _bits, 5, 7, value); } }     //05,06,07
        public bool IsBlockingToLiquid { get { return GetBoolean(_bits, 8); } set { SetBoolean(ref _bits, 8, value); } }    //08
        public bool IsBlockingToGas { get { return GetBoolean(_bits, 9); } set { SetBoolean(ref _bits, 9, value); } }       //09
        public uint LightLevel { get { return GetInteger(_bits, 10, 63); } set { SetInteger(ref _bits, 10, 63, value); } }  //10,11,12,13,14,15
        public uint BlockId { get { return GetInteger(_bits, 16, 4095); } set { SetInteger(ref _bits, 16, 4095, value); } } //16,17,18,19,20,21,22,23,24,25,26,27
        public uint MetaData { get { return GetInteger(_bits, 28, 15); } set { SetInteger(ref _bits, 28, 15, value); } }    //28,29,30,31
        #endregion
        #region value accessors
        private uint GetInteger(uint bits, int position, uint valueMask)
        {
            return (bits >> position) & valueMask;
        }

        private readonly object _setIntegerLock = new object();
        private void SetInteger(ref uint bits, int position, uint valueMask, uint value)
        {
            if (value > valueMask) { value = valueMask; }
            lock (_setIntegerLock)
            {
                bits &= ~(valueMask << position);
                bits |= (value << position);
            }
        }
        private bool GetBoolean(uint bits, int position)
        {
            return GetInteger(bits, position, 1) == 1;
        }
        private void SetBoolean(ref uint bits, int position, bool value)
        {
            SetInteger(ref bits, position, 1, (uint)(value ? 1 : 0));
        }
        #endregion
        #region supporting methods and arithmetic
        public static int GetNumberOfBitsRequiredForIntegerValue(uint value)
        {
            return (int)Math.Ceiling(Math.Log(value, 2));
        }

        private static readonly object _arrayCheckerLock = new object();
        private void ArrayChecker(ref uint[] array, int requiredIndex)
        {
            int length = array.Length;
            if (length > requiredIndex) { return; }
            lock (_arrayCheckerLock)
            {
                if (length > requiredIndex) { return; }
                uint[] oldArray = array;
                uint[] newArray = new uint[requiredIndex + 1];
                for (int i = 0; i < length; i++)
                {
                    newArray[i] = oldArray[i];
                }
                array = newArray;
            }
        }
        private static uint CreateFullMask(uint bitCount)
        {
            return (uint)(Math.Pow(2, bitCount) - 1);
        }
        private static void CheckMinMax(long min, long max)
        {
            if (min > max)
            {
                throw new Exception("The minimum value cannot be larger than the maximum. ");
            }
        }
        #endregion
        #region (de)serialization
        public Block(SerializationInfo info, StreamingContext context)
        {
            // built-in value
            _bits = info.GetUInt32("built-int");
            // cutom bits
            _customBits = new uint[info.GetUInt16("cBitCount")];
            int length = _customBits.Length;
            for (int i = 0; i < length; i++)
            {
                _customBits[i] = info.GetUInt32("cBit" + i);
            }

            // custom floats
            length = info.GetUInt16("cFloatCount");
            _customFloats = new ConcurrentDictionary<int, float>(3, length * 2);
            for (int i = 0; i < length; i++)
            {
                _customFloats[info.GetInt32("cFloatKey" + i)] = info.GetSingle("cFloatValue" + i);
            }

            // custom strings
            length = info.GetUInt16("cStringCount");
            _customStrings = new ConcurrentDictionary<int, string>(3, length * 2);
            for (int i = 0; i < length; i++)
            {
                _customStrings[info.GetInt32("cStringKey" + i)] = info.GetString("cStringValue" + i);
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // built-in value
            info.AddValue("built-int", _bits);

            // cutom bits
            ushort length = (ushort)_customBits.Length;
            info.AddValue("cBitCount", length);
            for (int i = 0; i < length; i++)
            {
                info.AddValue("cBit" + i, _customBits[i]);
            }

            // custom floats
            int[] keys = _customFloats.Keys.ToArray();
            length = (ushort)keys.Length;
            info.AddValue("cFloatCount", length);
            for (int i = 0; i < length; i++)
            {
                int key = keys[i];
                info.AddValue("cFloatKey" + i, key);
                info.AddValue("cFloatValue" + i, _customFloats[key]);
            }

            // custom strings
            keys = _customStrings.Keys.ToArray();
            length = (ushort)keys.Length;
            info.AddValue("cStringCount", length);
            for (int i = 0; i < length; i++)
            {
                int key = keys[i];
                info.AddValue("cStringKey" + i, key);
                info.AddValue("cStringValue" + i, _customStrings[key]);
            }
        }
        #endregion
    }
}

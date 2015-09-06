using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.World
{
    [Serializable]
    public class Block : ISerializable
    {
        #region custom values
        // Contains all integers and smaller values set by mods
        private uint[] _customBits;

        // contain larger values that are set by mods
        private Dictionary<int, float> _customFloats;
        private Dictionary<int, string> _customStrings;

        #region custom value constructs
        public delegate bool BoolGetterDelegate(Block block);
        public delegate uint UIntGetterDelegate(Block block);
        public delegate int IntGetterDelegate(Block block);
        public delegate float FloatGetterDelegate(Block block);
        public delegate string StringGetterDelegate(Block block);

        public delegate void BoolSetterDelegate(Block block, bool value);
        public delegate void UIntSetterDelegate(Block block, uint value);
        public delegate void IntSetterDelegate(Block block, int value);
        public delegate void FloatSetterDelegate(Block block, float value);
        public delegate void StringSetterDelegate(Block block, string value);

        public struct BoolFieldAccessor
        {
            public BoolFieldAccessor(BoolGetterDelegate get, BoolSetterDelegate set) { Get = get; Set = set; }
            public readonly BoolGetterDelegate Get;
            public readonly BoolSetterDelegate Set;
        }
        public struct UIntFieldAccessor
        {
            public UIntFieldAccessor(UIntGetterDelegate get, UIntSetterDelegate set) { Get = get; Set = set; }
            public readonly UIntGetterDelegate Get;
            public readonly UIntSetterDelegate Set;
        }
        public struct IntFieldAccessor
        {
            public IntFieldAccessor(IntGetterDelegate get, IntSetterDelegate set) { Get = get; Set = set; }
            public readonly IntGetterDelegate Get;
            public readonly IntSetterDelegate Set;
        }
        public struct FloatFieldAccessor
        {
            public FloatFieldAccessor(FloatGetterDelegate get, FloatSetterDelegate set) { Get = get; Set = set; }
            public readonly FloatGetterDelegate Get;
            public readonly FloatSetterDelegate Set;
        }
        public struct StringFieldAccessor
        {
            public StringFieldAccessor(StringGetterDelegate get, StringSetterDelegate set) { Get = get; Set = set; }
            public readonly StringGetterDelegate Get;
            public readonly StringSetterDelegate Set;
        }
        #endregion

        private static int _currentCustomBitsIndex = 0;
        private static int _currentCustomBitsPosition = 0;
        private static int _floatFields = 0;
        private static int _stringFields = 0;

        public BoolFieldAccessor CreateCustomBoolField()
        {
            UIntFieldAccessor uintAccessor = CreateCustomUIntFieldInternal(this, 0, 1);

            UIntGetterDelegate uintGetter = uintAccessor.Get;
            BoolGetterDelegate getter = new BoolGetterDelegate((block) => { return uintGetter(block) == 1; });

            UIntSetterDelegate uintSetter = uintAccessor.Set;
            BoolSetterDelegate setter = new BoolSetterDelegate((block, value) => { uintSetter(block, value ? 1u : 0u); });

            return new BoolFieldAccessor(getter, setter);
        }

        public UIntFieldAccessor CreateCustomUIntField(uint maxValue)
        {
            return CreateCustomUIntField(0, maxValue);
        }
        public UIntFieldAccessor CreateCustomUIntField(uint minValue, uint maxValue)
        {
            return CreateCustomUIntFieldInternal(this, minValue, maxValue);
        }
        private static readonly object _createCustomUIntFieldInternalLock = new object();
        private static UIntFieldAccessor CreateCustomUIntFieldInternal(Block instance, uint minValue, uint maxValue)
        {
            if (minValue > maxValue)
            {
                throw new Exception("Minimum value cannot be larger than the maximum value. ");
            }
            lock (_createCustomUIntFieldInternalLock)
            {
                UIntGetterDelegate getter;
                UIntSetterDelegate setter;

                if (maxValue == 0 || minValue == maxValue)
                {
                    getter = new UIntGetterDelegate((block) => { return maxValue; });
                    setter = new UIntSetterDelegate((block, value) => { });
                    return new UIntFieldAccessor(getter, setter);
                }

                uint range = maxValue - minValue;
                uint bitsNeeded = GetNumberOfBitsRequiredForIntegerValue(range);

                if (_currentCustomBitsPosition + bitsNeeded > 31)
                {
                    _currentCustomBitsIndex++;
                    _currentCustomBitsPosition = 0;
                }

                int index = _currentCustomBitsIndex;
                int position = _currentCustomBitsPosition;
                uint bitmask = CreateFullMask(bitsNeeded);

                if (minValue == 0)
                {
                    getter = new UIntGetterDelegate((block) =>
                    {
                        ArrayChecker(ref block._customBits, index);
                        return GetInteger(block._customBits[index], position, bitmask);
                    });
                    setter = new UIntSetterDelegate((block, value) =>
                    {
                        ArrayChecker(ref block._customBits, index);
                        SetInteger(ref block._customBits[index], position, bitmask, value);
                    });
                    return new UIntFieldAccessor(getter, setter);
                }

                uint min = minValue;
                uint max = maxValue;

                getter = new UIntGetterDelegate((block) =>
                {
                    ArrayChecker(ref block._customBits, index);
                    return GetInteger(block._customBits[index], position, bitmask) + minValue;
                });
                setter = new UIntSetterDelegate((block, value) =>
                {
                    ArrayChecker(ref block._customBits, index);
                    SetInteger(ref block._customBits[index], position, bitmask, value - minValue);
                });
                return new UIntFieldAccessor(getter, setter);
            }
        }

        public IntFieldAccessor CreateCustomIntField(int minValue, int maxValue)
        {
            return CreateCustomIntFieldInternal(this, minValue, maxValue);
        }
        private static IntFieldAccessor CreateCustomIntFieldInternal(Block instance, int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new Exception("Minimum value cannot be larger than the maximum value. ");
            }

            long min = minValue;
            long max = maxValue;
            uint range = (uint)(maxValue - minValue);
            int minCopy = minValue;

            UIntFieldAccessor uintAccessor = CreateCustomUIntFieldInternal(instance, 0, range);
            UIntGetterDelegate uintGetter = uintAccessor.Get;
            UIntSetterDelegate uintSetter = uintAccessor.Set;
            IntGetterDelegate getter;
            IntSetterDelegate setter;
            if (minValue == 0)
            {
                getter = new IntGetterDelegate((block) => { return (int)uintGetter(block); });
                setter = new IntSetterDelegate((block, value) => { uintSetter(block, (uint)value); });
            }
            else
            {
                getter = new IntGetterDelegate((block) => { return (int)(uintGetter(block) + minCopy); });
                setter = new IntSetterDelegate((block, value) =>
            {
                long valueCopy = value;
                long correctedValue = value - min;
                uintSetter(block, (uint)correctedValue);
            });
            }

            return new IntFieldAccessor(getter, setter);
        }

        public FloatFieldAccessor CreateCustomFloatField()
        {
            return CreateCustomFloatFieldInternal();
        }
        private static readonly object _createCustomFloatFieldInternalLock = new object();
        private static FloatFieldAccessor CreateCustomFloatFieldInternal()
        {
            lock (_createCustomFloatFieldInternalLock)
            {
                int numberOfFields = _floatFields;
                _floatFields++;
                FloatGetterDelegate getter = new FloatGetterDelegate((block) =>
                {
                    float retVal = 0;
                    return block._customFloats.TryGetValue(numberOfFields, out retVal) ? retVal : 0;
                });
                FloatSetterDelegate setter = new FloatSetterDelegate((block, value) => { block._customFloats[numberOfFields] = value; });
                return new FloatFieldAccessor(getter, setter);
            }
        }

        public StringFieldAccessor CreateCustomStringField()
        {
            return CreateCustomStringFieldInternal();
        }
        private static readonly object _createCustomStringFieldInternalLock = new object();
        private static StringFieldAccessor CreateCustomStringFieldInternal()
        {
            lock (_createCustomStringFieldInternalLock)
            {
                int numberOfFields = _stringFields;
                _stringFields++;
                StringGetterDelegate getter = new StringGetterDelegate((block) =>
                {
                    string retVal = String.Empty;
                    return block._customStrings.TryGetValue(numberOfFields, out retVal) ? retVal : String.Empty;
                });
                StringSetterDelegate setter = new StringSetterDelegate((block, value) => { block._customStrings[numberOfFields] = value; });
                return new StringFieldAccessor(getter, setter);
            }
        }
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
        private static uint GetInteger(uint bits, int position, uint valueMask)
        {
            return (bits >> position) & valueMask;
        }
        private static void SetInteger(ref uint bits, int position, uint valueMask, uint value)
        {
            if (value > valueMask) { value = valueMask; }
            bits &= ~(valueMask << position);
            bits |= (value << position);
        }
        private static bool GetBoolean(uint bits, int position)
        {
            return GetInteger(bits, position, 1) == 1;
        }
        private static void SetBoolean(ref uint bits, int position, bool value)
        {
            SetInteger(ref bits, position, 1, (uint)(value ? 1 : 0));
        }
        #endregion
        #region supporting methods and arithmetic
        public static uint GetNumberOfBitsRequiredForIntegerValue(uint value)
        {
            return (uint)Math.Ceiling(Math.Log(value, 2));
        }

        private static readonly object _arrayCheckerLock = new object();
        private static void ArrayChecker(ref uint[] array, int requiredIndex)
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
            _customFloats = new Dictionary<int, float>(length * 2);
            for (int i = 0; i < length; i++)
            {
                _customFloats[info.GetInt32("cFloatKey" + i)] = info.GetSingle("cFloatValue" + i);
            }

            // custom strings
            length = info.GetUInt16("cStringCount");
            _customStrings = new Dictionary<int, string>(length * 2);
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

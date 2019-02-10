using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using AltV.Net.Elements.Entities;

namespace AltV.Net.Native
{
    //TODO: check the types in getter methods
    [StructLayout(LayoutKind.Sequential)]
    public struct MValue
    {
        // The MValue param needs to be an List type
        public delegate void Function(ref MValue args, ref MValue result);

        public enum Type : byte
        {
            NIL = 0,
            BOOL = 1,
            INT = 2,
            UINT = 3,
            DOUBLE = 4,
            STRING = 5,
            LIST = 6,
            DICT = 7,
            ENTITY = 8,
            FUNCTION = 9
        }

        internal static readonly int Size = Marshal.SizeOf<MValue>();

        public static MValue Nil = new MValue(0, IntPtr.Zero);

        //TODO: create a map that holds function pointers for each object type, its probably faster then this switch now
        public static MValue? CreateFromObject(object obj)
        {
            if (obj == null)
            {
                return Create();
            }

            switch (obj)
            {
                case IEntity entity:
                    return Create(entity);
                case bool value:
                    return Create(value);
                case int value:
                    return Create(value);
                case uint value:
                    return Create(value);
                case long value:
                    return Create(value);
                case ulong value:
                    return Create(value);
                case double value:
                    return Create(value);
                case string value:
                    return Create(value);
                case MValue value:
                    return value;
                case MValue[] value:
                    return Create(value);
                /*case Dictionary<string, object> value:
                    dictMValues = new List<MValue>();
                    foreach (var dictValue in value.Values)
                    {
                        var dictMValue = CreateFromObject(dictValue);
                        dictMValues.Add(dictMValue ?? Create());
                    }

                    return Create(dictMValues.ToArray(), value.Keys.ToArray());
                case Dictionary<string, bool> value:
                    dictMValues = new List<MValue>();
                    foreach (var dictValue in value.Values)
                    {
                        dictMValues.Add(Create(dictValue));
                    }

                    return Create(dictMValues.ToArray(), value.Keys.ToArray());
                case Dictionary<string, int> value:
                    dictMValues = new List<MValue>();
                    foreach (var dictValue in value.Values)
                    {
                        dictMValues.Add(Create(dictValue));
                    }

                    return Create(dictMValues.ToArray(), value.Keys.ToArray());
                case Dictionary<string, long> value:
                    dictMValues = new List<MValue>();
                    foreach (var dictValue in value.Values)
                    {
                        dictMValues.Add(Create(dictValue));
                    }

                    return Create(dictMValues.ToArray(), value.Keys.ToArray());
                case Dictionary<string, uint> value:
                    dictMValues = new List<MValue>();
                    foreach (var dictValue in value.Values)
                    {
                        dictMValues.Add(Create(dictValue));
                    }

                    return Create(dictMValues.ToArray(), value.Keys.ToArray());
                case Dictionary<string, ulong> value:
                    dictMValues = new List<MValue>();
                    foreach (var dictValue in value.Values)
                    {
                        dictMValues.Add(Create(dictValue));
                    }

                    return Create(dictMValues.ToArray(), value.Keys.ToArray());
                case Dictionary<string, double> value:
                    dictMValues = new List<MValue>();
                    foreach (var dictValue in value.Values)
                    {
                        dictMValues.Add(Create(dictValue));
                    }

                    return Create(dictMValues.ToArray(), value.Keys.ToArray());*/
                case Invoker value:
                    return Create(value);
                case Function value:
                    return Create(value);
                /*case object[] value:
                    return Create(value.Select(objArrayValue => CreateFromObject(objArrayValue) ?? Create()).ToArray());
                case bool[] value:
                    return Create(value.Select(Create).ToArray());
                case int[] value:
                    return Create(value.Select(objArrayValue => Create(objArrayValue)).ToArray());
                case long[] value:
                    return Create(value.Select(Create).ToArray());
                case ulong[] value:
                    return Create(value.Select(Create).ToArray());
                case uint[] value:
                    return Create(value.Select(objArrayValue => Create(objArrayValue)).ToArray());
                case double[] value:
                    return Create(value.Select(Create).ToArray());*/
                case Net.Function function:
                    return Create(function.call);
                case IDictionary dictionary:
                    var dictKeys = new string[dictionary.Count];
                    var dictValues = new MValue[dictionary.Count];
                    var i = 0;
                    foreach (var key in dictionary.Keys)
                    {
                        if (key is string stringKey)
                        {
                            dictKeys[i++] = stringKey;
                        }
                        else
                        {
                            return Create();
                        }
                    }

                    i = 0;
                    foreach (var value in dictionary.Values)
                    {
                        dictValues[i++] = CreateFromObject(value) ?? Create();
                    }

                    return Create(dictValues, dictKeys);
                case ICollection collection:
                    var listValues = new MValue[collection.Count];
                    i = 0;
                    foreach (var value in collection)
                    {
                        listValues[i++] = CreateFromObject(value) ?? Create();
                    }

                    return Create(listValues);
                default:
                    Server.Instance.LogInfo("cant convert type:" + obj.GetType());
                    return Create();
            }
        }

        public static MValue Create()
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateNil(ref mValue);
            return mValue;
        }

        public static MValue Create(bool value)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateBool(value, ref mValue);
            return mValue;
        }

        public static MValue Create(long value)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateInt(value, ref mValue);
            return mValue;
        }

        public static MValue Create(ulong value)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateUInt(value, ref mValue);
            return mValue;
        }

        public static MValue Create(double value)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateDouble(value, ref mValue);
            return mValue;
        }

        public static MValue Create(string value)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateString(value, ref mValue);
            return mValue;
        }

        public static MValue Create(MValue[] values)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateList(values, (ulong) values.Length, ref mValue);
            return mValue;
        }

        public static MValue Create(MValue[] values, string[] keys)
        {
            if (values.Length != keys.Length)
                throw new ArgumentException($"values length: {values.Length} != keys length: {keys.Length}");
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateDict(values, keys, (ulong) values.Length, ref mValue);
            return mValue;
        }

        public static MValue Create(IEntity entity)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateEntity(entity.NativePointer, ref mValue);
            return mValue;
        }

        public static MValue Create(Function function)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateFunction(AltVNative.MValueCreate.Invoker_Create(function), ref mValue);
            return mValue;
        }

        public static MValue Create(Invoker invoker)
        {
            var mValue = Nil;
            AltVNative.MValueCreate.MValue_CreateFunction(invoker.NativePointer, ref mValue);
            return mValue;
        }

        public readonly Type type;
        public readonly IntPtr storagePointer;

        public MValue(Type type, IntPtr storagePointer)
        {
            this.type = type;
            this.storagePointer = storagePointer;
        }

        public bool GetBool()
        {
            return AltVNative.MValueGet.MValue_GetBool(ref this);
        }

        public long GetInt()
        {
            return AltVNative.MValueGet.MValue_GetInt(ref this);
        }

        public ulong GetUint()
        {
            return AltVNative.MValueGet.MValue_GetUInt(ref this);
        }

        public double GetDouble()
        {
            return AltVNative.MValueGet.MValue_GetDouble(ref this);
        }

        public string GetString()
        {
            var value = IntPtr.Zero;
            AltVNative.MValueGet.MValue_GetString(ref this, ref value);
            return Marshal.PtrToStringAnsi(value);
        }

        public void GetEntityPointer(ref IntPtr entityPointer)
        {
            AltVNative.MValueGet.MValue_GetEntity(ref this, ref entityPointer);
        }

        public IntPtr GetEntityPointer()
        {
            var entityPointer = IntPtr.Zero;
            AltVNative.MValueGet.MValue_GetEntity(ref this, ref entityPointer);
            return entityPointer;
        }

        public void GetList(ref MValueArray mValueArray)
        {
            AltVNative.MValueGet.MValue_GetList(ref this, ref mValueArray);
        }

        public MValue[] GetList()
        {
            var mValueArray = MValueArray.Nil;
            AltVNative.MValueGet.MValue_GetList(ref this, ref mValueArray);
            return mValueArray.ToArray();
        }

        public Dictionary<string, MValue> GetDictionary()
        {
            var stringViewArray = StringViewArray.Nil;
            var valueArrayRef = MValueArray.Nil;
            AltVNative.MValueGet.MValue_GetDict(ref this, ref stringViewArray, ref valueArrayRef);
            var valueArrayPtr = valueArrayRef.data;
            var stringViewArrayPtr = stringViewArray.data;
            var size = (int) stringViewArray.size;
            if (valueArrayRef.size != (ulong) size) // Value size != key size should never happen
            {
                return null;
            }

            var dictionary = new Dictionary<string, MValue>();
            for (var i = 0; i < size; i++)
            {
                dictionary[Marshal.PtrToStructure<StringView>(stringViewArrayPtr).Text] =
                    Marshal.PtrToStructure<MValue>(valueArrayPtr);
                valueArrayPtr += Size;
                stringViewArrayPtr += StringView.Size;
            }

            return dictionary;
        }

        public Function GetFunction()
        {
            return AltVNative.MValueGet.MValue_GetFunction(ref this);
        }

        public MValue CallFunction(MValue[] args)
        {
            var result = Nil;
            AltVNative.MValueCall.MValue_CallFunction(ref this, args, (ulong) args.Length, ref result);
            return result;
        }

        public override string ToString()
        {
            switch (type)
            {
                case Type.NIL:
                    return "Nil";
                case Type.BOOL:
                    return GetBool().ToString();
                case Type.INT:
                    return GetInt().ToString();
                case Type.UINT:
                    return GetUint().ToString();
                case Type.DOUBLE:
                    return GetDouble().ToString(CultureInfo.InvariantCulture);
                case Type.STRING:
                    return GetString();
                case Type.LIST:
                    return GetList().Aggregate("List:", (current, value) => current + value.ToString() + ",");
                case Type.DICT:
                    return GetDictionary().Aggregate("Dict:",
                        (current, value) => current + value.Key.ToString() + "=" + value.Value.ToString() + ",");
                case Type.ENTITY:
                    return "MValue<Entity>";
                case Type.FUNCTION:
                    return "MValue<Function>";
            }

            return "MValue<>";
        }

        public object ToObject()
        {
            return ToObject(Alt.Module.BaseEntityPool);
        }

        public object ToObject(IBaseEntityPool baseEntityPool)
        {
            switch (type)
            {
                case Type.NIL:
                    return null;
                case Type.BOOL:
                    return GetBool();
                case Type.INT:
                    return GetInt();
                case Type.UINT:
                    return GetUint();
                case Type.DOUBLE:
                    return GetDouble();
                case Type.STRING:
                    return GetString();
                case Type.LIST:
                    var mValueArray = MValueArray.Nil;
                    AltVNative.MValueGet.MValue_GetList(ref this, ref mValueArray);
                    var arrayValue = mValueArray.data;
                    var arrayValues = new object[mValueArray.size];
                    for (var i = 0; i < arrayValues.Length; i++)
                    {
                        arrayValues[i] = Marshal.PtrToStructure<MValue>(arrayValue).ToObject(baseEntityPool);
                        arrayValue += Size;
                    }

                    return arrayValues;
                case Type.DICT:
                    var stringViewArray = StringViewArray.Nil;
                    var valueArrayRef = MValueArray.Nil;
                    AltVNative.MValueGet.MValue_GetDict(ref this, ref stringViewArray, ref valueArrayRef);
                    var valueArrayPtr = valueArrayRef.data;
                    var stringViewArrayPtr = stringViewArray.data;
                    var size = (int) stringViewArray.size;
                    if (valueArrayRef.size != (ulong) size) // Value size != key size should never happen
                    {
                        return null;
                    }

                    var dictionary = new Dictionary<string, object>();
                    for (var i = 0; i < size; i++)
                    {
                        dictionary[Marshal.PtrToStructure<StringView>(stringViewArrayPtr).Text] =
                            Marshal.PtrToStructure<MValue>(valueArrayPtr).ToObject(baseEntityPool);
                        valueArrayPtr += Size;
                        stringViewArrayPtr += StringView.Size;
                    }

                    return dictionary;
                case Type.ENTITY:
                    var entityPointer = IntPtr.Zero;
                    GetEntityPointer(ref entityPointer);
                    if (entityPointer == IntPtr.Zero) return null;
                    if (baseEntityPool.GetOrCreate(entityPointer, out var entity))
                    {
                        return entity;
                    }

                    return null;
                case Type.FUNCTION:
                    return GetFunction();
                default:
                    return null;
            }
        }
    }
}
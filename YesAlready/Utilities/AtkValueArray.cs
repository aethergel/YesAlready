using System;
using System.Runtime.InteropServices;
using System.Text;

namespace YesAlready.Utils;

/// <summary>
/// A disposable AtkValue* object.
/// </summary>
/// <remarks>
/// https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Utility/Common.cs#L261.
/// </remarks>
internal unsafe class AtkValueArray : IDisposable
{
    public AtkValueArray(params object[] values)
    {
        Length = values.Length;
        Address = Marshal.AllocHGlobal(Length * Marshal.SizeOf<AtkValue>());
        Pointer = (AtkValue*)Address;

        for (var i = 0; i < values.Length; i++)
        {
            EncodeValue(i, values[i]);
        }
    }

    public IntPtr Address { get; private set; }

    public AtkValue* Pointer { get; private set; }

    public int Length { get; private set; }

    public static implicit operator AtkValue*(AtkValueArray arr) => arr.Pointer;

    public void Dispose()
    {
        for (var i = 0; i < Length; i++)
        {
            if (Pointer[i].Type == AtkValueType.String)
                Marshal.FreeHGlobal(new IntPtr(Pointer[i].String));
        }

        Marshal.FreeHGlobal(Address);
    }

    private unsafe void EncodeValue(int index, object value)
    {
        switch (value)
        {
            case uint uintValue:
                Pointer[index].Type = AtkValueType.UInt;
                Pointer[index].UInt = uintValue;
                break;
            case int intValue:
                Pointer[index].Type = AtkValueType.Int;
                Pointer[index].Int = intValue;
                break;
            case float floatValue:
                Pointer[index].Type = AtkValueType.Float;
                Pointer[index].Float = floatValue;
                break;
            case bool boolValue:
                Pointer[index].Type = AtkValueType.Bool;
                Pointer[index].Byte = Convert.ToByte(boolValue);
                break;
            case string stringValue:
                var stringBytes = Encoding.UTF8.GetBytes(stringValue + '\0');
                var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length + 1);

                Pointer[index].Type = AtkValueType.String;
                Pointer[index].String = (byte*)stringAlloc;
                break;
            default:
                throw new ArgumentException($"Unable to convert type {value.GetType()} to AtkValue");
        }
    }
}

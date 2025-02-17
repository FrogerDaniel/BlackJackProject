using System;
using Unity.Netcode;

[Serializable]
[GenerateSerializationForType(typeof(IntArrayWrapper))]
public class IntArrayWrapper : IEquatable<IntArrayWrapper>, INetworkSerializable
{
    public int[] Values;

    public IntArrayWrapper() { }

    public IntArrayWrapper(int[] values)
    {
        Values = values;
    }

    public bool Equals(IntArrayWrapper other)
    {
        if (other == null || Values == null || other.Values == null) return false;
        if (Values.Length != other.Values.Length) return false;

        for (int i = 0; i < Values.Length; i++)
        {
            if (Values[i] != other.Values[i])
            {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as IntArrayWrapper);
    }

    public override int GetHashCode()
    {
        return Values != null ? Values.GetHashCode() : 0;
    }

    public int Length()
    {
        return Values.Length;
    }

    public int this[int index]
    {
        get => Values[index];
        set => Values[index] = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = Values?.Length ?? 0;
        serializer.SerializeValue(ref length);
        if (length > 0)
        {
            if (serializer.IsReader)
            {
                Values = new int[length];
            }
            for (int i = 0; i < length; i++)
            {
                serializer.SerializeValue(ref Values[i]);
            }
        }
    }
}

using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
[GenerateSerializationForType(typeof(SpriteArrayWrapper))]
public class SpriteArrayWrapper : IEquatable<SpriteArrayWrapper>, INetworkSerializable
{
    public string[] SpriteNames;  // Store the names of the sprites
    private const string SpriteFolderPath = "Sprites/Cards Resized/";

    public SpriteArrayWrapper() { }

    public SpriteArrayWrapper(Sprite[] sprites)
    {
        SpriteNames = new string[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            SpriteNames[i] = sprites[i] ? sprites[i].name : string.Empty;
        }
    }

    public bool Equals(SpriteArrayWrapper other)
    {
        if (other == null || SpriteNames == null || other.SpriteNames == null) return false;
        if (SpriteNames.Length != other.SpriteNames.Length) return false;

        for (int i = 0; i < SpriteNames.Length; i++)
        {
            if (!SpriteNames[i].Equals(other.SpriteNames[i]))
            {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as SpriteArrayWrapper);
    }

    public override int GetHashCode()
    {
        return SpriteNames != null ? SpriteNames.GetHashCode() : 0;
    }

    public int Length()
    {
        return SpriteNames.Length;
    }

    public string this[int index]
    {
        get => SpriteNames[index];
        set => SpriteNames[index] = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = SpriteNames?.Length ?? 0;
        serializer.SerializeValue(ref length);
        if (length > 0)
        {
            if (serializer.IsReader)
            {
                SpriteNames = new string[length];
            }
            for (int i = 0; i < length; i++)
            {
                serializer.SerializeValue(ref SpriteNames[i]);
            }
        }
    }

    public Sprite[] GetSprites()
    {
        Sprite[] sprites = new Sprite[SpriteNames.Length];
        for (int i = 0; i < SpriteNames.Length; i++)
        {
            string fullPath = SpriteFolderPath + SpriteNames[i];
            sprites[i] = Resources.Load<Sprite>(fullPath);
            if (sprites[i] == null)
            {
                Debug.LogError("Failed to load sprite: " + SpriteNames[i] + " from path: " + fullPath);
            }
        }
        return sprites;
    }
}
    
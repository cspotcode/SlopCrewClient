using ProtoBuf;
using UnityEngine;

namespace cspotcode.SlopCrewClient;

[ProtoContract]
public class Vector3Surrogate
{
    [ProtoMember(1)]
    public readonly float X;
    [ProtoMember(2)]
    public readonly float Y;
    [ProtoMember(3)]
    public readonly float Z;

    public Vector3Surrogate(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static implicit operator Vector3Surrogate(Vector3 v)
    {
        return new Vector3Surrogate(v.x, v.y, v.z);
    }

    public static implicit operator Vector3(Vector3Surrogate v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }
}
[ProtoContract]
public class Vector2Surrogate
{
    [ProtoMember(1)]
    public readonly float X;
    [ProtoMember(2)]
    public readonly float Y;

    public Vector2Surrogate(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vector2Surrogate(Vector2 v)
    {
        return new Vector2Surrogate(v.x, v.y);
    }

    public static implicit operator Vector2(Vector2Surrogate v)
    {
        return new Vector2(v.X, v.Y);
    }
}

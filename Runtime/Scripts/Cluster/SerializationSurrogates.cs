using System.Runtime.Serialization;
using UnityEngine;

sealed class Vector2SerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {
        Vector2 v = (Vector2)obj;
        info.AddValue("x", v.x);
        info.AddValue("y", v.y);
    }

    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
        StreamingContext context, ISurrogateSelector selector)
    {
        Vector2 v = (Vector2)obj;
        v.x = (float)info.GetValue("x", typeof(float));
        v.y = (float)info.GetValue("y", typeof(float));
        return (System.Object)v;
    }
}

sealed class Vector3SerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3 v = (Vector3)obj;
        info.AddValue("x", v.x);
        info.AddValue("y", v.y);
        info.AddValue("z", v.z);
    }

    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
        StreamingContext context, ISurrogateSelector selector)
    {
        Vector3 v = (Vector3)obj;
        v.x = (float)info.GetValue("x", typeof(float));
        v.y = (float)info.GetValue("y", typeof(float));
        v.z = (float)info.GetValue("z", typeof(float));
        return (System.Object)v;
    }
}

sealed class Vector4SerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {
        Vector4 v = (Vector4)obj;
        info.AddValue("x", v.x);
        info.AddValue("y", v.y);
        info.AddValue("z", v.z);
        info.AddValue("w", v.w);
    }

    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
        StreamingContext context, ISurrogateSelector selector)
    {
        Vector4 v = (Vector4)obj;
        v.x = (float)info.GetValue("x", typeof(float));
        v.y = (float)info.GetValue("y", typeof(float));
        v.z = (float)info.GetValue("z", typeof(float));
        v.w = (float)info.GetValue("w", typeof(float));
        return (System.Object)v;
    }
}

sealed class QuaternionSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {
        Quaternion v = (Quaternion)obj;
        info.AddValue("x", v.x);
        info.AddValue("y", v.y);
        info.AddValue("z", v.z);
        info.AddValue("w", v.w);
    }

    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
        StreamingContext context, ISurrogateSelector selector)
    {
        Quaternion v = (Quaternion)obj;
        v.x = (float)info.GetValue("x", typeof(float));
        v.y = (float)info.GetValue("y", typeof(float));
        v.z = (float)info.GetValue("z", typeof(float));
        v.w = (float)info.GetValue("w", typeof(float));
        return (System.Object)v;
    }
}


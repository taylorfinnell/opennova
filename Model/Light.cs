using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

[Flags]
public enum LightFlags
{
    NO_CORONA = 1 << 0,
    NO_TERRAIN_LIGHT = 1 << 1,
    NO_OBJECT_LIGHT = 1 << 2,
    Flag3 = 1 << 3
}

public class Light
{
    public LightFlags Flags { get; private set; }
    public Matrix4x4 ViewProjMatrix { get; private set; }
    public Vector3 Offset { get; private set; }
    public int SubObjIndex { get; private set; }
    public float AttenStart { get; private set; }
    public float AttenEnd { get; private set; }
    public float Phase { get; private set; }
    public float Rate { get; private set; }
    public Vector4 ColorStart { get; private set; }
    public Vector4 ColorEnd { get; private set; }
    public Quaternion Rotation { get; private set; }
    public int Style { get; private set; }
    public int Unknown1 { get; private set; }
    public int Unknown2 { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        reader.MarkPosition();

        Offset = new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());

        AttenStart = reader.ReadSingle();
        AttenEnd = reader.ReadSingle();

        Style = reader.ReadByte();

        Phase = reader.ReadByte() / 256f;

        Rate = (float)reader.ReadInt16() / 256;

        byte r1 = reader.ReadByte();
        byte g1 = reader.ReadByte();
        byte b1 = reader.ReadByte();
        byte a1 = reader.ReadByte();
        ColorStart = new Vector4(r1, g1, b1, a1) / 255.0f;
        Debug.Assert(a1 == 0); // Alpha should be 0

        byte r2 = reader.ReadByte();
        byte g2 = reader.ReadByte();
        byte b2 = reader.ReadByte();
        byte a2 = reader.ReadByte();
        ColorEnd = new Vector4(r2, g2, b2, a2) / 255.0f;
        Debug.Assert(a2 == 0); // Alpha should be 0

        SubObjIndex = reader.ReadByte();

        reader.AssertBytesRead(33);

        Flags = (LightFlags)reader.ReadByte();

        Unknown1 = reader.ReadByte();
        Unknown2 = reader.ReadByte();

        Debug.Assert(Unknown1 == 0);
        Debug.Assert(Unknown2 == 0);

        Rotation = new Quaternion(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );

        ViewProjMatrix = reader.ReadMatrix4x4();
    }

    public static List<Light> FromNode(ModelNode node)
    {
        if (node.Identifier != "LGHT")
            throw new ArgumentException("Node is not a LGHT node", nameof(node));

        var lights = new List<Light>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        if (recordSize != 116)
            throw new NotImplementedException($"Light record size {recordSize} not supported");

        for (var i = 0; i < recordCount; i++)
        {
            reader.MarkPosition();
            var light = new Light();
            light.Deserialize(reader);
            lights.Add(light);
            reader.AssertBytesRead(recordSize);
        }

        return lights;
    }
}
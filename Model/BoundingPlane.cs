using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public class BoundingPlane
{
    public short Flags { get; private set; }
    public Vector3 Normal { get; private set; }
    public float Radius { get; private set; }

    private static int SignExtend16Bit(int value)
    {
        if ((value & 0x8000) != 0)
        {
            return value - 0x10000;
        }
        return value;
    }

    public void Deserialize(ONBinaryReader reader)
    {
        reader.MarkPosition();
        Flags = reader.ReadInt16();

        Normal = new Vector3(
            SignExtend16Bit(reader.ReadUInt16()) / 16384f,
            SignExtend16Bit(reader.ReadUInt16()) / 16384f,
            SignExtend16Bit(reader.ReadUInt16()) / 16384f);

        Radius = reader.ReadInt32() / 65536f;

        reader.AssertBytesRead(12);
    }

    public static List<BoundingPlane> FromNode(ModelNode node)
    {
        if (node.Identifier != "BPLN")
            throw new ArgumentException("Node is not a BPLN node", nameof(node));

        var boundingPlanes = new List<BoundingPlane>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        for (var i = 0; i < recordCount; i++)
        {
            var plane = new BoundingPlane();
            plane.Deserialize(reader);
            boundingPlanes.Add(plane);
        }

        return boundingPlanes;
    }
}
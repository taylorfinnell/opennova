using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public enum CollisionDominateAxis
{
    X = 1,
    Y = 2,
    Z = 4
}

public class CollisionNormal
{
    public Vector3 Position { get; private set; }
    public CollisionDominateAxis DominateAxis { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        Position = new Vector3(
            reader.ReadInt16() * 256.0f / 65536f,
            reader.ReadInt16() * 256.0f / 65536f,
            reader.ReadInt16() * 256.0f / 65536f);

        DominateAxis = (CollisionDominateAxis)reader.ReadInt16();

        Debug.Assert(DominateAxis == CollisionDominateAxis.X ||
            DominateAxis == CollisionDominateAxis.Y ||
            DominateAxis == CollisionDominateAxis.Z);
    }

    public static List<CollisionNormal> FromNode(ModelNode node)
    {
        if (node.Identifier != "CNRM")
            throw new ArgumentException("Node is not a CNRM node", nameof(node));

        var normals = new List<CollisionNormal>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        reader.MarkPosition();
        for (var i = 0; i < recordCount; i++)
        {
            var normal = new CollisionNormal();
            normal.Deserialize(reader);
            normals.Add(normal);
        }
        reader.AssertBytesRead(recordCount * recordSize);

        return normals;
    }
}
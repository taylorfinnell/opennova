using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public class CollisionVertex
{
    public Vector3 Position { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        float x = reader.ReadInt16() * 256f / 65536f;
        float y = reader.ReadInt16() * 256f / 65536f;
        float z = reader.ReadInt16() * 256f / 65536f;
        float unknown = reader.ReadInt16() * 256f / 65536f;
        Debug.Assert(unknown == 0);
        Position = new Vector3(x, y, z);
    }

    public static List<CollisionVertex> FromNode(ModelNode node)
    {
        if (node.Identifier != "CVRT")
            throw new ArgumentException("Node is not a CVRT node", nameof(node));

        var vertices = new List<CollisionVertex>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        reader.MarkPosition();
        for (var i = 0; i < recordCount; i++)
        {
            var vertex = new CollisionVertex();
            vertex.Deserialize(reader);
            vertices.Add(vertex);
        }
        reader.AssertBytesRead(recordCount * recordSize);

        return vertices;
    }
}

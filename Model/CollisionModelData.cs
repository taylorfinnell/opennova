using OpenNova.Core;
using System;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public class CollisionModelData
{
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    public Vector3 Center { get; private set; }
    public int NumVertices { get; private set; }
    public int NumNormals { get; private set; }
    public int NumFaces { get; private set; }
    public int NumCollisionObjects { get; private set; }
    public int NumberCollisionTransforms { get; private set; }
    public int NumBoundingPlanes { get; private set; }
    public int NumBoundingVolumes { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        reader.MarkPosition();
        Min = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()) / 65536f;
        Max = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()) / 65536f;
        Center = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()) / 65536f;
        NumVertices = reader.ReadInt32();
        NumNormals = reader.ReadInt32();
        NumFaces = reader.ReadInt32();
        NumCollisionObjects = reader.ReadInt32();
        NumberCollisionTransforms = reader.ReadInt32();
        NumBoundingPlanes = reader.ReadInt32();
        NumBoundingVolumes = reader.ReadInt32();
        reader.AssertBytesRead(64);
    }

    public static CollisionModelData FromNode(ModelNode node)
    {
        if (node.Identifier != "CMDL")
            throw new ArgumentException("Node is not a CMDL node", nameof(node));

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var data = new CollisionModelData();
        data.Deserialize(reader);
        return data;
    }
}
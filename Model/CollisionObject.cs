using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public class CollisionObject
{
    public int NumVerts { get; private set; }
    public int NumFaces { get; private set; }
    public int NumPlanes { get; private set; }
    public int NumBoundingVolumes { get; private set; }
    public int ParentSubObjectIndex { get; private set; }
    public Vector3 Offset { get; private set; }
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    public Vector3 Med { get; private set; }
    public float Radius { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        reader.MarkPosition();
        var unk1 = reader.ReadInt32();
        Debug.Assert(unk1 == 0);

        NumVerts = reader.ReadInt32();
        NumFaces = reader.ReadInt32();
        NumPlanes = reader.ReadInt32();
        NumBoundingVolumes = reader.ReadInt32();
        ParentSubObjectIndex = reader.ReadInt32();

        var unk3 = reader.ReadInt32();
        Debug.Assert(unk3 == 0);
        var unk4 = reader.ReadInt32();
        Debug.Assert(unk4 == 0);
        var unk5 = reader.ReadInt32();
        Debug.Assert(unk5 == 0);

        Offset = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        Min = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        Max = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        Med = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        Radius = reader.ReadInt32();
        reader.AssertBytesRead(88);
    }

    public static List<CollisionObject> FromNode(ModelNode node)
    {
        if (node.Identifier != "COBJ")
            throw new ArgumentException("Node is not a COBJ node", nameof(node));

        var collisionObjects = new List<CollisionObject>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        for (var i = 0; i < recordCount; i++)
        {
            var obj = new CollisionObject();
            obj.Deserialize(reader);
            collisionObjects.Add(obj);
        }

        return collisionObjects;
    }
}
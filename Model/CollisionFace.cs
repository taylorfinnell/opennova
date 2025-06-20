using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public class CollisionFace
{
    public int VertIndex1 { get; private set; }
    public int VertIndex2 { get; private set; }
    public int VertIndex3 { get; private set; }
    public int NormalIndex { get; private set; }
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    public bool DoubleSided { get; private set; }
    public bool MoveIgnore { get; private set; }
    public SurfaceType SurfaceType { get; private set; }
    public double Radius { get; private set; }
    public int Unknown4 { get; private set; }
    public int Unknown5 { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        VertIndex1 = reader.ReadInt16();
        VertIndex2 = reader.ReadInt16();
        VertIndex3 = reader.ReadInt16();
        NormalIndex = reader.ReadInt16();

        Min = reader.ReadVector3FP();
        Min = new Vector3(-Min.X, Min.Y, Min.Z);
        Max = reader.ReadVector3FP();
        Max = new Vector3(-Max.X, Max.Y, Max.Z);

        Radius = reader.ReadInt32() / 65536.0;

        var flags = (CollisionFaceFlags)reader.ReadInt32();
        DoubleSided = !flags.HasFlag(CollisionFaceFlags.BackfaceIgnore);
        MoveIgnore = flags.HasFlag(CollisionFaceFlags.MoveIgnore);

        SurfaceType = (SurfaceType)reader.ReadInt16();
        Debug.Assert((int)SurfaceType < 20);

        Unknown4 = reader.ReadByte();
        Unknown5 = reader.ReadByte();
    }

    public static List<CollisionFace> FromNode(ModelNode node)
    {
        if (node.Identifier != "CFAC")
            throw new ArgumentException("Node is not a CFAC node", nameof(node));

        var faces = new List<CollisionFace>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        reader.MarkPosition();
        for (var i = 0; i < recordCount; i++)
        {
            var face = new CollisionFace();
            face.Deserialize(reader);
            faces.Add(face);
        }
        reader.AssertBytesRead(recordCount * recordSize);

        return faces;
    }
}
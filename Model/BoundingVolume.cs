using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public class BoundingVolume
{
    public NodeType CollidableType { get; private set; }
    public BlinkBoxFlags Flags { get; private set; }
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    public uint NumBoundingPlanes { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        reader.MarkPosition();
        var rawCollidable = reader.ReadUInt32();
        CollidableType = (NodeType)rawCollidable;

        var rawFlag = reader.ReadByte();
        Flags = (BlinkBoxFlags)rawFlag;
        var p1 = reader.ReadByte();
        Debug.Assert(p1 == 0);
        var p2 = reader.ReadByte();
        Debug.Assert(p2 == 0);
        var p3 = reader.ReadByte();
        Debug.Assert(p3 == 0);

        Min = reader.ReadVector3FP();
        Max = reader.ReadVector3FP();
        NumBoundingPlanes = reader.ReadUInt32();

        // Never seen these bits turned high
        Debug.Assert(!Flags.HasFlag(BlinkBoxFlags.U4));
        Debug.Assert(!Flags.HasFlag(BlinkBoxFlags.U5));

        reader.AssertBytesRead(36);
    }

    public static List<BoundingVolume> FromNode(ModelNode node)
    {
        if (node.Identifier != "BVOL")
            throw new ArgumentException("Node is not a BVOL node", nameof(node));

        var boundingVolumes = new List<BoundingVolume>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        for (var i = 0; i < recordCount; i++)
        {
            var volume = new BoundingVolume();
            volume.Deserialize(reader);
            boundingVolumes.Add(volume);
        }

        return boundingVolumes;
    }
}
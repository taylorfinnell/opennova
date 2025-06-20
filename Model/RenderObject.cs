using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenNova.Model;

[GlobalClass]
public partial class RenderObject : Resource
{
    [Export] public int NumStrips { get; set; }
    [Export] public int NumAlphaStrips { get; set; }
    [Export] public int ParentIndex { get; set; }
    [Export] public float RelativeX { get; set; }
    [Export] public float RelativeY { get; set; }
    [Export] public float RelativeZ { get; set; }
    [Export] public float AbsX { get; set; }
    [Export] public float AbsY { get; set; }
    [Export] public float AbsZ { get; set; }
    [Export] public float BoundingCenterX { get; set; }
    [Export] public float BoundingCenterY { get; set; }
    [Export] public float BoundingCenterZ { get; set; }
    [Export] public float BoundingRadius { get; set; }

    public static List<RenderObject> FromNode(ModelNode node)
    {
        if (node.Identifier != "ROBJ") throw new ArgumentException("Node is not a ROBJ node");

        var objects = new List<RenderObject>();

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            var recordCount = reader.ReadInt32();
            var recordSize = reader.ReadInt32();

            for (var i = 0; i < recordCount; i++)
            {
                var numStrips = reader.ReadInt32();
                var numAlphaStrips = reader.ReadInt32();
                var subObjectIndex = reader.ReadInt32();
                var diffX = reader.ReadSingle();
                var diffY = reader.ReadSingle();
                var diffZ = reader.ReadSingle();
                var absX = reader.ReadSingle();
                var absY = reader.ReadSingle();
                var absZ = reader.ReadSingle();
                var boundingCenterX = reader.ReadSingle();
                var boundingCenterY = reader.ReadSingle();
                var boundingCenterZ = reader.ReadSingle();
                var boundingRadius = reader.ReadSingle();

                objects.Add(new RenderObject
                {
                    NumStrips = numStrips,
                    NumAlphaStrips = numAlphaStrips,
                    ParentIndex = subObjectIndex,
                    RelativeX = diffX,
                    RelativeY = diffY,
                    RelativeZ = diffZ,
                    AbsX = absX,
                    AbsY = absY,
                    AbsZ = absZ,
                    BoundingCenterX = boundingCenterX,
                    BoundingCenterY = boundingCenterY,
                    BoundingCenterZ = boundingCenterZ,
                    BoundingRadius = boundingRadius
                });
            }
        }

        return objects;
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // Write fields
            writer.Write(NumStrips);
            writer.Write(NumAlphaStrips);
            writer.Write(ParentIndex);
            writer.Write(RelativeX);
            writer.Write(RelativeY);
            writer.Write(RelativeZ);
            writer.Write(AbsX);
            writer.Write(AbsY);
            writer.Write(AbsZ);
            writer.Write(BoundingCenterX);
            writer.Write(BoundingCenterY);
            writer.Write(BoundingCenterZ);
            writer.Write(BoundingRadius);

            return ms.ToArray();
        }
    }

    public static ModelNode ToNode(List<RenderObject> objects)
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(objects.Count);
            writer.Write(52);

            foreach (var obj in objects)
            {
                var objData = obj.ToBytes();
                writer.Write(objData);
            }

            return new ModelNode("ROBJ", ms.ToArray());
        }
    }
}
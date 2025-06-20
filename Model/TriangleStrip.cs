using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SysVector3 = System.Numerics.Vector3;

namespace OpenNova.Model;

[GlobalClass]
public partial class TriangleStrip : Resource
{
    public TriangleStrip()
    {
        MaterialIndex = 0;
        IndexOffset = 0;
        NumIndices = 0;
        NumTriangles = 0;
        IsStrip = 0;
        StartVertex = 0;
        NumVertices = 0;
        Min = new SysVector3(0, 0, 0);
        Max = new SysVector3(0, 0, 0);
    }

    [Export] public int MaterialIndex { get; set; }
    [Export] public int IndexOffset { get; set; }
    [Export] public ushort NumIndices { get; set; }
    [Export] public ushort NumTriangles { get; set; }
    [Export] public int IsStrip { get; set; }
    [Export] public int StartVertex { get; set; }
    [Export] public int NumVertices { get; set; }
    public SysVector3 Min { get; set; }
    public SysVector3 Max { get; set; }
    [Export] public byte[] BoneTable { get; set; } = new byte[16];
    [Export] public int BoneTableLength { get; set; }
    public bool HasBoneTable => BoneTableLength > 0;

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(MaterialIndex);
            writer.Write(IndexOffset);
            writer.Write(NumIndices);
            writer.Write(NumTriangles);
            writer.Write(IsStrip);
            writer.Write(StartVertex);
            writer.Write(NumVertices);

            writer.Write(Min.X);
            writer.Write(Min.Y);
            writer.Write(Min.Z);
            writer.Write(Max.X);
            writer.Write(Max.Y);
            writer.Write(Max.Z);

            if (HasBoneTable)
            {
                writer.Write(BoneTable, 0, Math.Min(BoneTable.Length, 16));
                if (BoneTable.Length < 16) writer.Write(new byte[16 - BoneTable.Length]);

                writer.Write(BoneTableLength);
            }

            return ms.ToArray();
        }
    }

    public static List<TriangleStrip> FromNode(ModelNode node)
    {
        if (node.Identifier != "STRP") throw new ArgumentException("Node is not a STRP node");

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            var stripCount = reader.ReadInt32();
            var recordSize = reader.ReadInt32();

            var strips = new List<TriangleStrip>(stripCount);

            Debug.Assert(recordSize == 48 || recordSize == 68);

            for (var i = 0; i < stripCount; i++)
            {
                var start = (int)reader.BaseStream.Position;
                // Read strip data
                var materialIndex = reader.ReadInt32();
                var indexOffset = reader.ReadInt32();
                var numIndices = reader.ReadUInt16();
                var numTriangles = reader.ReadUInt16();
                var isStrip = reader.ReadInt32();
                var startVertex = reader.ReadInt32();
                var numVerts = reader.ReadInt32();

                // Bounds
                var minX = reader.ReadSingle();
                var minY = reader.ReadSingle();
                var minZ = reader.ReadSingle();
                var maxX = reader.ReadSingle();
                var maxY = reader.ReadSingle();
                var maxZ = reader.ReadSingle();
                Debug.Assert(isStrip == 0);
                var end = (int)reader.BaseStream.Position;
                Debug.Assert(end - start == 48);

                // Create the triangle strip first so we can use it
                var strip = new TriangleStrip
                {
                    MaterialIndex = materialIndex,
                    IndexOffset = indexOffset,
                    NumIndices = numIndices,
                    NumTriangles = numTriangles,
                    IsStrip = isStrip,
                    StartVertex = startVertex,
                    NumVertices = numVerts,
                    Min = new SysVector3(minX, minY, minZ),
                    Max = new SysVector3(maxX, maxY, maxZ)
                };

                // Read bone table if present
                if (recordSize == 68)
                {
                    var boneTable = reader.ReadBytes(16); // bone table (16 bytes)
                    var boneTableLen = reader.ReadInt32(); // bone table len
                    end = (int)reader.BaseStream.Position;
                    Debug.Assert(end - start == 68);

                    // Save bone table data
                    if (boneTableLen > 0 && boneTableLen <= 16)
                    {
                        strip.BoneTable = boneTable;
                        strip.BoneTableLength = boneTableLen;
                    }
                }

                strips.Add(strip);
            }

            return strips;
        }
    }
}
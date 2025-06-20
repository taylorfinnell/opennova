using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SysVector2 = System.Numerics.Vector2;
using SysVector3 = System.Numerics.Vector3;

namespace OpenNova.Model;

[GlobalClass]
public partial class Vertex : Resource
{
    public Vertex()
    {
        Position = SysVector3.Zero;
        Normal = SysVector3.UnitY;
        TexCoord0 = SysVector2.Zero;
        TexCoord1 = SysVector2.Zero;
        Tangent = SysVector3.UnitX;
        Bitangent = SysVector3.UnitZ;
        BoneWeights = SysVector3.Zero;
    }

    public SysVector3 Position { get; set; }
    public SysVector3 Normal { get; set; }
    public SysVector2 TexCoord0 { get; set; }
    public SysVector2 TexCoord1 { get; set; }
    public SysVector3 Tangent { get; set; }
    public SysVector3 Bitangent { get; set; }
    public SysVector3 BoneWeights { get; set; }
    [Export] public byte[] BoneIndices { get; set; } = new byte[4];
    [Export] public int Flag { get; set; }

    public bool HasTangents()
    {
        return (Flag & 0x14) > 0;
    }

    public bool IsSkinned()
    {
        return (Flag & 0x40) > 0;
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);

            if (IsSkinned())
            {
                writer.Write(BoneWeights.X);
                writer.Write(BoneWeights.Y);
                writer.Write(BoneWeights.Z);
                writer.Write(BoneIndices[0]);
                writer.Write(BoneIndices[1]);
                writer.Write(BoneIndices[2]);
                writer.Write(BoneIndices[3]);
            }

            writer.Write(Normal.X);
            writer.Write(Normal.Y);
            writer.Write(Normal.Z);

            writer.Write(TexCoord0.X);
            writer.Write(TexCoord0.Y);
            writer.Write(TexCoord1.X);
            writer.Write(TexCoord1.Y);

            if (HasTangents())
            {
                writer.Write(Tangent.X);
                writer.Write(Tangent.Y);
                writer.Write(Tangent.Z);

                writer.Write(Bitangent.X);
                writer.Write(Bitangent.Y);
                writer.Write(Bitangent.Z);
            }

            return ms.ToArray();
        }
    }

    public static List<Vertex> FromNode(ModelNode node, out int vertexCount)
    {
        if (node.Identifier != "VERT") throw new ArgumentException("Node is not a VERT node");

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            // Read collection header
            vertexCount = reader.ReadInt32();
            var recordSize = reader.ReadInt32();
            var flag = reader.ReadInt32();

            var hasTangents = (flag & 0x14) > 0;
            var isSkinned = (flag & 0x40) > 0;

            var vertices = new List<Vertex>(vertexCount);

            // Handle each vertex based on the format
            for (var i = 0; i < vertexCount; i++)
            {
                var startPos = (int)reader.BaseStream.Position;
                var vertex = new Vertex { Flag = flag };

                // Read position (always present)
                var posX = reader.ReadSingle();
                var posY = reader.ReadSingle();
                var posZ = reader.ReadSingle();
                vertex.Position = new SysVector3(posX, posY, posZ);

                if (isSkinned)
                {
                    var boneWeight1 = reader.ReadSingle();
                    var boneWeight2 = reader.ReadSingle();
                    var boneWeight3 = reader.ReadSingle();

                    vertex.BoneWeights = new SysVector3(boneWeight1, boneWeight2, boneWeight3);
                    vertex.BoneIndices[0] = reader.ReadByte();
                    vertex.BoneIndices[1] = reader.ReadByte();
                    vertex.BoneIndices[2] = reader.ReadByte();
                    vertex.BoneIndices[3] = reader.ReadByte();
                }

                var normX = reader.ReadSingle();
                var normY = reader.ReadSingle();
                var normZ = reader.ReadSingle();
                vertex.Normal = new SysVector3(normX, normY, normZ);

                var uv0X = reader.ReadSingle();
                var uv0Y = reader.ReadSingle();
                vertex.TexCoord0 = new SysVector2(uv0X, uv0Y);

                var uv1X = reader.ReadSingle();
                var uv1Y = reader.ReadSingle();
                vertex.TexCoord1 = new SysVector2(uv1X, uv1Y);

                if (hasTangents)
                {
                    var tanX = reader.ReadSingle();
                    var tanY = reader.ReadSingle();
                    var tanZ = reader.ReadSingle();
                    vertex.Tangent = new SysVector3(tanX, tanY, tanZ);

                    var biTanX = reader.ReadSingle();
                    var biTanY = reader.ReadSingle();
                    var biTanZ = reader.ReadSingle();
                    vertex.Bitangent = new SysVector3(biTanX, biTanY, biTanZ);
                }

                var endPos = reader.BaseStream.Position;
                var bytesRead = (int)(endPos - startPos);

                Debug.Assert(bytesRead == recordSize);

                vertices.Add(vertex);
            }

            return vertices;
        }
    }

    // Calculate the fourth bone weight (weights must sum to 1.0)
    public float GetFourthBoneWeight()
    {
        var sum = BoneWeights.X + BoneWeights.Y + BoneWeights.Z;
        var weight4 = 1.0f - sum;
        return Math.Max(0.0f, weight4); // Ensure non-negative
    }
}
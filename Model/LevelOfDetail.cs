using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenNova.Model;

internal class LevelOfDetail
{
    public List<RenderObject> RenderObjects { get; set; } = new();
    public string ModelName { get; set; } = string.Empty;
    public int VertexCount { get; set; }
    public int IndexCount { get; set; }
    public int LodThreshold { get; set; }
    public int RenderObjectCount { get; set; }
    public List<ushort> Indices { get; set; } = new();
    public List<Vertex> Vertices { get; set; } = new();
    public List<TriangleStrip> Strips { get; set; } = new();

    public static LevelOfDetail FromNode(ModelNode node)
    {
        if (node.Identifier != "RLOD") throw new ArgumentException("Node is not a RLOD node");

        var lod = new LevelOfDetail();

        foreach (var childNode in node.Children)
            switch (childNode.Identifier)
            {
                case "RMDL":
                    var rmdlData = RenderModelData.FromNode(childNode);
                    lod.ModelName = rmdlData.ModelType;
                    lod.LodThreshold = rmdlData.LodThreshold;
                    lod.RenderObjectCount = rmdlData.RenderObjectCount;
                    break;
                case "VERT":
                    lod.Vertices = Vertex.FromNode(childNode, out var vertexCount);
                    lod.VertexCount = vertexCount;
                    break;
                case "INDX":
                    lod.Indices = IndexBuffer.FromNode(childNode, out var indexCount);
                    lod.IndexCount = indexCount;
                    break;
                case "STRP":
                    lod.Strips = TriangleStrip.FromNode(childNode);
                    break;
                case "ROBJ":
                    lod.RenderObjects = RenderObject.FromNode(childNode);
                    break;
            }

        return lod;
    }

    public ModelNode ToNode()
    {
        var children = new List<ModelNode>();

        children.Add(new RenderModelData
        {
            ModelType = ModelName,
            LodThreshold = LodThreshold,
            RenderObjectCount = RenderObjectCount
        }.ToNode());

        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            var vertexSize = 40;
            var vertexFlag = 0;

            if (Vertices.Count > 0)
            {
                var firstVertex = Vertices[0];
                vertexFlag = firstVertex.Flag;

                var hasTangents = firstVertex.HasTangents();
                var isSkinned = firstVertex.IsSkinned();

                // Ensure flag has correct bits set
                if (hasTangents)
                    vertexFlag |= 0x14; // Set tangent flag bits

                if (isSkinned)
                    vertexFlag |= 0x40; // Set skinned flag bit

                // Calculate vertex size based on format
                if (isSkinned && hasTangents)
                    vertexSize = 80; // Skinned + Tangents (80 bytes)
                else if (isSkinned)
                    vertexSize = 56; // Skinned only (56 bytes)
                else if (hasTangents)
                    vertexSize = 64; // Tangents only (64 bytes)
                else
                    vertexSize = 40; // Basic format (40 bytes)
            }

            // Write collection header
            writer.Write(Vertices.Count);
            writer.Write(vertexSize);
            writer.Write(vertexFlag);

            // Write all vertices
            foreach (var vertex in Vertices)
            {
                var vertexData = vertex.ToBytes();
                writer.Write(vertexData);
            }

            children.Add(new ModelNode("VERT", ms.ToArray()));
        }

        // Create INDX node for index data
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // Write collection header
            writer.Write(Indices.Count);
            writer.Write(2); // Each index is 2 bytes (ushort)

            // Write all indices
            foreach (var index in Indices) writer.Write(index);

            children.Add(new ModelNode("INDX", ms.ToArray()));
        }

        // Add STRP node for triangle strips if we have any
        if (Strips.Count > 0)
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Determine if strips have bone tables
                var hasBoneTables = Strips.Any(s => s.HasBoneTable);
                var stripSize = hasBoneTables ? 68 : 48; // 68 bytes with bone table, 48 without

                // Write collection header
                writer.Write(Strips.Count);
                writer.Write(stripSize);

                // Write all strips
                foreach (var strip in Strips)
                {
                    var stripData = strip.ToBytes();
                    writer.Write(stripData);
                }

                children.Add(new ModelNode("STRP", ms.ToArray()));
            }

        // Add ROBJ node if we have render objects
        if (RenderObjects.Count > 0) children.Add(RenderObject.ToNode(RenderObjects));

        // Create RLOD node with all children
        return new ModelNode("RLOD", children);
    }
}
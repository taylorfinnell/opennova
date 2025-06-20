using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OpenNova.Model;

[GlobalClass]
public partial class IndexBuffer : Resource
{
    public static List<ushort> FromNode(ModelNode node, out int indexCount)
    {
        if (node.Identifier != "INDX") throw new ArgumentException("Node is not an INDX node");

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            indexCount = reader.ReadInt32();
            var recordSize = reader.ReadInt32();
            Debug.Assert(recordSize == 2);

            var indices = new List<ushort>(indexCount);

            for (var i = 0; i < indexCount; i++) indices.Add(reader.ReadUInt16());

            return indices;
        }
    }

    public ModelNode ToNode(List<ushort> indices)
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(indices.Count);
            writer.Write(2);

            foreach (var index in indices) writer.Write(index);

            return new ModelNode("INDX", ms.ToArray());
        }
    }
}
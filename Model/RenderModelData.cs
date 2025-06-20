using System;
using System.IO;

using System.Text;

namespace OpenNova.Model;

internal class RenderModelData
{
    public string ModelType { get; set; } = string.Empty;
    public int LodThreshold { get; set; }
    public int RenderObjectCount { get; set; }

    public static RenderModelData FromNode(ModelNode node)
    {
        if (node.Identifier != "RMDL") throw new ArgumentException("Node is not a RMDL node");

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            var typeBytes = reader.ReadBytes(4);
            var modelType = Encoding.ASCII.GetString(typeBytes);

            var lodThreshold = reader.ReadInt32();
            var renderObjectCount = reader.ReadInt32();

            return new RenderModelData
            {
                ModelType = modelType.TrimEnd('\0'),
                LodThreshold = lodThreshold,
                RenderObjectCount = renderObjectCount
            };
        }
    }

    public ModelNode ToNode()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            var typeBytes = Encoding.ASCII.GetBytes(ModelType.PadRight(4, '\0'));
            writer.Write(typeBytes, 0, 4);

            writer.Write(LodThreshold);
            writer.Write(RenderObjectCount);

            return new ModelNode("RMDL", ms.ToArray());
        }
    }
}
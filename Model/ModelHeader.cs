using Godot;
using System;
using System.IO;
using System.Text;

namespace OpenNova.Model;

public enum MeshType
{
    INVALID = 0,
    BASIC = 1,
    SKINNED = 2
}

[GlobalClass]
public partial class ModelHeader : Resource
{
    [Export] public string Name { get; set; } = "";
    [Export] public MeshType MeshType { get; set; }
    [Export] public int NumLods { get; set; }
    [Export] public int LodDistance { get; set; }

    public static ModelHeader FromNode(ModelNode node)
    {
        if (node.Identifier != "GHDR") throw new ArgumentException("Node is not a GHDR node");

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            var nameBytes = reader.ReadBytes(16);
            var name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');

            var meshType = reader.ReadInt32();
            var numLods = reader.ReadInt32();
            var lodDistance = reader.ReadInt32();

            return new ModelHeader
            {
                Name = name,
                MeshType = (MeshType)meshType,
                NumLods = numLods,
                LodDistance = lodDistance
            };
        }
    }

    public ModelNode ToNode()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            var nameBytes = Encoding.UTF8.GetBytes(Name.PadRight(16, '\0'));
            writer.Write(nameBytes, 0, Math.Min(nameBytes.Length, 16));

            var padding = 16 - Math.Min(nameBytes.Length, 16);
            if (padding > 0) writer.Write(new byte[padding]);

            writer.Write((int)MeshType);
            writer.Write(NumLods);
            writer.Write(LodDistance);

            return new ModelNode("GHDR", ms.ToArray());
        }
    }
}
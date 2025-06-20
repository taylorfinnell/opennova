using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenNova.Model;

[GlobalClass]
public partial class ModelNode : Resource
{
    private const int PARENT_FLAG = 1 << 31;
    private const int LENGTH_MASK = 0x00FFFFFF;

    public ModelNode()
    {
        // Parameterless constructor for Godot
    }

    public ModelNode(string identifier, byte[] data)
    {
        Identifier = identifier.PadRight(4).Substring(0, 4);
        IsParent = false;
        Data = data;
        Children = new Godot.Collections.Array<ModelNode>();
    }

    public ModelNode(string identifier, List<ModelNode> children)
    {
        Identifier = identifier.PadRight(4).Substring(0, 4);
        IsParent = true;
        Data = Array.Empty<byte>();
        Children = new Godot.Collections.Array<ModelNode>(children);
    }

    [Export] public string Identifier { get; set; } = "";
    [Export] public bool IsParent { get; set; }
    [Export] public byte[] Data { get; set; } = Array.Empty<byte>();
    [Export] public Godot.Collections.Array<ModelNode> Children { get; set; }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Encoding.ASCII.GetBytes(Identifier));

        if (IsParent)
        {
            var contentSize = 0;
            using (var ms = new MemoryStream())
            {
                var childWriter = new BinaryWriter(ms);
                foreach (var child in Children) child.Write(childWriter);
                contentSize = (int)ms.Length;
            }

            writer.Write(PARENT_FLAG | contentSize & LENGTH_MASK);

            foreach (var child in Children) child.Write(writer);
        }
        else
        {
            writer.Write(Data.Length);
            writer.Write(Data);
        }
    }

    public static ModelNode Read(BinaryReader reader)
    {
        var identifier = Encoding.ASCII.GetString(reader.ReadBytes(4));

        var flag = reader.ReadInt32();
        var isParent = (flag & PARENT_FLAG) != 0;
        var contentLength = flag & LENGTH_MASK;

        if (isParent)
        {
            var children = new List<ModelNode>();
            var startPosition = reader.BaseStream.Position;
            var endPosition = startPosition + contentLength;

            while (reader.BaseStream.Position < endPosition) children.Add(Read(reader));

            return new ModelNode(identifier, children);
        }

        var data = reader.ReadBytes(contentLength);
        return new ModelNode(identifier, data);
    }
}

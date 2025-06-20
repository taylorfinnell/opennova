using Godot;
using System;
using System.IO;
using System.Text;

namespace OpenNova.Model;

public class ModelFile
{
    private const string MAGIC = "3DI3";
    private const int VERSION = 259;

    public ModelFile(ModelNode rootNode)
    {
        RootNode = rootNode;
    }

    [Export] public ModelNode RootNode { get; set; }

    public void Write(string filePath)
    {
        using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        using (var writer = new BinaryWriter(fs))
        {
            writer.Write(Encoding.ASCII.GetBytes(MAGIC));
            writer.Write(VERSION);
            RootNode.Write(writer);
        }
    }

    public static ModelFile Read(string filePath)
    {
        using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        using (var reader = new BinaryReader(fs))
        {
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (magic != MAGIC)
                throw new InvalidOperationException($"Invalid magic number: {magic}. Expected: {MAGIC}");

            var version = reader.ReadInt32();
            if (version != VERSION)
                throw new InvalidOperationException($"Invalid version: {version}. Expected: {VERSION}");

            var rootNode = ModelNode.Read(reader);
            return new ModelFile(rootNode);
        }
    }

    public static ModelFile Parse(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        using (var reader = new BinaryReader(ms))
        {
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (magic != MAGIC)
                throw new InvalidOperationException($"Invalid magic number: {magic}. Expected: {MAGIC}");

            var version = reader.ReadInt32();
            if (version != VERSION)
                throw new InvalidOperationException($"Invalid version: {version}. Expected: {VERSION}");

            var rootNode = ModelNode.Read(reader);
            return new ModelFile(rootNode);
        }
    }

    public string GetModelName()
    {
        if (RootNode == null)
            return "Unknown";

        // Find GHDR node
        var ghdrNode = FindNodeByPath("ROOT/GHDR");
        if (ghdrNode == null)
            return "Unknown";

        var header = ModelHeader.FromNode(ghdrNode);
        return header.Name;
    }

    private ModelNode FindNodeByPath(string path)
    {
        if (RootNode == null)
            return null;

        string[] parts = path.Split('/');
        if (parts.Length == 0 || parts[0] != RootNode.Identifier)
            return null;

        ModelNode current = RootNode;
        for (int i = 1; i < parts.Length; i++)
        {
            bool found = false;
            foreach (var child in current.Children)
            {
                if (child.Identifier == parts[i])
                {
                    current = child;
                    found = true;
                    break;
                }
            }
            if (!found)
                return null;
        }
        return current;
    }
}
using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenNova.Model;

public class CollisionTranslation
{
    public Vector3 Translation { get; private set; }

    public void Deserialize(ONBinaryReader reader)
    {
        reader.MarkPosition();
        Translation = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        reader.AssertBytesRead(12);
    }

    public static List<CollisionTranslation> FromNode(ModelNode node)
    {
        if (node.Identifier != "CXLT")
            throw new ArgumentException("Node is not a CXLT node", nameof(node));

        var translations = new List<CollisionTranslation>();

        using var stream = new MemoryStream(node.Data);
        using var reader = new ONBinaryReader(stream);

        var recordCount = reader.ReadInt32();
        var recordSize = reader.ReadInt32();

        for (var i = 0; i < recordCount; i++)
        {
            var translation = new CollisionTranslation();
            translation.Deserialize(reader);
            translations.Add(translation);
        }

        return translations;
    }
}
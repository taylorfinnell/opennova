using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace OpenNova.Model;

public class ONMaterial
{

    public int MaterialIndex { get; set; }
    public string ShaderName { get; set; } = string.Empty;
    public List<MaterialTexture> Textures { get; } = new();
    public MaterialFlags MaterialFlags { get; set; }
    private AlphaGeneration AlphaGen { get; set; }
    private RGBGeneration RGBGen { get; set; }
    private UVParameters UParameters { get; set; }
    private UVParameters VParameters { get; set; }
    private TextureAnimation Animation { get; set; }
    public float AlphaTestValue { get; private set; }
    public Vector4 ReflectColor { get; private set; }
    public bool IsGlass { get; private set; }
    public byte EmissiveType { get; private set; }

    public bool IsDoubleSided()
    {
        return MaterialFlags.HasFlag(MaterialFlags.TwoSided);
    }

    public bool IsAlphaTested()
    {
        return MaterialFlags.HasFlag(MaterialFlags.AlphaTest);
    }

    public bool IsAlphaTestedInverted()
    {
        return MaterialFlags.HasFlag(MaterialFlags.AlphaTest) &&
               MaterialFlags.HasFlag(MaterialFlags.AlphaTestInverted);
    }

    public float GetAlphaTestValue()
    {
        return IsAlphaTestedInverted() ? 1.0f - AlphaTestValue : AlphaTestValue;
    }

    public static ONMaterial FromBytes(BinaryReader reader, int materialIndex)
    {
        var material = new ONMaterial { MaterialIndex = materialIndex };

        var nameBytes = reader.ReadBytes(0x20);
        material.ShaderName = Encoding.Default.GetString(nameBytes).Trim('\0');

        var numTextures = reader.ReadInt32();
        for (var i = 0; i < numTextures; i++)
        {
            var texture = MaterialTexture.FromBytes(reader);
            material.Textures.Add(texture);
        }

        reader.ReadBytes((24 - numTextures) * 20);

        material.AlphaGen = AlphaGeneration.FromBytes(reader);
        material.RGBGen = RGBGeneration.FromBytes(reader);

        reader.ReadBytes(12);

        material.UParameters = UVParameters.FromBytes(reader);
        material.VParameters = UVParameters.FromBytes(reader);

        material.ReflectColor = ReadVector4ByteBackwards(reader);

        var unknown1 = reader.ReadInt32();
        Debug.Assert(unknown1 == 0);

        var unknown2 = reader.ReadByte();
        material.EmissiveType = unknown2;

        var unknown3 = reader.ReadByte();
        Debug.Assert(unknown3 == 0);

        var isGlass = reader.ReadByte();
        material.IsGlass = isGlass == 1;

        var unknown4 = reader.ReadByte();
        Debug.Assert(unknown4 == 0);

        material.MaterialFlags = (MaterialFlags)reader.ReadByte();

        material.AlphaTestValue = reader.ReadByte() / 256f;

        var unknown5 = reader.ReadByte();
        var unknown6 = reader.ReadByte();
        Debug.Assert(unknown5 == 0);
        Debug.Assert(unknown6 == 0);

        material.Animation = TextureAnimation.FromBytes(reader);

        return material;
    }

    private static Vector4 ReadVector4ByteBackwards(BinaryReader reader)
    {
        var b = reader.ReadByte();
        var g = reader.ReadByte();
        var r = reader.ReadByte();
        var a = reader.ReadByte();

        return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}

public class MaterialCollection
{
    public int NumMaterials { get; private set; }
    public int MaterialSize { get; private set; }
    public List<ONMaterial> Materials { get; } = new();

    internal static MaterialCollection FromNode(ModelNode node)
    {
        if (node.Identifier != "MTRL") throw new ArgumentException("Node is not a MTRL node");

        var collection = new MaterialCollection();

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            // Read the collection header
            collection.NumMaterials = reader.ReadInt32();
            collection.MaterialSize = reader.ReadInt32();

            for (var i = 0; i < collection.NumMaterials; i++)
            {
                var material = ONMaterial.FromBytes(reader, i);
                collection.Materials.Add(material);
            }
        }

        return collection;
    }
}
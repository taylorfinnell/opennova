using System;
using System.IO;
using System.Text;

namespace OpenNova.Model;

public struct MaterialTexture
{
    public string TextureName { get; set; }
    public TextureSlot TextureSlot { get; set; }
    public TextureType TextureType { get; set; }
    public TextureFlags TextureFlags { get; set; }
    public byte FrameNumber { get; set; }

    public static MaterialTexture FromBytes(BinaryReader reader)
    {
        var texture = new MaterialTexture();

        var nameBytes = reader.ReadBytes(0x10);
        var textureName = Encoding.Default.GetString(nameBytes).Trim('\0');

        texture.TextureName = Path.GetFileName(textureName).ToLower();

        if (texture.TextureName.EndsWith(".tga")) texture.TextureName = texture.TextureName.Replace(".tga", ".dds");

        texture.TextureSlot = (TextureSlot)reader.ReadByte();
        texture.TextureType = (TextureType)reader.ReadByte();
        texture.TextureFlags = (TextureFlags)reader.ReadByte();
        texture.FrameNumber = reader.ReadByte();

        return texture;
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            var nameBytes = Encoding.Default.GetBytes(TextureName.PadRight(16, '\0'));
            writer.Write(nameBytes, 0, Math.Min(nameBytes.Length, 16));

            writer.Write((byte)TextureSlot);
            writer.Write((byte)TextureType);
            writer.Write((byte)TextureFlags);
            writer.Write(FrameNumber);

            return ms.ToArray();
        }
    }
}

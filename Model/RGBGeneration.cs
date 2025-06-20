using System.IO;
using System.Numerics;

namespace OpenNova.Model;

internal class RGBGeneration
{
    public int Style { get; private set; }
    public float Rate { get; private set; }
    public Vector4 StartColor { get; private set; }
    public Vector4 EndColor { get; private set; }
    public float Phase { get; private set; }
    public int Register { get; set; } = -1;

    public static RGBGeneration FromBytes(BinaryReader reader)
    {
        var rgbGen = new RGBGeneration();

        rgbGen.Style = reader.ReadByte();

        var phaseOrRegister = reader.ReadByte();
        if (rgbGen.Style <= 112)
            rgbGen.Phase = phaseOrRegister / 256f;
        else
            rgbGen.Register = phaseOrRegister;

        rgbGen.Rate = reader.ReadInt16() / 256f;

        rgbGen.StartColor = ReadVector4ByteBackwards(reader);
        rgbGen.EndColor = ReadVector4ByteBackwards(reader);

        return rgbGen;
    }

    private static Vector4 ReadVector4ByteBackwards(BinaryReader reader)
    {
        var b = reader.ReadByte();
        var g = reader.ReadByte();
        var r = reader.ReadByte();
        var a = reader.ReadByte();

        return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    private static void WriteVector4ByteBackwards(BinaryWriter writer, Vector4 color)
    {
        writer.Write((byte)(color.Z * 255f)); // B
        writer.Write((byte)(color.Y * 255f)); // G
        writer.Write((byte)(color.X * 255f)); // R
        writer.Write((byte)(color.W * 255f)); // A
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write((byte)Style);

            if (Style <= 112)
                writer.Write((byte)(Phase * 256f));
            else
                writer.Write((byte)Register);

            writer.Write((short)(Rate * 256f));
            WriteVector4ByteBackwards(writer, StartColor);
            WriteVector4ByteBackwards(writer, EndColor);

            return ms.ToArray();
        }
    }
}

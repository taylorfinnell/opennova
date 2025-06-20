using System.IO;

namespace OpenNova.Model;

internal class AlphaGeneration
{
    public byte Style { get; private set; }
    public float Phase { get; private set; }
    public int Register { get; set; } = -1;
    public float Rate { get; private set; }
    public short Start { get; private set; }
    public short End { get; private set; }

    public static AlphaGeneration FromBytes(BinaryReader reader)
    {
        var alphaGen = new AlphaGeneration();

        alphaGen.Style = reader.ReadByte();

        var phaseOrRegister = reader.ReadByte();
        if (alphaGen.Style <= 112)
            alphaGen.Phase = phaseOrRegister / 256f;
        else
            alphaGen.Register = phaseOrRegister;

        alphaGen.Rate = reader.ReadInt16() / 256f;
        alphaGen.Start = reader.ReadInt16();
        alphaGen.End = reader.ReadInt16();

        return alphaGen;
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(Style);

            if (Style <= 112)
                writer.Write((byte)(Phase * 256f));
            else
                writer.Write((byte)Register);

            writer.Write((short)(Rate * 256f));
            writer.Write(Start);
            writer.Write(End);

            return ms.ToArray();
        }
    }
}

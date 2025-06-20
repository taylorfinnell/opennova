using System.IO;

namespace OpenNova.Model;

internal class UVParameters
{
    public byte Style { get; private set; }
    public float Phase { get; private set; }
    public float GenRate { get; private set; }
    public float Start { get; private set; }
    public float End { get; private set; }
    public int Register { get; set; } = -1;

    public static UVParameters FromBytes(BinaryReader reader)
    {
        var uvParams = new UVParameters();

        uvParams.Style = reader.ReadByte();
        var phaseOrRegister = reader.ReadByte();

        if (uvParams.Style <= 112)
            uvParams.Phase = phaseOrRegister / 256f;
        else
            uvParams.Register = phaseOrRegister;

        uvParams.GenRate = reader.ReadInt16() / 256f;
        uvParams.Start = reader.ReadInt16() / 256f;
        uvParams.End = reader.ReadInt16() / 256f;

        return uvParams;
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

            writer.Write((short)(GenRate * 256f));
            writer.Write((short)(Start * 256f));
            writer.Write((short)(End * 256f));

            return ms.ToArray();
        }
    }
}

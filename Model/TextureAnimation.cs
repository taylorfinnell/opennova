using System.IO;

namespace OpenNova.Model;

internal class TextureAnimation
{
    public byte NumFrames { get; private set; }
    public byte AnimationType { get; private set; }
    public short CycleFrameTime { get; private set; }

    public static TextureAnimation FromBytes(BinaryReader reader)
    {
        var animation = new TextureAnimation
        {
            NumFrames = reader.ReadByte(),
            AnimationType = reader.ReadByte(),
            CycleFrameTime = reader.ReadInt16()
        };

        return animation;
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(NumFrames);
            writer.Write(AnimationType);
            writer.Write(CycleFrameTime);

            return ms.ToArray();
        }
    }
}

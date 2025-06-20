
using Godot;
using OpenNova.Core;
using System.IO;

namespace OpenNova.Mission
{
    [GlobalClass]
    public partial class BoundingBox : Resource
    {
        [Export] public float MinX { get; set; }
        [Export] public float MinY { get; set; }
        [Export] public float MinZ { get; set; }
        [Export] public float MaxX { get; set; }
        [Export] public float MaxY { get; set; }
        [Export] public float MaxZ { get; set; }
        public byte[] UnknownData { get; set; }

        public void Deserialize(ONBinaryReader reader)
        {
            var xMinRaw = reader.ReadInt32();
            var yMinRaw = reader.ReadInt32();
            var zMinRaw = reader.ReadInt32();
            var xMaxRaw = reader.ReadInt32();
            var yMaxRaw = reader.ReadInt32();
            var zMaxRaw = reader.ReadInt32();

            MinX = xMinRaw / 65536f;
            MinY = yMinRaw / 65536f;
            MinZ = zMinRaw / 65536f;
            MaxX = xMaxRaw / 65536f;
            MaxY = yMaxRaw / 65536f;
            MaxZ = zMaxRaw / 65536f;

            UnknownData = reader.ReadBytes(12);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((int)(MinX * 65536f));
            writer.Write((int)(MinY * 65536f));
            writer.Write((int)(MinZ * 65536f));
            writer.Write((int)(MaxX * 65536f));
            writer.Write((int)(MaxY * 65536f));
            writer.Write((int)(MaxZ * 65536f));
            writer.Write(UnknownData ?? new byte[12]);
        }
    }
}

using Godot;
using OpenNova.Core;
using System.IO;

namespace OpenNova.Mission
{
    [GlobalClass]
    public partial class AreaTrigger : Resource
    {
        [Export] public int WpNumber { get; set; }
        [Export] public float MinX { get; set; }
        [Export] public float MinY { get; set; }
        [Export] public float MinZ { get; set; }
        [Export] public float MaxX { get; set; }
        [Export] public float MaxY { get; set; }
        [Export] public float MaxZ { get; set; }
        public int Reserved { get; set; }  // Padding or attributes

        public void Deserialize(ONBinaryReader reader)
        {
            WpNumber = reader.ReadInt32();

            // Coordinates are in fixed-point format (divide by 65536)
            var minXRaw = reader.ReadInt32();  // 4-7
            var minYRaw = reader.ReadInt32();  // 8-11  
            var minZRaw = reader.ReadInt32();  // 12-15
            var maxXRaw = reader.ReadInt32();  // 16-19
            var maxYRaw = reader.ReadInt32();  // 20-23
            var maxZRaw = reader.ReadInt32();  // 24-27

            MinX = minXRaw / 65536f;
            MinZ = minYRaw / 65536f;
            MinY = minZRaw / 65536f;
            MaxX = maxXRaw / 65536f;
            MaxZ = maxYRaw / 65536f;
            MaxY = maxZRaw / 65536f;

            Reserved = reader.ReadInt32();
            GD.Print("Reserved: " + Reserved);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(WpNumber);

            // Coordinates are in fixed-point format (multiply by 65536)
            writer.Write((int)(MinX * 65536f));
            writer.Write((int)(MinZ * 65536f));  // Note: MinZ maps to Y
            writer.Write((int)(MinY * 65536f));  // Note: MinY maps to Z
            writer.Write((int)(MaxX * 65536f));
            writer.Write((int)(MaxZ * 65536f));  // Note: MaxZ maps to Y
            writer.Write((int)(MaxY * 65536f));  // Note: MaxY maps to Z

            writer.Write(Reserved);
        }
    }
}

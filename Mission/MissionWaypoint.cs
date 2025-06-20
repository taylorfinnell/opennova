using Godot;
using OpenNova.Core;
using System.IO;

namespace OpenNova.Mission
{

    [GlobalClass]
    public partial class MissionWaypoint : Resource
    {
        [Export] public MissionWaypointFlags Flags { get; set; }
        [Export] public uint MarkerCount { get; set; }
        public Godot.Collections.Array<uint> WaypointNumbers { get; set; }

        public void Initialize()
        {
            WaypointNumbers = new Godot.Collections.Array<uint>();
        }

        public void Deserialize(ONBinaryReader reader)
        {
            reader.MarkPosition();
            Flags = (MissionWaypointFlags)reader.ReadUInt32();
            MarkerCount = reader.ReadUInt32();

            for (int i = 0; i < MarkerCount; i++)
            {
                var n = reader.ReadUInt32();
                WaypointNumbers.Add(n);
            }

            int remainingBytes = 128 - (int)(MarkerCount * 4);
            if (remainingBytes > 0)
            {
                reader.ReadBytes(remainingBytes);
            }

            reader.AssertBytesRead(136);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((uint)Flags);
            writer.Write(MarkerCount);

            // Write waypoint numbers
            for (int i = 0; i < MarkerCount && i < WaypointNumbers.Count; i++)
            {
                writer.Write(WaypointNumbers[i]);
            }

            // Pad remaining space with zeros
            int remainingBytes = 128 - (int)(MarkerCount * 4);
            if (remainingBytes > 0)
            {
                writer.Write(new byte[remainingBytes]);
            }
        }
    }
}

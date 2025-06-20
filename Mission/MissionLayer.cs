using Godot;
using OpenNova.Core;
using System.Diagnostics;
using System.IO;

namespace OpenNova.Mission
{
    [GlobalClass]
    public partial class MissionLayer : Resource
    {
        public byte[] RawData { get; set; }

        public void Deserialize(ONBinaryReader reader)
        {
            RawData = reader.ReadBytes(20);

            for (int i = 0; i < RawData.Length; i++)
            {
                Debug.Assert(RawData[i] == 0, $"Record20 byte at index {i} is not zero: {RawData[i]}");
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(RawData ?? new byte[20]);
        }
    }
}

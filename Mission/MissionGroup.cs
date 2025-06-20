using Godot;
using OpenNova.Core;
using System.Diagnostics;
using System.IO;

namespace OpenNova.Mission
{
    [GlobalClass]
    public partial class MissionGroup : Resource
    {
        public byte[] RawData { get; set; }

        public void Deserialize(ONBinaryReader reader)
        {
            RawData = reader.ReadBytes(32);

            for (int j = 0; j < RawData.Length; j++)
            {
                Debug.Assert(RawData[j] == 0x00 || RawData[j] == 10, $"Record32 byte {j} should be 0x00 or 0x0A but was 0x{RawData[j]:X2}");
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(RawData ?? new byte[32]);
        }
    }
}

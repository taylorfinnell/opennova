
using Godot;
using OpenNova.Core;
using System.IO;

namespace OpenNova.Mission
{
    [GlobalClass]
    public partial class WeaponLoadout : Resource
    {
        public byte[] RawData { get; set; }

        public void Deserialize(ONBinaryReader reader, int length)
        {
            RawData = reader.ReadBytes(length);
        }

        public void Serialize(BinaryWriter writer)
        {
            if (RawData != null)
                writer.Write(RawData);
        }
    }
}

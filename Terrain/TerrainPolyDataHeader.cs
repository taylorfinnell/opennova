using Godot;
using OpenNova.Core;
using System;

namespace OpenNova.Terrain
{
    [Tool]
    [GlobalClass]
    public partial class TerrainPolyDataHeader : Resource
    {
        public int STRUCT_SIZE_OF = 0xA0;
        [Export] public string Magic { get; private set; }
        [Export] public int NumChildrenPerNode { get; private set; }
        [Export] public int NumLodLevels { get; private set; }
        [Export] public int InitialSize { get; private set; }
        [Export] public int MinNodeSize { get; private set; }

        public void Deserialize(BitReader reader)
        {
            byte[] magicBytes = reader.ReadBytes(4);
            Magic = System.Text.Encoding.ASCII.GetString(magicBytes);

            InitialSize = reader.ReadInt16();
            NumLodLevels = reader.ReadInt16();

            MinNodeSize = InitialSize / (int)Math.Pow(2, NumLodLevels - 1);

            reader.ReadBytes(STRUCT_SIZE_OF - 8);
        }

        public int GetTotalNodeCount()
        {
            return (int)((Math.Pow(4, NumLodLevels) - 1) / 3);
        }
    }
}


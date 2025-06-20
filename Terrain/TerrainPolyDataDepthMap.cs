using Godot;
using OpenNova.Core;

namespace OpenNova.Terrain
{
    [Tool]
    [GlobalClass]
    public partial class TerrainPolyDataDepthMap : Resource
    {
        [Export] public Godot.Collections.Array<short> DepthMap;

        public TerrainPolyDataDepthMap()
        {
        }

        public void Init(int w, int h)
        {
            DepthMap = new Godot.Collections.Array<short>();
            for (int i = 0; i < w * h; i++) { DepthMap.Add(0); }
        }

        public void Deserialize(BitReader reader)
        {
            var x = reader.ReadBits(16);
            var y = reader.ReadBits(16);

            int idx = 0;
            for (int i = 0; i < x; i++)
            {
                int v4 = reader.ReadBits(4);
                int v5 = reader.ReadBits(16);

                for (int j = 0; j < y; j++)
                {
                    int v = reader.ReadBits(v4);
                    DepthMap[idx] = (short)(v + v5);
                    idx++;
                }
            }
        }
    }
}


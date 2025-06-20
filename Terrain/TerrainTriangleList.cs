using Godot;

namespace OpenNova.Terrain
{
    [Tool]
    [GlobalClass]
    public partial class TerrainTriangleList : Resource
    {
        [Export] public int NumVertices;
        [Export] public Godot.Collections.Array<int> Indices;
        [Export] public int NumIndices;
    }
}


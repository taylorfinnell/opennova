using Godot;

namespace OpenNova.Terrain
{
    [Tool]
    [GlobalClass]
    public partial class TerrainVertex : Resource
    {
        [Export] public float X;
        [Export] public float Y;
        [Export] public float Z;
    }
}


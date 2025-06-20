using Godot;

namespace OpenNova.Terrain
{
    [Tool]
    [GlobalClass]
    public partial class QuadTreeNode : Resource
    {
        [Export] public int SectorX { get; set; }
        [Export] public int SectorY { get; set; }
        [Export] public int Size { get; set; }
        [Export] public Godot.Collections.Array<TerrainVertex> Vertices { get; set; }
        [Export] public Godot.Collections.Array<TerrainTriangleList> Strips { get; set; }
        [Export] public QuadTreeNode Parent { get; set; }
        [Export] public Godot.Collections.Array<QuadTreeNode> Children { get; set; }
        [Export] public int Level { get; set; }
        [Export] public bool HasChildren { get; set; }

        public QuadTreeNode()
        {
        }

        public void Init(int sectorX, int sectorY, int size, int level = 0)
        {
            Vertices = new Godot.Collections.Array<TerrainVertex>();
            Strips = new Godot.Collections.Array<TerrainTriangleList>();
            Children = new Godot.Collections.Array<QuadTreeNode>();
            SectorX = sectorX;
            SectorY = sectorY;
            Size = size;
            Level = level;
            for (int i = 0; i < 4; i++)
                Children.Add(null);
        }
    }
}

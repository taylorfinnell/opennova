using Godot;
using OpenNova.Terrain;

namespace OpenNova.Definition
{
    [Tool]
    [GlobalClass]
    public partial class TerrainDefinition : Resource
    {
        [Export] public string Name { get; set; }
        [Export] public string TerrainName { get; set; }
        [Export] public string TerrainCreator { get; set; }
        [Export] public float WaterHeight { get; set; }
        [Export] public int DetailDensity { get; set; }
        [Export] public int DetailDensity2 { get; set; }
        [Export] public int SectorCount { get; set; }
        [Export] public bool WrapX { get; set; }
        [Export] public bool WrapY { get; set; }
        [Export] public Vector2I Origin { get; set; }
        [Export] public Texture2D PolytrnColormap { get; set; }
        [Export] public Texture2D PolytrnDetailmap { get; set; }
        [Export] public Texture2D PolytrnDetailmapC1 { get; set; }
        [Export] public Texture2D PolytrnDetailmapC2 { get; set; }
        [Export] public Texture2D PolytrnDetailmapC3 { get; set; }
        [Export] public Texture2D PolytrnDetailmap2 { get; set; }
        [Export] public Texture2D PolytrnDetailmapdist { get; set; }
        [Export] public TerrainPolyDataFile TerrainPolyDataFile { get; set; }
        [Export] public Texture2D PolytrnTilestrip { get; set; }
        [Export] public Texture2D PolytrnCharmap { get; set; }
        [Export] public Texture2D PolytrnFoliagemap { get; set; }
        [Export] public Texture2D PolytrnDetailblendmap { get; set; }
        [Export] public int[] WaterRgb { get; set; }
        [Export] public float WaterMurk { get; set; }
        [Export] public Vector2I LockTopLeft { get; set; }
        [Export] public Vector2I LockTopRight { get; set; }
        [Export] public Vector2I LockBottomLeft { get; set; }
        [Export] public Vector2I LockBottomRight { get; set; }
        [Export] public int[] Sectors { get; set; }
        [Export] public int SectorsWidth { get; set; }
        [Export] public int SectorsHeight { get; set; }
        [Export] public Godot.Collections.Array<FoliageDef> Foliages { get; set; }

        public void Init()
        {
            Foliages = new Godot.Collections.Array<FoliageDef>();
            WaterRgb = new int[3];
        }
    }
}
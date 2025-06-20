using Godot;

[Tool]
[GlobalClass]
public partial class TerrainHeightDisplay : Label
{
    [Export] public TerrainLoader TerrainLoader { get; set; }

    public override void _Ready()
    {
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 0;
        AnchorBottom = 0;
        OffsetLeft = 10;
        OffsetTop = 40;
        OffsetRight = 200;
        OffsetBottom = 60;

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = Colors.Transparent;
        AddThemeStyleboxOverride("normal", styleBox);
        AddThemeColorOverride("font_color", Colors.White);
        AddThemeColorOverride("font_shadow_color", Colors.Black);
        AddThemeConstantOverride("shadow_offset_x", 1);
        AddThemeConstantOverride("shadow_offset_y", 1);
    }

    public override void _ExitTree()
    {
        RemoveThemeStyleboxOverride("normal");
        RemoveThemeColorOverride("font_color");
        RemoveThemeColorOverride("font_shadow_color");
        RemoveThemeConstantOverride("shadow_offset_x");
        RemoveThemeConstantOverride("shadow_offset_y");
        TerrainLoader = null;
    }

    public override void _Process(double delta)
    {
        Vector3 cameraPos = GetCameraPosition();
        float terrainHeight = TerrainLoader?.GetHeight(cameraPos.X, cameraPos.Z) ?? 0f;
        int sectorIndex = TerrainLoader?.GetSectorIndex(cameraPos.X, cameraPos.Z) ?? 0;

        Text = $"Pos: ({cameraPos.X:F1}, {cameraPos.Y:F1}, {cameraPos.Z:F1})\nTerrain: {terrainHeight:F1}\nSector: {sectorIndex}";
    }

    private Vector3 GetCameraPosition()
    {
        if (Engine.IsEditorHint())
        {
            var viewport = GetViewport();
            if (viewport?.GetCamera3D() != null)
            {
                return viewport.GetCamera3D().GlobalPosition;
            }
        }
        else
        {
            var viewport = GetViewport();
            if (viewport?.GetCamera3D() != null)
            {
                return viewport.GetCamera3D().GlobalPosition;
            }
        }

        return Vector3.Zero;
    }
}
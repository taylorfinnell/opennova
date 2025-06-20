using Godot;

[Tool]
[GlobalClass]
public partial class FPSDisplay : Label
{
    public override void _Ready()
    {
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 0;
        AnchorBottom = 0;
        OffsetLeft = 10;
        OffsetTop = 10;
        OffsetRight = 100;
        OffsetBottom = 30;

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
    }

    public override void _Process(double delta)
    {
        Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
}

using Godot;

[Tool]
[GlobalClass]
public partial class Camera : Camera3D
{
    [Export] public float WalkSpeed { get; set; } = 10.0f;
    [Export] public float SprintSpeed { get; set; } = 30.0f;
    [Export] public float MouseSensitivity { get; set; } = 0.001f;
    [Export] public TerrainLoader TerrainLoader { get; set; }
    [Export] public float EyeHeight { get; set; } = 1.8f;

    private bool _mouseCaptured = false;

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            SetProcessMode(ProcessModeEnum.Always);
        }
    }

    public override void _ExitTree()
    {
        if (!Engine.IsEditorHint() && _mouseCaptured)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _mouseCaptured = false;
        }
        TerrainLoader = null;
    }

    public override void _Input(InputEvent @event)
    {
        if (Engine.IsEditorHint()) return;

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (mouseButton.Pressed)
                {
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                    _mouseCaptured = true;
                }
                else
                {
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                    _mouseCaptured = false;
                }
            }
        }

        if (@event is InputEventMouseMotion mouseMotion && _mouseCaptured)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            RotateObjectLocal(Vector3.Right, -mouseMotion.Relative.Y * MouseSensitivity);

            Vector3 rotation = RotationDegrees;
            rotation.X = Mathf.Clamp(rotation.X, -90, 90);
            RotationDegrees = rotation;
        }
    }

    // TODO: Input mappings...
    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || !_mouseCaptured) return;

        Vector3 velocity = Vector3.Zero;
        float currentSpeed = WalkSpeed;

        if (Input.IsKeyPressed(Key.Shift))
        {
            currentSpeed = SprintSpeed;
        }

        if (Input.IsKeyPressed(Key.W))
        {
            velocity -= Transform.Basis.Z;
        }
        if (Input.IsKeyPressed(Key.S))
        {
            velocity += Transform.Basis.Z;
        }
        if (Input.IsKeyPressed(Key.A))
        {
            velocity -= Transform.Basis.X;
        }
        if (Input.IsKeyPressed(Key.D))
        {
            velocity += Transform.Basis.X;
        }

        if (Input.IsKeyPressed(Key.Space))
        {
            velocity += Vector3.Up;
        }
        if (Input.IsKeyPressed(Key.Ctrl))
        {
            velocity -= Vector3.Up;
        }

        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * currentSpeed;
            GlobalPosition += velocity * (float)delta;
        }

        if (TerrainLoader != null)
        {
            float terrainHeight = TerrainLoader.GetHeight(GlobalPosition.X, GlobalPosition.Z);
            Vector3 position = GlobalPosition;
            position.Y = terrainHeight + EyeHeight;
            GlobalPosition = position;
        }
    }
}

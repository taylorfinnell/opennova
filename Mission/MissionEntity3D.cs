using Godot;
using OpenNova.Mission;

[Tool]
[GlobalClass]
public partial class MissionEntity3D : Node3D
{
    [Export]
    public MissionEntity AssociatedEntity { get; set; }

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            SetProcess(false);
        }

        if (AssociatedEntity != null)
        {
            SyncFromEntity();
        }
    }

    public override void _ExitTree()
    {
        AssociatedEntity = null;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() && AssociatedEntity != null)
        {
            SyncToEntity();
        }
    }

    public MissionEntity GetEntity()
    {
        return AssociatedEntity;
    }

    private void SyncFromEntity()
    {
        if (AssociatedEntity == null) return;

        Transform = AssociatedEntity.GetWorldTransform();
    }

    private void SyncToEntity()
    {
        if (AssociatedEntity == null) return;

        var correctionMatrix = Transform3D.Identity.Rotated(Vector3.Right, Mathf.Pi / 2);
        var rawPosition = correctionMatrix * Position;

        AssociatedEntity.X = rawPosition.X;
        AssociatedEntity.Y = rawPosition.Y;
        AssociatedEntity.Z = rawPosition.Z;

        var euler = Quaternion.GetEuler();

        float pitch = -Mathf.RadToDeg(euler.X);
        float yaw = -(Mathf.RadToDeg(euler.Y) - 180f);
        float roll = Mathf.RadToDeg(euler.Z);

        AssociatedEntity.Pitch = (short)pitch;
        AssociatedEntity.Yaw = (short)yaw;
        AssociatedEntity.Roll = (short)roll;
    }
}

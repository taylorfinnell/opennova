using Godot;
using OpenNova.Mission;


[Tool]
[GlobalClass]
public partial class MissionLoader : Node3D
{
    [Export] public MissionFile Mission { get; set; }
    [Export] public bool LoadOnReady { get; set; } = true;

    public override void _Ready()
    {
        if (LoadOnReady && Mission != null)
        {
            LoadMission();
        }
        else if (!Engine.IsEditorHint() && Mission != null)
        {
            LoadMission();
        }
    }

    public override void _ExitTree()
    {
        ClearMissionEntities();
        Mission = null;
    }

    public void LoadMission()
    {
        if (Mission == null)
        {
            GD.PrintErr("No mission assigned to load");
            return;
        }

        ClearMissionEntities();

        CreateEntityCollection(Mission.Items, Colors.Blue, "Item");
        CreateEntityCollection(Mission.Decorations, Colors.Brown, "Building");
        CreateEntityCollection(Mission.Markers, Colors.Yellow, "Marker");
        CreateEntityCollection(Mission.Organics, Colors.Red, "Organic");

        GD.Print($"Loaded mission: {Mission.Header.MissionName}");
        GD.Print($"Items: {Mission.Items.Count}, Buildings: {Mission.Decorations.Count}, Markers: {Mission.Markers.Count}, Organics: {Mission.Organics.Count}");
    }

    public void LoadMission(MissionFile mission)
    {
        Mission = mission;
        LoadMission();
    }

    private void ClearMissionEntities()
    {
        var existingEntities = FindChildren("*", "MissionEntity3D", true, false);
        foreach (MissionEntity3D entity in existingEntities)
        {
            RemoveChild(entity);
            entity.QueueFree();
        }
    }

    private void CreateEntityCollection(Godot.Collections.Array<MissionEntity> entities, Color color, string type)
    {
        foreach (var entity in entities)
        {
            string graphicName = string.Empty;

            var entity3D = new MissionEntity3D();
            entity3D.AssociatedEntity = entity;

            var itemRegistry = ItemRegistry.Instance;
            if (itemRegistry != null)
            {
                var definition = itemRegistry.GetItemByInternalId(entity.TypeId);

                if (definition != null && definition.Graphic != null)
                {
                    entity3D.AddChild(definition.Graphic.Instantiate<Node3D>());
                }
            }

            AddChild(entity3D);


            if (Engine.IsEditorHint())
            {
                var sceneRoot = GetTree().EditedSceneRoot;
                if (sceneRoot != null)
                {
                    entity3D.Owner = sceneRoot;
                }
                else if (GetOwner() != null)
                {
                    entity3D.Owner = GetOwner();
                }
            }
        }
    }
}

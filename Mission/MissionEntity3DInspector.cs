#if TOOLS
using Godot;

[Tool]
public partial class MissionEntity3DInspector : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object)
    {
        return @object is MissionEntity3D;
    }

    public override void _ParseBegin(GodotObject @object)
    {
        if (@object is MissionEntity3D entity3D)
        {
            var editEntityBtn = new Button();
            editEntityBtn.Text = "Edit Mission Entity Resource";
            editEntityBtn.Pressed += () => OnEditEntityPressed(entity3D);
            AddCustomControl(editEntityBtn);

            var editItemBtn = new Button();
            editItemBtn.Text = "Edit Item Definition Resource";
            editItemBtn.Pressed += () => OnEditItemDefinitionPressed(entity3D);
            AddCustomControl(editItemBtn);
        }
    }

    private void OnEditItemDefinitionPressed(MissionEntity3D entity3D)
    {
        var entity = entity3D.GetEntity();
        if (entity == null)
        {
            GD.PrintErr("No associated MissionEntity found");
            return;
        }

        var itemRegistry = ItemRegistry.Instance;
        if (itemRegistry == null)
        {
            GD.PrintErr("ItemRegistry not available");
            return;
        }

        var itemDefinition = itemRegistry.GetItemByInternalId(entity.TypeId);
        if (itemDefinition != null)
        {
            EditorInterface.Singleton.EditResource(itemDefinition);
            GD.Print($"Opened ItemDefinition resource for editing: {itemDefinition.Name} (ID: {entity.TypeId})");
        }
        else
        {
            GD.PrintErr($"No ItemDefinition found for TypeId: {entity.TypeId}");
        }
    }

    private void OnEditEntityPressed(MissionEntity3D entity3D)
    {
        var entity = entity3D.GetEntity();
        if (entity != null)
        {
            EditorInterface.Singleton.EditResource(entity);
            GD.Print($"Opened MissionEntity resource for editing: {entity.Id}");
        }
        else
        {
            GD.PrintErr("No associated MissionEntity found");
        }
    }
}
#endif
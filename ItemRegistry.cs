using Godot;
using OpenNova.Definition;

[Tool]
[GlobalClass]
public partial class ItemRegistry : Node
{
    private static ItemRegistry _instance;
    private ItemCollectionResource _itemCollection;

    public static ItemRegistry Instance => _instance;

    public override void _EnterTree()
    {
        if (_instance != null)
        {
            this.QueueFree();
        }
        _instance = this;
        LoadItemCollection();
    }

    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
        _itemCollection = null;
    }

    private void LoadItemCollection()
    {
        string itemDefPath = FileUtils.ResolveResourcePath("items.def");

        if (ResourceLoader.Exists(itemDefPath))
        {
            _itemCollection = ResourceLoader.Load<ItemCollectionResource>(itemDefPath);
            if (_itemCollection != null)
            {
                GD.Print($"ItemRegistry: Loaded {_itemCollection.Count} item definitions from {itemDefPath}");
            }
            else
            {
                GD.PrintErr($"ItemRegistry: Failed to load item collection from {itemDefPath}");
            }
        }
        else
        {
            GD.Print($"ItemRegistry: Item collection not found at {itemDefPath}");
        }
    }

    public ItemCollectionResource GetItemCollection()
    {
        return _itemCollection;
    }

    public ItemDefinition GetItemByInternalId(int internalId)
    {
        return _itemCollection?.GetItemByInternalId(internalId);
    }

    public Godot.Collections.Array<ItemDefinition> GetItemsByType(EntityType entityType)
    {
        return _itemCollection?.GetItemsByType(entityType) ?? new Godot.Collections.Array<ItemDefinition>();
    }

    public bool HasItem(int internalId)
    {
        return _itemCollection?.HasItem(internalId) ?? false;
    }
    public int GetItemCount()
    {
        return _itemCollection?.Count ?? 0;
    }
}

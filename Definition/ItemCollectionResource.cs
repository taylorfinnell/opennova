using Godot;
using OpenNova.Definition;
using System.Linq;

[Tool]
[GlobalClass]
public partial class ItemCollectionResource : Resource
{
    [Export] public Godot.Collections.Array<ItemDefinition> Items { get; set; }

    public void Init()
    {
        Items = new Godot.Collections.Array<ItemDefinition>();
    }

    public ItemDefinition GetItemByInternalId(int internalId)
    {
        return Items.FirstOrDefault(item => item.InternalId == internalId);
    }

    public Godot.Collections.Array<ItemDefinition> GetItemsByType(EntityType entityType)
    {
        var result = new Godot.Collections.Array<ItemDefinition>();
        foreach (var item in Items)
        {
            if (item.EntityType == entityType)
            {
                result.Add(item);
            }
        }
        return result;
    }

    public bool HasItem(int id)
    {
        return GetItemByInternalId(id) != null;
    }

    public void AddItem(ItemDefinition item)
    {
        Items.Add(item);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public int Count => Items.Count;
}

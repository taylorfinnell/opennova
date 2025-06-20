using Godot;

namespace OpenNova.Definition
{
    [Tool]
    [GlobalClass]
    public partial class EmplacedWeapon : Resource
    {
        [Export] public string Name { get; set; }
        [Export] public int WeaponId { get; set; }
        [Export] public float[] Position { get; set; }
    }
}

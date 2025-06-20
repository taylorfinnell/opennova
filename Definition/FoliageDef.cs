using Godot;

namespace OpenNova.Definition
{
    [Tool]
    [GlobalClass]
    public partial class FoliageDef : Resource
    {
        [Export] public string Graphic { get; set; } = "";
        [Export] public int ColorLower { get; set; }
        [Export] public int ColorUpper { get; set; }
        [Export] public int Match { get; set; }
        [Export] public Godot.Collections.Array<string> Attrib { get; set; }

        public void Init()
        {
            Attrib = new Godot.Collections.Array<string>();
        }
    }
}
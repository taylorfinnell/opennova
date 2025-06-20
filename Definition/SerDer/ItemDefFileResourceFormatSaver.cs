using Godot;
using System;
using System.Text;

namespace OpenNova.Definition.SerDer
{
    [GlobalClass]
    public partial class ItemDefFileResourceFormatSaver : ResourceFormatSaver
    {
        public override string[] _GetRecognizedExtensions(Resource resource)
        {
            if (resource is ItemCollectionResource)
            {
                return new string[] { "def" };
            }
            return new string[] { };
        }

        public override bool _Recognize(Resource resource)
        {
            return resource is ItemCollectionResource;
        }

        public override Error _Save(Resource resource, string path, uint flags)
        {
            if (!(resource is ItemCollectionResource collection))
            {
                GD.PrintErr("Resource is not an ItemCollectionResource");
                return Error.InvalidParameter;
            }

            try
            {
                GD.Print($"Saving .def file: {path}");

                var sb = new StringBuilder();

                foreach (var item in collection.Items)
                {
                    WriteItemDefinition(sb, item);
                    sb.AppendLine(); // Extra line between items
                }

                // Write to file
                var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"Failed to create .def file: {path}");
                    return Error.FileCorrupt;
                }

                file.StoreString(sb.ToString());
                file.Close();

                GD.Print($"Successfully saved {collection.Count} item definitions to {path}");
                return Error.Ok;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error saving .def file {path}: {ex.Message}");
                return Error.Failed;
            }
        }

        private void WriteItemDefinition(StringBuilder sb, ItemDefinition item)
        {
            sb.AppendLine($"begin \"{item.Name}\"");

            // Essential properties first
            sb.AppendLine($"  id {item.Id}");

            if (item.InternalId != 0)
                sb.AppendLine($"  sid {item.InternalId}");

            if (item.EntityType != EntityType.Unknown)
                sb.AppendLine($"  type {item.EntityType.ToString().ToLower()}");

            if (!string.IsNullOrEmpty(item.SubType))
                sb.AppendLine($"  sub_type {item.SubType}");

            if (item.Graphic != null)
                sb.AppendLine($"  graphic {GetResourceFilename(item.Graphic)}");

            if (!string.IsNullOrEmpty(item.GraphicEnemy))
                sb.AppendLine($"  graphic_enemy {item.GraphicEnemy}");

            if (item.UnitType != 0)
                sb.AppendLine($"  unit_type {item.UnitType}");

            // Combat properties
            if (item.Hp > 0)
                sb.AppendLine($"  hp {item.Hp:F0}");

            if (item.Armor > 0)
                sb.AppendLine($"  armor {item.Armor:F0}");

            if (item.CriticalHp > 0)
                sb.AppendLine($"  criticalhp {item.CriticalHp:F0}");

            if (item.CriticalDrain > 0)
                sb.AppendLine($"  criticaldrain {item.CriticalDrain:F0}");

            if (item.RadarSig > 0)
                sb.AppendLine($"  radarsig {item.RadarSig:F0}");

            if (item.HeatSig > 0)
                sb.AppendLine($"  heatsig {item.HeatSig:F0}");

            if (item.Score > 0)
                sb.AppendLine($"  score {item.Score:F0}");

            if (item.Attributes != EntityAttributes.None)
                sb.AppendLine($"  attrib: {FormatAttributes(item.Attributes)}");

            if (item.Attributes2 != EntityAttributes2.None)
                sb.AppendLine($"  attrib2: {FormatAttributes2(item.Attributes2)}");

            if (!string.IsNullOrEmpty(item.AiFunction))
                sb.AppendLine($"  ai_function {item.AiFunction}");

            if (!string.IsNullOrEmpty(item.RenderFunction))
                sb.AppendLine($"  render_function {item.RenderFunction}");

            if (!string.IsNullOrEmpty(item.MoveFunction))
                sb.AppendLine($"  move_function {item.MoveFunction}");

            if (!string.IsNullOrEmpty(item.DiskFunction))
                sb.AppendLine($"  disk_function {item.DiskFunction}");

            if (item.Scale != 1.0f && item.Scale > 0)
                sb.AppendLine($"  scale {item.Scale:F2}");

            if (!string.IsNullOrEmpty(item.ShadowTexture))
            {
                sb.AppendLine($"  shadow {item.ShadowTexture} {item.ShadowWidth:F1} {item.ShadowHeight:F1} {item.ShadowOffsetX:F2} {item.ShadowOffsetY:F2}");
            }

            if (!string.IsNullOrEmpty(item.PrimaryWeapon))
                sb.AppendLine($"  primary_weapon {item.PrimaryWeapon}");

            if (item.AddEWeap != null)
                WriteEmplacedWeapon(sb, "addeweap", item.AddEWeap);
            if (item.AddEWeapC != null)
                WriteEmplacedWeapon(sb, "addeweapc", item.AddEWeapC);
            if (item.AddEWeapG != null)
                WriteEmplacedWeapon(sb, "addeweapg", item.AddEWeapG);

            if (!string.IsNullOrEmpty(item.SoundProfile))
                sb.AppendLine($"  sound_profile {item.SoundProfile}");

            if (!string.IsNullOrEmpty(item.Husk))
                sb.AppendLine($"  husk {item.Husk}");

            if (!string.IsNullOrEmpty(item.ParticleDeath))
                sb.AppendLine($"  particledeath {item.ParticleDeath}");
            if (!string.IsNullOrEmpty(item.ParticleH2ODeath))
                sb.AppendLine($"  particleh2odeath {item.ParticleH2ODeath}");

            // AI and behavior
            if (!string.IsNullOrEmpty(item.DefaultAip))
                sb.AppendLine($"  default_aip {item.DefaultAip}");

            sb.AppendLine("end");
        }

        private string FormatAttributes(EntityAttributes attributes)
        {
            var parts = new System.Collections.Generic.List<string>();

            foreach (EntityAttributes value in Enum.GetValues<EntityAttributes>())
            {
                if (value != EntityAttributes.None && attributes.HasFlag(value))
                {
                    parts.Add(value.ToString());
                }
            }

            return string.Join(" ", parts);
        }

        private string FormatAttributes2(EntityAttributes2 attributes)
        {
            var parts = new System.Collections.Generic.List<string>();

            foreach (EntityAttributes2 value in Enum.GetValues<EntityAttributes2>())
            {
                if (value != EntityAttributes2.None && attributes.HasFlag(value))
                {
                    parts.Add(value.ToString());
                }
            }

            return string.Join(" ", parts);
        }

        private void WriteEmplacedWeapon(StringBuilder sb, string prefix, EmplacedWeapon weapon)
        {
            if (weapon.Position != null && weapon.Position.Length >= 3)
            {
                sb.AppendLine($"  {prefix} {weapon.Name} {weapon.WeaponId} {weapon.Position[0]:F1} {weapon.Position[1]:F1} {weapon.Position[2]:F1}");
            }
        }

        private static string GetResourceFilename(Resource resource)
        {
            if (resource == null || string.IsNullOrEmpty(resource.ResourcePath))
                return "";

            var path = resource.ResourcePath;
            var filename = System.IO.Path.GetFileName(path);
            return filename;
        }
    }
}

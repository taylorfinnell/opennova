using Godot;
using OpenNova.DefinitionsParsers;

namespace OpenNova.Definition.SerDer
{
    [GlobalClass]
    public partial class TerrainDefFormatLoader : ResourceFormatLoader
    {
        public override string[] _GetRecognizedExtensions()
        {
            return new[] { "trn" };
        }

        public override bool _HandlesType(StringName type)
        {
            return type == "TerrainDefinition" || type == "Resource";
        }

        public override string _GetResourceType(string path)
        {
            if (path.GetExtension().ToLower() == "trn")
                return "TerrainDefinition";
            return "";
        }

        public override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open terrain file: {path}");
                return default;
            }

            byte[] data = file.GetBuffer((long)file.GetLength());
            var parser = new TerrainDefParser();
            var terrainDef = parser.Parse(data);
            terrainDef.ResourcePath = path;

            GD.Print($"Successfully loaded terrain definition: {terrainDef.TerrainName}");
            return terrainDef;
        }
    }
}
using Godot;
using OpenNova.Terrain;

namespace OpenNova.Definition
{
    [GlobalClass]
    public partial class CptResourceFormatLoader : ResourceFormatLoader
    {
        public override string[] _GetRecognizedExtensions()
        {
            return new[] { "cpt" };
        }

        public override bool _HandlesType(StringName type)
        {
            return type == "TerrainPolyDataFile" || type == "Resource";
        }

        public override string _GetResourceType(string path)
        {
            if (path.GetExtension().ToLower() == "cpt")
                return "TerrainPolyDataFile";
            return "";
        }

        public override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

            if (file == null)
            {
                GD.PrintErr($"Failed to open CPT file: {path}");
                return default(Variant);
            }

            byte[] data = file.GetBuffer((long)file.GetLength());
            var terrainPolyData = new TerrainPolyDataFile();
            terrainPolyData.Parse(data);
            return terrainPolyData;
        }
    }
}
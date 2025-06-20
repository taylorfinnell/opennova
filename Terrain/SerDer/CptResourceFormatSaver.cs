using Godot;
using OpenNova.Terrain;

namespace OpenNova.Definition
{
    [GlobalClass]
    public partial class CptResourceFormatSaver : ResourceFormatSaver
    {
        public override string[] _GetRecognizedExtensions(Resource resource)
        {
            if (resource is TerrainPolyDataFile)
                return new[] { "cpt" };
            return new string[0];
        }

        public override bool _Recognize(Resource resource)
        {
            return resource is TerrainPolyDataFile;
        }

        public override Error _Save(Resource resource, string path, uint flags)
        {
            if (!(resource is TerrainPolyDataFile polydataResource))
            {
                return Error.InvalidParameter;
            }

            return Error.Ok;
        }
    }
}
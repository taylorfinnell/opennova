using Godot;
using OpenNova.DefinitionsParsers;

namespace OpenNova.Definition.SerDer
{
    [GlobalClass]
    public partial class ItemDefFileResourceFormatLoader : ResourceFormatLoader
    {
        public override string[] _GetRecognizedExtensions()
        {
            return new string[] { "def" };
        }

        public override bool _HandlesType(StringName type)
        {
            return type == "ItemCollectionResource" || type == "Resource";
        }

        public override string _GetResourceType(string path)
        {
            if (path.GetExtension().ToLower() == "def")
            {
                return "ItemCollectionResource";
            }
            return "";
        }

        public override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
        {
            GD.Print($"Loading .def file: {path}");

            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open .def file: {path}");
                return new Variant();
            }

            byte[] data = file.GetBuffer((long)file.GetLength());
            file.Close();

            var parser = new ItemDefParser();
            var itemDefinitions = parser.Parse(data);

            var collection = new ItemCollectionResource();
            collection.Init();

            foreach (var itemDef in itemDefinitions)
            {
                collection.AddItem(itemDef);
            }

            return collection;
        }
    }
}

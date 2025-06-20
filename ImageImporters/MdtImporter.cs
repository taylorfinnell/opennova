#if TOOLS
using Godot;

[Tool]
public partial class MdtImporter : EditorImportPlugin
{
    public override string _GetImporterName()
    {
        return "novalogic.mdt";
    }

    public override string _GetVisibleName()
    {
        return "Novalogic MDT Texture";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return new string[] { "mdt", "MDT" };
    }

    public override string _GetSaveExtension()
    {
        return "res";
    }

    public override string _GetResourceType()
    {
        return "PortableCompressedTexture2D";
    }

    public override int _GetPresetCount()
    {
        return 1;
    }

    public override string _GetPresetName(int presetIndex)
    {
        return "Default";
    }

    public override float _GetPriority()
    {
        return 2.0f;
    }

    public override int _GetImportOrder()
    {
        return 1; // Higher priority than default importers
    }

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        var options = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        var filter = new Godot.Collections.Dictionary
        {
            { "name", "filter" },
            { "default_value", true },
            { "property_hint", (int)PropertyHint.None }
        };
        options.Add(filter);

        var mipmaps = new Godot.Collections.Dictionary
        {
            { "name", "mipmaps" },
            { "default_value", true },
            { "property_hint", (int)PropertyHint.None }
        };
        options.Add(mipmaps);

        return options;
    }

    public override bool _GetOptionVisibility(string path, StringName optionName, Godot.Collections.Dictionary options)
    {
        return true;
    }

    public override Error _Import(string sourceFile, string savePath, Godot.Collections.Dictionary options,
        Godot.Collections.Array<string> platformVariants, Godot.Collections.Array<string> genFiles)
    {
        GD.Print($"Importing Novalogic MDT Texture: {sourceFile}");

        bool mipmaps = false;

        var image = new Image();
        Error loadError = image.LoadTgaFromBuffer(Godot.FileAccess.GetFileAsBytes(sourceFile));

        if (loadError != Error.Ok)
        {
            GD.PushError($"Failed to load MDT as TGA: {sourceFile}, error: {loadError}");
            return loadError;
        }

        if (mipmaps)
        {
            image.GenerateMipmaps();
        }

        var texture = new PortableCompressedTexture2D();
        texture.CreateFromImage(image, PortableCompressedTexture2D.CompressionMode.Lossless);
        if (texture == null)
        {
            GD.PushError($"Failed to create texture from MDT image: {sourceFile}");
            return Error.Failed;
        }

        string resourceSavePath = $"{savePath}.{_GetSaveExtension()}";
        Error saveError = ResourceSaver.Save(texture, resourceSavePath);

        if (saveError != Error.Ok)
        {
            GD.PushError($"Failed to save MDT texture: {saveError}");
            return saveError;
        }

        GD.Print($"Successfully imported MDT texture: {sourceFile}");
        return Error.Ok;
    }
}
#endif
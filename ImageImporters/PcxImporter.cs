#if TOOLS
using Godot;
using OpenNova.Utils;
using System.Collections.Generic;

[Tool]
public partial class PcxImporter : EditorImportPlugin
{
    public override string _GetImporterName()
    {
        return "novalogic.pcx";
    }

    public override string _GetVisibleName()
    {
        return "Novalogic PCX Texture";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return new string[] { "pcx", "PCX" };
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
        GD.Print($"Importing Novalogic PCX Texture: {sourceFile}");

        try
        {
            bool mipmaps = options.GetValueOrDefault("mipmaps", false).AsBool();

            byte[] pcxData = Godot.FileAccess.GetFileAsBytes(sourceFile);
            if (pcxData == null || pcxData.Length == 0)
            {
                GD.PushError($"Failed to read PCX file: {sourceFile}");
                return Error.FileCorrupt;
            }

            var pcxParser = new PcxParser();
            var parseResult = pcxParser.Parse(pcxData);

            byte[] finalImageData;
            Image.Format format;

            if (parseResult.HasPalette && parseResult.BitsPerPixel == 8)
            {
                finalImageData = ConvertIndexedToRgb(parseResult.ImageData, parseResult.Palette);
                format = Image.Format.Rgb8;
            }
            else
            {
                finalImageData = parseResult.ImageData;
                format = parseResult.BitsPerPixel == 8 ? Image.Format.L8 : Image.Format.Rgb8;
            }

            var image = Image.CreateFromData(parseResult.Width, parseResult.Height, false, format, finalImageData);

            if (mipmaps)
            {
                image.GenerateMipmaps();
            }

            var texture = new PortableCompressedTexture2D();
            texture.CreateFromImage(image, PortableCompressedTexture2D.CompressionMode.Lossless);
            if (texture == null)
            {
                GD.PushError($"Failed to create texture from PCX image: {sourceFile}");
                return Error.Failed;
            }

            string resourceSavePath = $"{savePath}.{_GetSaveExtension()}";
            Error saveError = ResourceSaver.Save(texture, resourceSavePath);

            if (saveError != Error.Ok)
            {
                GD.PushError($"Failed to save PCX texture: {saveError}");
                return saveError;
            }

            GD.Print($"Successfully imported PCX texture: {sourceFile}");
            return Error.Ok;
        }
        catch (System.Exception ex)
        {
            GD.PushError($"Exception during PCX import: {ex.Message}\n{ex.StackTrace}");
            return Error.Failed;
        }
    }

    private byte[] ConvertIndexedToRgb(byte[] indexedData, byte[] palette)
    {
        if (palette == null || palette.Length != 768) // 256 colors * 3 bytes (RGB)
            return indexedData;

        byte[] rgbData = new byte[indexedData.Length * 3];
        int rgbIndex = 0;

        foreach (byte colorIndex in indexedData)
        {
            int paletteOffset = colorIndex * 3;

            if (paletteOffset + 2 < palette.Length)
            {
                rgbData[rgbIndex++] = palette[paletteOffset];     // R
                rgbData[rgbIndex++] = palette[paletteOffset + 1]; // G
                rgbData[rgbIndex++] = palette[paletteOffset + 2]; // B
            }
            else
            {
                rgbData[rgbIndex++] = 0; // R
                rgbData[rgbIndex++] = 0; // G
                rgbData[rgbIndex++] = 0; // B
            }
        }

        return rgbData;
    }
}
#endif
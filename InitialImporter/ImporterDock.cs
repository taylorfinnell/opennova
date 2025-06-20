#if TOOLS
using Godot;
using OpenNova.InitialImporter.PackFile;
using OpenNova.InitialImporter.ScriptFile;
using System.IO;
using System.Linq;

[Tool]
public partial class ImporterDock : Control
{
    private LineEdit _folderPathEdit;
    private Button _browseFolderButton;
    private Button _importButton;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        CreateUI();
    }

    public override void _EnterTree()
    {
        Name = "Novalogic Importer";
    }

    public override void _ExitTree()
    {
        // Disconnect any remaining signal connections
        if (_browseFolderButton != null)
        {
            _browseFolderButton.Pressed -= OnBrowseFolderPressed;
        }

        if (_importButton != null)
        {
            _importButton.Pressed -= OnImportPressed;
        }

        // Clean up UI references
        _folderPathEdit = null;
        _browseFolderButton = null;
        _importButton = null;
    }

    private void CreateUI()
    {
        var vbox = new VBoxContainer();
        AddChild(vbox);

        // Source folder selection
        var folderHBox = new HBoxContainer();
        vbox.AddChild(folderHBox);

        _folderPathEdit = new LineEdit();
        _folderPathEdit.PlaceholderText = "Select source folder...";
        _folderPathEdit.Editable = false;
        _folderPathEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        folderHBox.AddChild(_folderPathEdit);

        _browseFolderButton = new Button();
        _browseFolderButton.Text = "Browse";
        _browseFolderButton.Pressed += OnBrowseFolderPressed;
        folderHBox.AddChild(_browseFolderButton);

        // Import button
        _importButton = new Button();
        _importButton.Text = "Import";
        _importButton.Disabled = true;
        _importButton.Pressed += OnImportPressed;
        vbox.AddChild(_importButton);
    }

    private void OnBrowseFolderPressed()
    {
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenDir;
        fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        fileDialog.CurrentDir = OS.GetSystemDir(OS.SystemDir.Desktop);

        GetViewport().AddChild(fileDialog);
        fileDialog.PopupCentered(new Vector2I(800, 600));

        fileDialog.DirSelected += OnFolderSelected;
        fileDialog.CloseRequested += () => fileDialog.QueueFree();
    }

    private void OnFolderSelected(string path)
    {
        _folderPathEdit.Text = path;
        _importButton.Disabled = false;
    }

    private void OnImportPressed()
    {
        var selectedPath = _folderPathEdit.Text;
        if (string.IsNullOrEmpty(selectedPath))
        {
            GD.PrintErr("No folder selected");
            return;
        }

        GD.Print($"Starting import from: {selectedPath}");

        try
        {
            ImportFromDirectory(selectedPath);
            GD.Print("Import completed successfully!");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Import failed: {ex.Message}");
        }
    }

    private void ImportFromDirectory(string sourcePath)
    {
        // Find all .pff files in the directory
        var pffFiles = Directory.GetFiles(sourcePath, "*.pff", SearchOption.AllDirectories);

        if (pffFiles.Length == 0)
        {
            GD.Print("No .pff files found in the selected directory");
            return;
        }

        GD.Print($"Found {pffFiles.Length} .pff files");

        int totalExtracted = 0;
        foreach (var pffPath in pffFiles)
        {
            if(!pffPath.ToLower().Contains("resource") && !pffPath.ToLower().Contains("localres"))
            {
                continue;
            }
            GD.Print($"Processing: {Path.GetFileName(pffPath)}");
            totalExtracted += ExtractFromPackFile(pffPath);
        }

        GD.Print($"Extracted {totalExtracted} files total");
    }

    private int ExtractFromPackFile(string pffPath)
    {
        int extractedCount = 0;

        try
        {
            using var packFile = new PackFile(pffPath);
            packFile.Load();

            var entries = packFile.GetEntries().ToArray();
            GD.Print($"  Found {entries.Length} entries in {Path.GetFileName(pffPath)}");

            foreach (var entry in entries)
            {
                if (entry.IsDeleted())
                    continue;

                var fileName = entry.GetFileName();
                var extension = entry.GetFileExtension().ToLowerInvariant();

                var supportedExtensions = new[] { ".tga", ".mdt", ".pcx", ".dds", ".3di", ".cpt", ".trn", ".def", ".bms" };

                if (!supportedExtensions.Contains(extension))
                    continue;

                try
                {
                    var data = entry.GetDecodedBytes();
                    if (entry.IsScript())
                    {
                        data = ScriptFile.Decrypt(data, NovalogicGame.JO_DFX_DFX2);
                    }

                    string outputPath = GetOutputPath(fileName, extension);

                    using var fileAccess = Godot.FileAccess.Open(outputPath, Godot.FileAccess.ModeFlags.Write);
                    if (fileAccess != null)
                    {
                        fileAccess.StoreBuffer(data);
                        fileAccess.Close();

                        GD.Print($"    Extracted: {fileName} -> {outputPath}");
                        extractedCount++;
                    }
                    else
                    {
                        GD.PrintErr($"    Failed to create file: {outputPath}");
                    }
                }
                catch (System.Exception ex)
                {
                    GD.PrintErr($"    Failed to extract {fileName}: {ex.Message}");
                }
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to process {Path.GetFileName(pffPath)}: {ex.Message}");
        }

        return extractedCount;
    }

    private string GetOutputPath(string fileName, string extension)
    {
        return extension switch
        {
            ".tga" or ".mdt" or ".pcx" or ".dds" => $"res://assets/textures/{fileName}",
            _ => $"res://assets/{fileName}"
        };
    }
}
#endif

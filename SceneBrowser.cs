using Godot;
using System.Collections.Generic;
using System.IO;

[Tool]
public partial class SceneBrowser : Control
{
    private ItemList sceneList;
    private List<string> scenePaths = new List<string>();

    public override void _Ready()
    {
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 1;
        AnchorBottom = 1;

        var vbox = new VBoxContainer();
        AddChild(vbox);
        vbox.AnchorLeft = 0;
        vbox.AnchorTop = 0;
        vbox.AnchorRight = 1;
        vbox.AnchorBottom = 1;

        var titleLabel = new Label();
        titleLabel.Text = "Mission Scenes";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(titleLabel);

        sceneList = new ItemList();
        sceneList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(sceneList);

        sceneList.ItemActivated += OnSceneDoubleClicked;

        PopulateSceneList();
    }

    public override void _ExitTree()
    {
        if (sceneList != null)
        {
            sceneList.ItemActivated -= OnSceneDoubleClicked;
            sceneList = null;
        }
        scenePaths?.Clear();
        scenePaths = null;
    }

    private void PopulateSceneList()
    {
        scenePaths.Clear();
        sceneList.Clear();


        if (Engine.IsEditorHint())
        {
            PopulateSceneListEditor();
        }
        else
        {
            PopulateSceneListExported();
        }
    }

    private void PopulateSceneListEditor()
    {
        string scenesDir = "res://MissionScenes/";

        var dir = DirAccess.Open(scenesDir);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.EndsWith(".tscn"))
                {
                    string fullPath = scenesDir + fileName;
                    string displayName = Path.GetFileNameWithoutExtension(fileName);

                    sceneList.AddItem(displayName);
                    scenePaths.Add(fullPath);
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        else
        {
            GD.PushError("Could not access MissionScenes directory");
        }
    }

    private void PopulateSceneListExported()
    {
        var missionScenes = new Dictionary<string, string>
        {
            { "00TRg", "res://MissionScenes/00TRg.tscn" },
            { "00TRi", "res://MissionScenes/00TRi.tscn" },
            { "03TR", "res://MissionScenes/03TR.tscn" }
        };

        foreach (var kvp in missionScenes)
        {
            if (ResourceLoader.Exists(kvp.Value))
            {
                sceneList.AddItem(kvp.Key);
                scenePaths.Add(kvp.Value);
            }
            else
            {
                GD.Print($"Mission scene not found: {kvp.Value}");
            }
        }
    }

    private void OnSceneDoubleClicked(long index)
    {
        if (index >= 0 && index < scenePaths.Count)
        {
            string scenePath = scenePaths[(int)index];
            GD.Print($"Loading scene: {scenePath}");

            var packedScene = ResourceLoader.Load<PackedScene>(scenePath);
            if (packedScene != null)
            {
                GetTree().ChangeSceneToPacked(packedScene);
            }
            else
            {
                GD.PushError($"Failed to load scene: {scenePath}");
            }
        }
    }
}

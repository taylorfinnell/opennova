#if TOOLS
using Godot;
using OpenNova.Definition;
using OpenNova.Definition.SerDer;

[Tool]
public partial class OpenNovaPlugin : EditorPlugin
{
    private MissionResourceFormatLoader missionLoader;
    private MissionResourceFormatSaver missionSaver;
    private ItemDefFileResourceFormatLoader defLoader;
    private ItemDefFileResourceFormatSaver defSaver;
    private TerrainDefFormatLoader terrainLoader;
    private TerrainDefFormatSaver terrainSaver;
    private CptResourceFormatLoader cptLoader;
    private CptResourceFormatSaver cptSaver;
    private MissionEntity3DInspector inspectorPlugin;
    private ModelImporter modelImporter;
    private MdtImporter mdtImporter;
    private PcxImporter pcxImporter;
    private ImporterDock importerDock;

    public override void _EnterTree()
    {
        GD.Print("Loaded OpenNova");

        missionLoader = new MissionResourceFormatLoader();
        missionSaver = new MissionResourceFormatSaver();

        ResourceLoader.AddResourceFormatLoader(missionLoader);
        ResourceSaver.AddResourceFormatSaver(missionSaver);

        defLoader = new ItemDefFileResourceFormatLoader();
        defSaver = new ItemDefFileResourceFormatSaver();

        ResourceLoader.AddResourceFormatLoader(defLoader);
        ResourceSaver.AddResourceFormatSaver(defSaver);

        terrainLoader = new TerrainDefFormatLoader();
        terrainSaver = new TerrainDefFormatSaver();

        ResourceLoader.AddResourceFormatLoader(terrainLoader);
        ResourceSaver.AddResourceFormatSaver(terrainSaver);

        cptLoader = new CptResourceFormatLoader();
        cptSaver = new CptResourceFormatSaver();

        ResourceLoader.AddResourceFormatLoader(cptLoader);
        ResourceSaver.AddResourceFormatSaver(cptSaver);

        inspectorPlugin = new MissionEntity3DInspector();
        AddInspectorPlugin(inspectorPlugin);

        modelImporter = new ModelImporter();
        AddImportPlugin(modelImporter);

        mdtImporter = new MdtImporter();
        AddImportPlugin(mdtImporter);

        pcxImporter = new PcxImporter();
        AddImportPlugin(pcxImporter);

        importerDock = new ImporterDock();
        importerDock.Name = "Novalogic Importer";
        AddControlToDock(DockSlot.LeftUr, importerDock);
    }

    public override void _ExitTree()
    {
        if (inspectorPlugin != null)
        {
            RemoveInspectorPlugin(inspectorPlugin);
            inspectorPlugin = null;
        }

        if (missionLoader != null)
        {
            ResourceLoader.RemoveResourceFormatLoader(missionLoader);
            missionLoader = null;
        }

        if (missionSaver != null)
        {
            ResourceSaver.RemoveResourceFormatSaver(missionSaver);
            missionSaver = null;
        }

        if (defLoader != null)
        {
            ResourceLoader.RemoveResourceFormatLoader(defLoader);
            defLoader = null;
        }

        if (defSaver != null)
        {
            ResourceSaver.RemoveResourceFormatSaver(defSaver);
            defSaver = null;
        }

        if (terrainLoader != null)
        {
            ResourceLoader.RemoveResourceFormatLoader(terrainLoader);
            terrainLoader = null;
        }

        if (terrainSaver != null)
        {
            ResourceSaver.RemoveResourceFormatSaver(terrainSaver);
            terrainSaver = null;
        }

        if (cptLoader != null)
        {
            ResourceLoader.RemoveResourceFormatLoader(cptLoader);
            cptLoader = null;
        }

        if (cptSaver != null)
        {
            ResourceSaver.RemoveResourceFormatSaver(cptSaver);
            cptSaver = null;
        }

        if (modelImporter != null)
        {
            RemoveImportPlugin(modelImporter);
        }

        if (mdtImporter != null)
        {
            RemoveImportPlugin(mdtImporter);
            mdtImporter = null;
        }

        if (pcxImporter != null)
        {
            RemoveImportPlugin(pcxImporter);
            pcxImporter = null;
        }

        if (importerDock != null)
        {
            RemoveControlFromDocks(importerDock);
            importerDock = null;
        }
    }
}
#endif
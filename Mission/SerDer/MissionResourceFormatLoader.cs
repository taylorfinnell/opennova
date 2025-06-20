using Godot;
using OpenNova.Mission;

[GlobalClass]
public partial class MissionResourceFormatLoader : ResourceFormatLoader
{
    public override string[] _GetRecognizedExtensions()
    {
        return new[] { "bms" };
    }

    public override bool _HandlesType(StringName type)
    {
        return type == "MissionFile" || type == "Resource";
    }

    public override string _GetResourceType(string path)
    {
        if (path.GetExtension().ToLower() == "bms")
            return "MissionFile";
        return "";
    }

    public override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            return default(Variant);
        }


        try
        {
            byte[] data = file.GetBuffer((long)file.GetLength());
            var missionFile = MissionFile.Parse(data);
            missionFile.ResourcePath = path;
            return missionFile;
        }
        catch (System.Exception)
        {
            return default(Variant);
        }
    }
}

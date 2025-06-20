using Godot;
using OpenNova.Mission;

[GlobalClass]
public partial class MissionResourceFormatSaver : ResourceFormatSaver
{
    public override string[] _GetRecognizedExtensions(Resource resource)
    {
        if (resource is MissionFile)
            return new[] { "bms" };
        return new string[0];
    }

    public override bool _Recognize(Resource resource)
    {
        return resource is MissionFile;
    }


    public override Error _Save(Resource resource, string path, uint flags)
    {
        if (resource is not MissionFile missionFile)
            return Error.InvalidParameter;


        byte[] data = missionFile.Serialize();
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null)
            return Error.FileCorrupt;

        file.StoreBuffer(data);
        return Error.Ok;
    }
}

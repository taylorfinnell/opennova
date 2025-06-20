using System.Diagnostics;

public static class FileAccessBridge
{
    public static byte[] ReadAllBytes(string filePath)
    {
        Debug.Assert(filePath.StartsWith("res://") || filePath.StartsWith("user://"));
        {
            using var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                var error = Godot.FileAccess.GetOpenError();
                throw new System.IO.IOException($"Failed to open file: {filePath}, error: {error}");
            }

            ulong length = file.GetLength();
            return file.GetBuffer((long)length);
        }
    }
}
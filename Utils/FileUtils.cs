using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public static class FileUtils
{
    public static string EnsureExtension(string filePath, string extension)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        if (!extension.StartsWith("."))
            extension = "." + extension;

        if (filePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            return filePath;

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string directory = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(directory))
            return fileNameWithoutExtension + extension;
        else
            return Path.Combine(directory, fileNameWithoutExtension + extension).Replace("\\", "/");
    }

    public static string ResolveResourcePath(string resourceName, string baseDir = null, string[] extensions = null)
    {
        if (string.IsNullOrEmpty(resourceName))
            return null;

        GD.Print($"ResolveResourcePath: Looking for '{resourceName}', IsEditor: {Engine.IsEditorHint()}");

        string fileName = Path.GetFileName(resourceName);
        string cleanName = Path.GetFileNameWithoutExtension(fileName);
        string fileExtension = Path.GetExtension(fileName);

        List<string> searchPaths = new List<string>();

        if (!string.IsNullOrEmpty(baseDir))
            searchPaths.Add(baseDir);

        searchPaths.Add("res://assets/");
        searchPaths.Add("res://");
        searchPaths.Add("res://textures/");

        foreach (var path in searchPaths)
        {
            string fullPath = Path.Combine(path, fileName).Replace("\\", "/");
            if (ResourceLoader.Exists(fullPath) || Godot.FileAccess.FileExists(fullPath))
                return fullPath;

            if (!Engine.IsEditorHint())
            {
                string[] variations = {
                    fileName.ToLowerInvariant(),
                    fileName.ToUpperInvariant(),
                    char.ToUpper(fileName[0]) + fileName.Substring(1).ToLowerInvariant()
                };

                foreach (var variation in variations)
                {
                    string varPath = Path.Combine(path, variation).Replace("\\", "/");
                    if (ResourceLoader.Exists(varPath) || Godot.FileAccess.FileExists(varPath))
                        return varPath;
                }
            }
            else
            {
                string foundPath = FindFileWithoutCase(path, fileName);
                if (!string.IsNullOrEmpty(foundPath))
                    return foundPath;
            }
        }

        if (extensions != null && extensions.Length > 0)
        {
            foreach (var path in searchPaths)
            {
                foreach (var extension in extensions)
                {
                    string ext = extension.StartsWith(".") ? extension : "." + extension;
                    string fullPath = Path.Combine(path, cleanName + ext).Replace("\\", "/");

                    if (ResourceLoader.Exists(fullPath) || Godot.FileAccess.FileExists(fullPath))
                        return fullPath;

                    string foundPath = FindFileWithoutCase(path, cleanName + ext);
                    if (!string.IsNullOrEmpty(foundPath))
                        return foundPath;
                }
            }
        }

        return null;
    }

    private static string FindFileWithoutCase(string directory, string fileName)
    {
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            return null;

        string lowerFileName = fileName.ToLowerInvariant();

        try
        {
            // Open the directory
            var dirAccess = Godot.DirAccess.Open(directory);
            if (dirAccess == null)
                return null;

            // List all files
            dirAccess.ListDirBegin();
            string file = dirAccess.GetNext();

            while (!string.IsNullOrEmpty(file))
            {
                // Skip directories
                if (dirAccess.CurrentIsDir())
                {
                    file = dirAccess.GetNext();
                    continue;
                }

                // Case insensitive comparison
                if (file.ToLowerInvariant() == lowerFileName)
                {
                    string fullPath = Path.Combine(directory, file).Replace("\\", "/");
                    dirAccess.ListDirEnd();

                    GD.Print($"Found case-insensitive match: {fullPath} for {fileName}");

                    return fullPath;
                }

                file = dirAccess.GetNext();
            }

            dirAccess.ListDirEnd();
        }
        catch (Exception ex)
        {
            GD.PushError($"Error searching for file: {ex.Message}");
        }

        return null;
    }
}
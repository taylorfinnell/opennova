using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlaceholderMeshUtil : RefCounted
{
    private readonly Dictionary<Color, StandardMaterial3D> _materialCache = new();

    private readonly Dictionary<Vector3, BoxMesh> _meshCache = new();

    public MeshInstance3D CreatePlaceholderMesh(int typeId, Color color)
    {
        Vector3 size = new Vector3(2.0f, 2.0f, 2.0f);

        if (!_meshCache.TryGetValue(size, out var boxMesh))
        {
            boxMesh = new BoxMesh();
            boxMesh.Size = size;
            _meshCache[size] = boxMesh;
        }

        if (!_materialCache.TryGetValue(color, out var material))
        {
            material = new StandardMaterial3D();
            material.AlbedoColor = color;
            _materialCache[color] = material;
        }

        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = boxMesh;
        meshInstance.MaterialOverride = material;
        meshInstance.Name = "PlaceholderMesh";

        return meshInstance;
    }

    public void ClearCache()
    {
        _materialCache.Clear();
        _meshCache.Clear();
    }
}
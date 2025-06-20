using Godot;
using OpenNova.Definition;
using OpenNova.Terrain;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class TerrainLoader : Node3D
{
    [Export] public TerrainDefinition TerrainDefinition { get; set; }
    [Export] public bool LoadOnReady { get; set; } = true;
    [Export] public int DefaultLodLevel { get; set; } = 2;
    [Export] public Shader TerrainShader { get; set; }

    private SectorMeshes _cachedSectorMeshes;
    private ShaderMaterial _cachedMaterial;
    private bool _meshesGenerated = false;

    private struct SectorMeshes
    {
        public ArrayMesh Sector1;
        public ArrayMesh Sector2;
        public ArrayMesh Sector3;
        public ArrayMesh Sector4;
    }

    public override void _Ready()
    {
        if (LoadOnReady && TerrainDefinition != null)
        {
            GenerateTerrain();
        }
        else if (!Engine.IsEditorHint() && TerrainDefinition != null)
        {
            GenerateTerrain();
        }
    }

    public override void _ExitTree()
    {
        ClearTerrain();
        _cachedMaterial = null;
        _cachedSectorMeshes = new SectorMeshes();
        _meshesGenerated = false;
        TerrainDefinition = null;
    }

    public void GenerateTerrain()
    {
        if (TerrainDefinition == null)
        {
            GD.PrintErr("No TerrainDefinition assigned to load");
            return;
        }

        ClearTerrain();

        if (TerrainDefinition.TerrainPolyDataFile == null)
        {
            GD.PrintErr($"No polydata resource assigned to terrain definition");
            return;
        }

        GenerateTerrainMeshes(TerrainDefinition.TerrainPolyDataFile);

        GD.Print($"Generated terrain: {TerrainDefinition.TerrainName}");
    }

    private void ClearTerrain()
    {
        var existingMeshes = FindChildren("*", "MeshInstance3D", true, false);
        foreach (MeshInstance3D mesh in existingMeshes)
        {
            RemoveChild(mesh);
            mesh.QueueFree();
        }

        _meshesGenerated = false;
        _cachedMaterial = null;
        _cachedSectorMeshes = new SectorMeshes();
    }

    private void GenerateTerrainMeshes(TerrainPolyDataFile polydata)
    {
        if (TerrainDefinition.Sectors == null || TerrainDefinition.SectorsWidth == 0 || TerrainDefinition.SectorsHeight == 0)
        {
            GD.PrintErr("No sector data in terrain definition");
            return;
        }

        if (!_meshesGenerated)
        {
            GenerateCachedSectorMeshes(polydata);
            _meshesGenerated = true;
        }

        if (_cachedMaterial == null)
        {
            _cachedMaterial = CreateCachedTerrainMaterial();
        }

        int gridHeight = TerrainDefinition.SectorsHeight;
        int gridWidth = TerrainDefinition.SectorsWidth;

        int sectorsProcessed = 0;
        int meshesCreated = 0;

        for (int gridY = 0; gridY < gridHeight; gridY++)
        {
            for (int gridX = 0; gridX < gridWidth; gridX++)
            {
                int flatIndex = gridY * gridWidth + gridX;
                int sectorIndex = TerrainDefinition.Sectors[flatIndex];

                if (sectorIndex == 0) continue;

                sectorsProcessed++;
                GD.Print($"Instancing sector {sectorIndex} at grid position ({gridX}, {gridY})");

                var meshInstance = CreateSectorMeshInstance(sectorIndex, gridX, gridY);
                if (meshInstance != null)
                {
                    AddChild(meshInstance);
                    meshesCreated++;
                    GD.Print($"Created mesh instance: {meshInstance.Name} at position {meshInstance.Position}");

                    if (Engine.IsEditorHint())
                    {
                        var sceneRoot = GetTree().EditedSceneRoot;
                        if (sceneRoot != null)
                        {
                            meshInstance.Owner = sceneRoot;
                        }
                        else if (GetOwner() != null)
                        {
                            meshInstance.Owner = GetOwner();
                        }
                    }
                }
            }
        }

        GD.Print($"Terrain generation complete: {sectorsProcessed} sectors processed, {meshesCreated} mesh instances created");
    }

    private void GenerateCachedSectorMeshes(TerrainPolyDataFile polydata)
    {
        GD.Print("Generating cached sector meshes...");

        _cachedSectorMeshes.Sector1 = GenerateSectorMeshFromQuadtree(polydata, 1);
        _cachedSectorMeshes.Sector2 = GenerateSectorMeshFromQuadtree(polydata, 2);
        _cachedSectorMeshes.Sector3 = GenerateSectorMeshFromQuadtree(polydata, 3);
        _cachedSectorMeshes.Sector4 = GenerateSectorMeshFromQuadtree(polydata, 4);

        GD.Print("Cached sector meshes generated successfully");
    }

    private ShaderMaterial CreateCachedTerrainMaterial()
    {
        if (TerrainDefinition == null || TerrainShader == null) return null;

        GD.Print("Creating cached terrain material...");

        var material = new ShaderMaterial();
        material.Shader = TerrainShader;

        // Load and set texture uniforms based on TerrainDefinition
        LoadTextureUniform(material, "polytrn_colormap", TerrainDefinition.PolytrnColormap);
        LoadTextureUniform(material, "polytrn_detailmap", TerrainDefinition.PolytrnDetailmap);
        LoadTextureUniform(material, "polytrn_detailmap_c1", TerrainDefinition.PolytrnDetailmapC1);
        LoadTextureUniform(material, "polytrn_detailmap_c2", TerrainDefinition.PolytrnDetailmapC2);
        LoadTextureUniform(material, "polytrn_detailmap_c3", TerrainDefinition.PolytrnDetailmapC3);
        LoadTextureUniform(material, "polytrn_detailmap2", TerrainDefinition.PolytrnDetailmap2);
        LoadTextureUniform(material, "polytrn_detailmapdist", TerrainDefinition.PolytrnDetailmapdist);
        LoadTextureUniform(material, "polytrn_tilestrip", TerrainDefinition.PolytrnTilestrip);
        LoadTextureUniform(material, "polytrn_detailblendmap", TerrainDefinition.PolytrnDetailblendmap);

        // Set water parameters
        material.SetShaderParameter("water_height", TerrainDefinition.WaterHeight);
        material.SetShaderParameter("water_murk", TerrainDefinition.WaterMurk);

        if (TerrainDefinition.WaterRgb != null && TerrainDefinition.WaterRgb.Length >= 3)
        {
            var waterColor = new Vector3(
                TerrainDefinition.WaterRgb[0] / 255f,
                TerrainDefinition.WaterRgb[1] / 255f,
                TerrainDefinition.WaterRgb[2] / 255f
            );
            material.SetShaderParameter("water_color", waterColor);
        }

        // Set terrain parameters
        material.SetShaderParameter("detail_scale1", TerrainDefinition.DetailDensity);
        material.SetShaderParameter("detail_scale2", TerrainDefinition.DetailDensity2);

        GD.Print("Cached terrain material created successfully");
        return material;
    }

    private MeshInstance3D CreateSectorMeshInstance(int sectorIndex, int gridX, int gridY)
    {
        ArrayMesh mesh = sectorIndex switch
        {
            1 => _cachedSectorMeshes.Sector1,
            2 => _cachedSectorMeshes.Sector2,
            3 => _cachedSectorMeshes.Sector3,
            4 => _cachedSectorMeshes.Sector4,
            _ => null
        };

        if (mesh == null)
        {
            GD.PrintErr($"No cached mesh found for sector {sectorIndex}");
            return null;
        }

        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;
        meshInstance.Name = $"TerrainSector_{sectorIndex}_{gridX}_{gridY}";

        if (_cachedMaterial != null)
        {
            // Clone the material so we can set sector-specific parameters
            var sectorMaterial = _cachedMaterial.Duplicate() as ShaderMaterial;
            sectorMaterial.SetShaderParameter("sector_index", sectorIndex);
            meshInstance.SetSurfaceOverrideMaterial(0, sectorMaterial);
        }

        float worldX = (gridX + TerrainDefinition.Origin.X) * TerrainPolyDataFile.SECTOR_SIZE;
        float worldZ = (gridY + TerrainDefinition.Origin.Y) * TerrainPolyDataFile.SECTOR_SIZE;
        meshInstance.Position = new Vector3(worldX, 0, worldZ);

        return meshInstance;
    }

    private ArrayMesh GenerateSectorMeshFromQuadtree(TerrainPolyDataFile polydata, int sectorIndex)
    {
        if (polydata.RootNode == null)
        {
            GD.PrintErr("No quadtree data in polydata");
            return null;
        }

        var lodNodes = polydata.GetNodesAtLod(DefaultLodLevel);
        if (lodNodes.Length == 0)
        {
            GD.PrintErr($"No nodes found at LOD level {DefaultLodLevel}");
            return null;
        }

        var sectorOffset = polydata.GetSectorOffset(sectorIndex);

        var sectorNodes = FilterNodesForSector(lodNodes, sectorOffset, (int)TerrainPolyDataFile.SECTOR_SIZE);

        if (sectorNodes.Length == 0)
        {
            GD.Print($"No geometry found for sector {sectorIndex}");
            return null;
        }

        // Create mesh from quadtree nodes
        var arrayMesh = CreateMeshFromNodes(sectorNodes, sectorOffset);
        if (arrayMesh == null) return null;

        GD.Print($"Generated cached mesh for sector {sectorIndex}");
        return arrayMesh;
    }


    private QuadTreeNode[] FilterNodesForSector(QuadTreeNode[] nodes, Vector2I sectorOffset, int sectorSize)
    {
        var sectorNodes = new List<QuadTreeNode>();

        foreach (var node in nodes)
        {
            // Check if node overlaps with this sector
            bool overlaps = !(node.SectorX >= sectorOffset.X + sectorSize ||
                             node.SectorX + node.Size <= sectorOffset.X ||
                             node.SectorY >= sectorOffset.Y + sectorSize ||
                             node.SectorY + node.Size <= sectorOffset.Y);

            if (overlaps)
            {
                sectorNodes.Add(node);
            }
        }

        return sectorNodes.ToArray();
    }

    private ArrayMesh CreateMeshFromNodes(QuadTreeNode[] nodes, Vector2I sectorOffset)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();

        foreach (var node in nodes)
        {
            if (node.Vertices == null || node.Strips == null) continue;

            int baseVertexIndex = vertices.Count;

            // Add vertices for this node
            foreach (var vertex in node.Vertices)
            {
                // Convert vertex position relative to sector
                float x = vertex.X + node.SectorX - sectorOffset.X;
                float z = vertex.Y + node.SectorY - sectorOffset.Y;
                float y = vertex.Z;

                vertices.Add(new Vector3(x, y, z));

                float u = x / TerrainPolyDataFile.SECTOR_SIZE;
                float v = z / TerrainPolyDataFile.SECTOR_SIZE;
                uvs.Add(new Vector2(u, v));

                normals.Add(Vector3.Up);
            }

            foreach (var strip in node.Strips)
            {
                if (strip.Indices == null || strip.Indices.Count < 3) continue;

                for (int i = 0; i < strip.Indices.Count - 2; i++)
                {
                    int i0 = baseVertexIndex + strip.Indices[i];
                    int i1 = baseVertexIndex + strip.Indices[i + 1];
                    int i2 = baseVertexIndex + strip.Indices[i + 2];

                    // Ensure indices are valid
                    if (i0 >= vertices.Count || i1 >= vertices.Count || i2 >= vertices.Count)
                        continue;

                    // Triangle strip winding order alternates - flipped for Godot
                    if (i % 2 == 0)
                    {
                        indices.Add(i0);
                        indices.Add(i2);
                        indices.Add(i1);
                    }
                    else
                    {
                        indices.Add(i0);
                        indices.Add(i1);
                        indices.Add(i2);
                    }
                }
            }
        }

        if (vertices.Count == 0 || indices.Count == 0)
        {
            GD.PrintErr("No valid geometry generated for sector");
            return null;
        }

        // Create ArrayMesh
        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
        arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        GD.Print($"Created mesh with {vertices.Count} vertices and {indices.Count / 3} triangles");
        return arrayMesh;
    }


    private void LoadTextureUniform(ShaderMaterial material, string uniformName, Texture2D texture)
    {
        if (texture != null)
        {
            material.SetShaderParameter(uniformName, texture);
        }
    }

    public float GetHeight(float worldX, float worldZ)
    {
        if (TerrainDefinition == null || TerrainDefinition.TerrainPolyDataFile == null)
            return 0f;

        var sectors = TerrainDefinition.Sectors;
        if (sectors == null || TerrainDefinition.SectorsWidth == 0 || TerrainDefinition.SectorsHeight == 0)
            return 0f;

        float adjustedX = worldX - (TerrainDefinition.Origin.X * 512);
        float adjustedZ = worldZ - (TerrainDefinition.Origin.Y * 512);

        int sectorX = Mathf.FloorToInt(adjustedX / 512);
        int sectorZ = Mathf.FloorToInt(adjustedZ / 512);

        if (sectorX < 0 || sectorZ < 0 || sectorX >= TerrainDefinition.SectorsWidth || sectorZ >= TerrainDefinition.SectorsHeight)
            return 0f;

        int flatIndex = sectorZ * TerrainDefinition.SectorsWidth + sectorX;
        if (flatIndex >= sectors.Length)
            return 0f;

        int sectorIndex = sectors[flatIndex];
        if (sectorIndex == 0)
            return 0f; // No terrain in this sector

        var sectorOffset = TerrainDefinition.TerrainPolyDataFile.GetSectorOffset(sectorIndex);

        // Local coordinates within the current sector
        float localX = adjustedX % 512f;
        float localZ = adjustedZ % 512f;
        if (localX < 0) localX += 512f;
        if (localZ < 0) localZ += 512f;

        int heightmapX = sectorOffset.X + (int)localX;
        int heightmapZ = sectorOffset.Y + (int)localZ;

        return TerrainDefinition.TerrainPolyDataFile.GetInterpolatedHeightAt(heightmapX, heightmapZ);
    }

    public int GetSectorIndex(float worldX, float worldZ)
    {
        if (TerrainDefinition == null)
            return 0;

        var sectors = TerrainDefinition.Sectors;
        if (sectors == null || TerrainDefinition.SectorsWidth == 0 || TerrainDefinition.SectorsHeight == 0)
            return 0;

        float adjustedX = worldX - (TerrainDefinition.Origin.X * 512);
        float adjustedZ = worldZ - (TerrainDefinition.Origin.Y * 512);

        int sectorX = Mathf.FloorToInt(adjustedX / 512);
        int sectorZ = Mathf.FloorToInt(adjustedZ / 512);

        if (sectorX < 0 || sectorZ < 0 || sectorX >= TerrainDefinition.SectorsWidth || sectorZ >= TerrainDefinition.SectorsHeight)
            return 0;

        int flatIndex = sectorZ * TerrainDefinition.SectorsWidth + sectorX;
        if (flatIndex >= sectors.Length)
            return 0;

        return sectors[flatIndex];
    }
}

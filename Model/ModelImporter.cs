#if TOOLS
using Godot;
using OpenNova.Model;
using OpenNova.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

[Tool]
public partial class ModelImporter : EditorImportPlugin
{
    // Importer settings
    public override string _GetImporterName()
    {
        return "novalogic.model";
    }

    public override string _GetVisibleName()
    {
        return "Novalogic Model";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return new string[] { "3di", "3DI" };
    }

    public override string _GetSaveExtension()
    {
        return "scn";
    }

    public override string _GetResourceType()
    {
        return "PackedScene";
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
        return (int)1;
    }

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        var options = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        // Scale import option
        var scale = new Godot.Collections.Dictionary
        {
            { "name", "scale" },
            { "default_value", 1.0f },
            { "property_hint", (int)PropertyHint.Range },
            { "hint_string", "0.001,100,0.001" }
        };
        options.Add(scale);

        return options;
    }

    public override bool _GetOptionVisibility(string path, StringName optionName, Godot.Collections.Dictionary options)
    {
        return true;
    }

    public override Error _Import(string sourceFile, string savePath, Godot.Collections.Dictionary options,
        Godot.Collections.Array<string> platformVariants, Godot.Collections.Array<string> genFiles)
    {
        try
        {
            GD.Print($"Importing Novalogic Model: {sourceFile}");

            // Read import options
            float scale = options["scale"].AsSingle();

            // Load the model file
            byte[] fileData = FileAccessBridge.ReadAllBytes(sourceFile);
            ModelFile modelFile;

            using (var ms = new MemoryStream(fileData))
            using (var reader = new BinaryReader(ms))
            {
                // Read magic number and version
                string magic = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));
                if (magic != "3DI3")
                    throw new InvalidDataException($"Invalid magic number: {magic}. Expected: 3DI3");

                int version = reader.ReadInt32();
                if (version != 259)
                    throw new InvalidDataException($"Invalid version: {version}. Expected: 259");

                // Read root node
                var rootNode = ModelNode.Read(reader);
                modelFile = new ModelFile(rootNode);
            }

            // Extract model data
            var modelData = ExtractModelData(modelFile);
            if (modelData == null)
            {
                GD.PushError($"Failed to extract model data from: {sourceFile}");
                return Error.Failed;
            }

            // Create hierarchical scene that handles both skinned and non-skinned meshes
            PackedScene scene = CreateModel(modelData, scale);

            if (scene == null)
            {
                GD.PushError($"Failed to create model scene: {sourceFile}");
                return Error.Failed;
            }

            // Save the scene
            string scenePath = $"{savePath}.{_GetSaveExtension()}";
            var saveError = ResourceSaver.Save(scene, scenePath);
            if (saveError != Error.Ok)
            {
                GD.PushError($"Failed to save model scene: {saveError}");
                return saveError;
            }

            return Error.Ok;
        }
        catch (Exception ex)
        {
            GD.PushError($"Exception during model import: {ex.Message}\n{ex.StackTrace}");
            return Error.Failed;
        }
    }

    private void ProcessLights(ModelData modelData, Node3D root, Dictionary<int, Node3D> parts)
    {
        var lights = modelData.Lights;

        if (modelData.Lights.Count == 0)
            return;

        foreach (var light in lights)
        {
            var lightNode = new OmniLight3D
            {
                Name = $"Light",
                LightColor = new Color(light.ColorStart.Z, light.ColorStart.Y, light.ColorStart.X),
                OmniRange = light.AttenEnd,
                OmniAttenuation = light.AttenStart > 0 ? 1.0f - (light.AttenStart / light.AttenEnd) : 1.0f,
                Position = new Vector3(
                    -light.Offset.X,
                    light.Offset.Y,
                    light.Offset.Z
                )
            };

            parts[light.SubObjIndex].AddChild(lightNode);
        }
    }

    private static Vector3 TransformUserPointPosition(UserPoint point)
    {
        var position = new System.Numerics.Vector3(
            point.X / 65536.0f,
            point.Y / 65536.0f,
            point.Z / 65536.0f
        );

        return ConvertCollisionPosition(position);
    }

    private static Vector3 ConvertCollisionPosition(System.Numerics.Vector3 position)
    {
        var transformed = CoordinateTransformer.TransformCollisionPosition(position);
        return new Vector3(transformed.X, transformed.Y, transformed.Z);
    }


    private void ProcessUserPoints(ModelData modelData, Node3D root, Dictionary<int, Node3D> parts)
    {
        var userPoints = modelData.UserPoints;

        foreach (var point in userPoints)
        {
            string pointName = point.Name;
            var mat = System.Numerics.Matrix4x4.Identity;
            if (point.SubObjectIndex >= 0 && point.SubObjectIndex < modelData.RenderObjects.Count)
            {
                mat = System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(
                    -modelData.RenderObjects[point.SubObjectIndex].AbsX,
                    modelData.RenderObjects[point.SubObjectIndex].AbsY,
                    modelData.RenderObjects[point.SubObjectIndex].AbsZ
                    ));
            }
            System.Numerics.Matrix4x4.Invert(mat, out var inv);

            var gdPoint = TransformUserPointPosition(point);
            var snPoint = new System.Numerics.Vector3(
                gdPoint.X,
                gdPoint.Y,
                gdPoint.Z
            );
            var trans = System.Numerics.Vector3.Transform(
                snPoint,
                inv
            );

            // Create point node with local position
            var pointNode = new Node3D
            {
                Name = pointName,
                Position = new Vector3(trans.X, trans.Y, trans.Z)
            };

            // Add to appropriate parent node
            if (point.SubObjectIndex < 0 || point.SubObjectIndex >= parts.Count || !parts.ContainsKey(point.SubObjectIndex))
            {
                // Add to root if invalid index or if parts[0] exists
                if (parts.ContainsKey(0))
                {
                    parts[0].AddChild(pointNode);
                }
                else
                {
                    // Add to root node as fallback
                    root.AddChild(pointNode);
                }
            }
            else
            {
                parts[point.SubObjectIndex].AddChild(pointNode);
            }
        }
    }

    private PackedScene CreateModel(ModelData modelData, float scale)
    {
        try
        {
            // Create root node for the model
            var rootNode = new Node3D();
            rootNode.Name = modelData.ModelName;

            var partNodes = new Dictionary<int, Node3D>();

            for (int i = 0; i < modelData.RenderObjects.Count; i++)
            {
                var renderObject = modelData.RenderObjects[i];
                var partNumberNode = new Node3D
                {
                    Name = "PN" + (i + 1),
                    Position = new Vector3(-renderObject.AbsX, renderObject.AbsY, renderObject.AbsZ)
                };
                partNodes[i] = partNumberNode;

                rootNode.AddChild(partNumberNode);
            }

            // Process components using the part nodes
            ProcessMeshes(modelData, rootNode, partNodes);
            ProcessLights(modelData, rootNode, partNodes);
            ProcessUserPoints(modelData, rootNode, partNodes);
            ProcessBulletCollisions(modelData, rootNode, partNodes);
            ProcessBoundingVolumeCollisions(modelData, rootNode, partNodes);

            foreach (var n in partNodes.Values)
            {
                SetOwnerRecursively(n, rootNode);
            }

            // Create packed scene
            var packedScene = new PackedScene();
            packedScene.Pack(rootNode);

            return packedScene;
        }
        catch (Exception ex)
        {
            GD.PushError($"Error creating hierarchical scene: {ex.Message}");
            return null;
        }
    }

    private void ProcessBulletCollisions(ModelData modelData, Node3D rootNode, Dictionary<int, Node3D> parts)
    {
        if (!modelData.HasCollisionData || modelData.CollisionObjects.Count == 0 ||
            modelData.CollisionVertices.Count == 0 || modelData.CollisionFaces.Count == 0)
            return;

        // Track vertex and face offsets as we process collision objects
        int vertOffset = 0;
        int faceOffset = 0;

        // Track BulletMesh counters for each part
        var bulletMeshCounters = new Dictionary<int, int>();

        // Process each collision object
        for (var i = 0; i < modelData.CollisionObjects.Count; i++)
        {
            var cobj = modelData.CollisionObjects[i];

            // Skip if this object has no faces or vertices
            if (cobj.NumFaces == 0 || cobj.NumVerts == 0)
            {
                vertOffset += (int)cobj.NumVerts;
                faceOffset += (int)cobj.NumFaces;
                continue;
            }

            // Determine parent node using same logic as ProcessUserPoints
            Node3D parentPart;
            int parentIndex = -1;
            if (cobj.ParentSubObjectIndex < 0 || cobj.ParentSubObjectIndex >= parts.Count || !parts.ContainsKey(cobj.ParentSubObjectIndex))
            {
                // Add to parts[0] if it exists, otherwise use root as fallback
                if (parts.ContainsKey(0))
                {
                    parentPart = parts[0];
                    parentIndex = 0;
                }
                else
                {
                    // Use root node as final fallback
                    parentPart = rootNode;
                    parentIndex = -1;
                }
            }
            else
            {
                parentPart = parts[cobj.ParentSubObjectIndex];
                parentIndex = cobj.ParentSubObjectIndex;
            }

            // Calculate inverse transform matrix for the parent (same logic as ProcessUserPoints)
            var mat = System.Numerics.Matrix4x4.Identity;
            if (parentIndex >= 0 && parentIndex < modelData.RenderObjects.Count)
            {
                mat = System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(
                    -modelData.RenderObjects[parentIndex].AbsX,
                    modelData.RenderObjects[parentIndex].AbsY,
                    modelData.RenderObjects[parentIndex].AbsZ
                    ));
            }
            System.Numerics.Matrix4x4.Invert(mat, out var inv);

            // Process the collision vertices for this object 
            var objectVertices = new List<Vector3>();
            for (int v = vertOffset; v < vertOffset + cobj.NumVerts && v < modelData.CollisionVertices.Count; v++)
            {
                // Convert each vertex to Godot coordinates
                var gdVertex = ConvertCollisionPosition(modelData.CollisionVertices[v].Position);

                // Transform to local space relative to parent
                var snVertex = new System.Numerics.Vector3(gdVertex.X, gdVertex.Y, gdVertex.Z);
                var localVertex = System.Numerics.Vector3.Transform(snVertex, inv);

                objectVertices.Add(new Vector3(localVertex.X, localVertex.Y, localVertex.Z));
            }

            // Skip if no vertices
            if (objectVertices.Count == 0)
            {
                vertOffset += (int)cobj.NumVerts;
                faceOffset += (int)cobj.NumFaces;
                continue;
            }

            // Create triangles list for trimesh
            var triangles = new List<Vector3>();

            // Process the faces that use these vertices
            for (int j = faceOffset; j < faceOffset + cobj.NumFaces && j < modelData.CollisionFaces.Count; j++)
            {
                var face = modelData.CollisionFaces[j];

                // The face indices are already relative to this collision object
                int localIdx1 = face.VertIndex1;
                int localIdx2 = face.VertIndex2;
                int localIdx3 = face.VertIndex3;

                // Skip if indices are out of bounds for our local vertex list
                if (localIdx1 < 0 || localIdx1 >= objectVertices.Count ||
                    localIdx2 < 0 || localIdx2 >= objectVertices.Count ||
                    localIdx3 < 0 || localIdx3 >= objectVertices.Count)
                    continue;

                // Get vertices from our local vertex list
                var v1 = objectVertices[localIdx1];
                var v2 = objectVertices[localIdx2];
                var v3 = objectVertices[localIdx3];

                // Add vertices to triangle list in the correct order for Godot ConcavePolygonShape3D
                triangles.Add(v1);
                triangles.Add(v2);
                triangles.Add(v3);
            }

            // Skip if no valid triangles
            if (triangles.Count == 0)
            {
                vertOffset += (int)cobj.NumVerts;
                faceOffset += (int)cobj.NumFaces;
                continue;
            }

            // Increment BulletMesh counter for this part
            if (!bulletMeshCounters.ContainsKey(parentIndex))
            {
                bulletMeshCounters[parentIndex] = 0;
            }
            bulletMeshCounters[parentIndex]++;

            // Create a single static body for this collision object and parent directly to the part node
            var staticBody = new StaticBody3D { Name = $"BulletMesh {bulletMeshCounters[parentIndex]}" };
            parentPart.AddChild(staticBody);

            // Create trimesh shape
            var trimeshShape = new ConcavePolygonShape3D();
            trimeshShape.Data = triangles.ToArray();

            // Create collision shape node
            var collisionShape = new CollisionShape3D
            {
                Shape = trimeshShape,
                Name = "Shape"
            };

            // Add collision shape to static body
            staticBody.AddChild(collisionShape);

            vertOffset += (int)cobj.NumVerts;
            faceOffset += (int)cobj.NumFaces;
        }
    }

    private void ProcessMeshes(ModelData modelData, Node3D rootNode, Dictionary<int, Node3D> parts)
    {
        int stripOffset = 0;
        for (int i = 0; i < modelData.RenderObjects.Count; i++)
        {
            var renderObject = modelData.RenderObjects[i];

            var partNumberNode = parts[i];

            int numStrips = renderObject.NumStrips + renderObject.NumAlphaStrips;

            int endStrip = stripOffset + numStrips;

            var mesh = new ArrayMesh();
            var meshInstance = new MeshInstance3D
            {
                Visible = true,
            };
            meshInstance.Name = $"Mesh";

            for (int stripIndex = stripOffset; stripIndex < endStrip && stripIndex < modelData.Strips.Count; stripIndex++)
            {
                var strip = modelData.Strips[stripIndex];

                if (strip.NumIndices <= 0 || strip.NumTriangles <= 0)
                    continue;

                var surfaceTool = new SurfaceTool();
                surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
                CreateSurfaceFromStrip(surfaceTool, mesh, modelData, strip, i);
                surfaceTool.Commit(mesh);

            }

            // Part node is already added to rootNode in CreateModel
            // Don't add it again here

            if (mesh.GetSurfaceCount() > 0)
            {
                meshInstance.Mesh = mesh;

                partNumberNode.AddChild(meshInstance);
            }

            // Update strip offset
            stripOffset += numStrips;
        }
    }

    private void CreateSurfaceFromStrip(SurfaceTool surfaceTool, ArrayMesh mesh, ModelData modelData, TriangleStrip strip, int subObject)
    {
        try
        {
            // Skip if no indices or triangles
            if (strip.NumIndices < 3 || strip.NumTriangles <= 0)
                return;

            // Get indices for this strip
            int startIndex = strip.IndexOffset;

            // Skip strips with invalid indices
            if (startIndex + strip.NumIndices > modelData.Indices.Count)
            {
                GD.PushError("Invalid strip parameters, skipping mesh creation");
                return;
            }

            // Process triangles as triangle list
            Debug.Assert(strip.IsStrip != 1);
            {
                // Track if we've added any valid triangles
                bool addedTriangles = false;

                // Process as triangle list - every 3 vertices form a triangle
                for (int i = 0; i < strip.NumIndices - 2 && startIndex + i + 2 < modelData.Indices.Count; i += 3)
                {
                    // Get indices for vertices in this triangle
                    ushort a = (ushort)(modelData.Indices[startIndex + i] + strip.StartVertex);
                    ushort b = (ushort)(modelData.Indices[startIndex + i + 1] + strip.StartVertex);
                    ushort c = (ushort)(modelData.Indices[startIndex + i + 2] + strip.StartVertex);

                    // Skip degenerate triangles
                    if (a == b || b == c || c == a)
                        continue;

                    // Skip invalid indices
                    if (a >= modelData.Vertices.Count || b >= modelData.Vertices.Count || c >= modelData.Vertices.Count)
                    {
                        GD.PushError($"Invalid vertex indices: {a}, {b}, {c} out of {modelData.Vertices.Count}, skipping triangle");
                        continue;
                    }

                    // Get vertices
                    var v1 = modelData.Vertices[a];
                    var v2 = modelData.Vertices[b];
                    var v3 = modelData.Vertices[c];

                    var ro = modelData.RenderObjects[subObject];
                    var abs = new System.Numerics.Vector3(ro.AbsX, ro.AbsY, ro.AbsZ);
                    var abs2 = new System.Numerics.Vector3(abs.X, abs.Y, abs.Z);

                    // First vertex
                    var pos1 = TransformPosition(v1.Position - abs2, 1);
                    var norm1 = TransformNormal(v1.Normal);
                    var uv1 = new Vector2(v1.TexCoord0.X, v1.TexCoord0.Y);
                    var uv1_2 = new Vector2(v1.TexCoord1.X, v1.TexCoord1.Y);

                    // Second vertex
                    var pos2 = TransformPosition(v2.Position - abs2, 1);
                    var norm2 = TransformNormal(v2.Normal);
                    var uv2 = new Vector2(v2.TexCoord0.X, v2.TexCoord0.Y);
                    var uv2_2 = new Vector2(v2.TexCoord1.X, v2.TexCoord1.Y);

                    // Third vertex
                    var pos3 = TransformPosition(v3.Position - abs2, 1);
                    var norm3 = TransformNormal(v3.Normal);
                    var uv3 = new Vector2(v3.TexCoord0.X, v3.TexCoord0.Y);
                    var uv3_2 = new Vector2(v3.TexCoord1.X, v3.TexCoord1.Y);

                    // Add skinning data if this is a skinned mesh
                    if (modelData.HasSkinnedMeshes)
                    {
                        // For vertex 1
                        float[] boneWeights1 = new float[4];
                        int[] boneIndices1 = new int[4];
                        ProcessSkinnedVertexData(v1, strip, boneWeights1, boneIndices1);

                        // For vertex 2
                        float[] boneWeights2 = new float[4];
                        int[] boneIndices2 = new int[4];
                        ProcessSkinnedVertexData(v2, strip, boneWeights2, boneIndices2);

                        // For vertex 3
                        float[] boneWeights3 = new float[4];
                        int[] boneIndices3 = new int[4];
                        ProcessSkinnedVertexData(v3, strip, boneWeights3, boneIndices3);

                        // Add vertices with skinning data
                        surfaceTool.SetBones(new int[] {
                            boneIndices1[0], boneIndices1[1], boneIndices1[2], boneIndices1[3]
                        });
                        surfaceTool.SetWeights(new float[] {
                            boneWeights1[0], boneWeights1[1], boneWeights1[2], boneWeights1[3]
                        });
                        surfaceTool.SetNormal(norm1);
                        surfaceTool.SetUV(uv1);
                        surfaceTool.SetUV2(uv1_2);
                        surfaceTool.AddVertex(pos1);

                        surfaceTool.SetBones(new int[] {
                            boneIndices2[0], boneIndices2[1], boneIndices2[2], boneIndices2[3]
                        });
                        surfaceTool.SetWeights(new float[] {
                            boneWeights2[0], boneWeights2[1], boneWeights2[2], boneWeights2[3]
                        });
                        surfaceTool.SetNormal(norm2);
                        surfaceTool.SetUV(uv2);
                        surfaceTool.SetUV2(uv2_2);
                        surfaceTool.AddVertex(pos2);

                        surfaceTool.SetBones(new int[] {
                            boneIndices3[0], boneIndices3[1], boneIndices3[2], boneIndices3[3]
                        });
                        surfaceTool.SetWeights(new float[] {
                            boneWeights3[0], boneWeights3[1], boneWeights3[2], boneWeights3[3]
                        });
                        surfaceTool.SetNormal(norm3);
                        surfaceTool.SetUV(uv3);
                        surfaceTool.SetUV2(uv3_2);
                        surfaceTool.AddVertex(pos3);
                    }
                    else
                    {
                        // Add vertices without skinning data
                        surfaceTool.SetNormal(norm1);
                        surfaceTool.SetUV(uv1);
                        surfaceTool.SetUV2(uv1_2);
                        surfaceTool.AddVertex(pos1);

                        surfaceTool.SetNormal(norm2);
                        surfaceTool.SetUV(uv2);
                        surfaceTool.SetUV2(uv2_2);
                        surfaceTool.AddVertex(pos2);

                        surfaceTool.SetNormal(norm3);
                        surfaceTool.SetUV(uv3);
                        surfaceTool.SetUV2(uv3_2);
                        surfaceTool.AddVertex(pos3);
                    }

                    addedTriangles = true;
                }

                // Early exit if no valid triangles were created
                if (!addedTriangles)
                {
                    return;
                }
            }

            surfaceTool.GenerateTangents();


            // Get the material for this strip
            Material godotMaterial = null;
            var materials = modelData.Materials;
            if (materials != null && strip.MaterialIndex >= 0 && strip.MaterialIndex < materials.Materials.Count)
            {
                var material = materials.Materials[strip.MaterialIndex];
                godotMaterial = CreateMaterial(material);
            }
            else
            {
                // Create a default material
                godotMaterial = new StandardMaterial3D
                {
                    AlbedoColor = new Color(0.8f, 0.8f, 0.8f)
                };
            }

            // Set the material
            surfaceTool.SetMaterial(godotMaterial);

            // Commit to the array mesh - this automatically sets the material
        }
        catch (Exception ex)
        {
            GD.PushError($"Error creating surface from strip with SurfaceTool: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Helper method to process skinned vertex data for SurfaceTool
    private void ProcessSkinnedVertexData(Vertex vertex, TriangleStrip strip, float[] weights, int[] indices)
    {
        // Add the first three weights from the vertex
        weights[0] = vertex.BoneWeights.X;
        weights[1] = vertex.BoneWeights.Y;
        weights[2] = vertex.BoneWeights.Z;

        // Calculate the fourth weight (weights must sum to 1.0)
        float sum = vertex.BoneWeights.X + vertex.BoneWeights.Y + vertex.BoneWeights.Z;
        weights[3] = Math.Max(0.0f, 1.0f - sum);

        // Set bone indices - map local indices to skeleton indices if bone table exists
        if (strip.HasBoneTable)
        {
            // Use bone table to map local indices to skeleton indices
            indices[0] = vertex.BoneIndices[0] < strip.BoneTableLength ? strip.BoneTable[vertex.BoneIndices[0]] : 0;
            indices[1] = vertex.BoneIndices[1] < strip.BoneTableLength ? strip.BoneTable[vertex.BoneIndices[1]] : 0;
            indices[2] = vertex.BoneIndices[2] < strip.BoneTableLength ? strip.BoneTable[vertex.BoneIndices[2]] : 0;
            indices[3] = vertex.BoneIndices[3] < strip.BoneTableLength ? strip.BoneTable[vertex.BoneIndices[3]] : 0;
        }
        else
        {
            // No bone table - use raw indices or default to 0
            indices[0] = vertex.BoneIndices[0];
            indices[1] = vertex.BoneIndices[1];
            indices[2] = vertex.BoneIndices[2];
            indices[3] = vertex.BoneIndices[3];

            // If no bone influences, default to bone 0 with full weight
            if (weights[0] < 0.01f && weights[1] < 0.01f && weights[2] < 0.01f && weights[3] < 0.01f)
            {
                weights[0] = 1.0f;
                weights[1] = weights[2] = weights[3] = 0.0f;
                indices[0] = indices[1] = indices[2] = indices[3] = 0;
            }
        }
    }


    private Vector3 TransformPosition(System.Numerics.Vector3 position, float scale)
    {
        // Apply coordinate system transformation (Novalogic to Godot)
        // Flip X-axis to match the original importer's transformation
        return new Vector3(
            -position.X * scale,  // Flip X axis
            position.Y * scale,   // Keep Y axis
            position.Z * scale    // Keep Z axis
        );
    }

    private Vector3 TransformNormal(System.Numerics.Vector3 normal)
    {
        // Apply same transformation to normals as to positions
        return new Vector3(
            -normal.X,  // Flip X axis
            normal.Y,   // Keep Y axis
            normal.Z    // Keep Z axis
        ).Normalized(); // Ensure normal is normalized
    }

    private static StandardMaterial3D CreateMaterial(ONMaterial material)
    {
        var result = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
            CullMode = material.IsDoubleSided()
                ? BaseMaterial3D.CullModeEnum.Disabled
                : BaseMaterial3D.CullModeEnum.Back
        };

        // Handle FFP_GLASS shader
        if (material.IsGlass)
        {
            GD.Print($"Creating glass material for shader: {material.ShaderName}, IsGlass: {material.IsGlass}");
            result.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            result.AlbedoColor = new Color(1.0f, 1.0f, 1.0f, 0.1f); // Semi-transparent
            result.Metallic = 0.0f;
            result.Roughness = 0.0f; // Very smooth for glass
            result.RefractionScale = 0.05f;
            result.RefractionEnabled = true;


            // Set glass-like properties
            result.ClearcoatEnabled = true;
            result.Clearcoat = 1.0f;
            result.ClearcoatRoughness = 0.0f;
        }
        // Handle alpha blended materials (shader names ending with _AB)
        else if (material.ShaderName.EndsWith("_AB"))
        {
            result.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            result.BlendMode = BaseMaterial3D.BlendModeEnum.Mix;
        }
        // Handle alpha testing for non-glass materials
        else if (material.IsAlphaTested())
        {
            result.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
            result.AlphaScissorThreshold = material.GetAlphaTestValue();
        }

        // Process textures by priority and slot
        if (material.Textures.Count > 0)
        {
            ImageTexture diffuse0Texture = null;
            ImageTexture diffuse1Texture = null;
            ImageTexture normalTexture = null;

            // First pass: Collect all textures by type/slot
            foreach (var texture in material.Textures)
            {
                string path = ResolveTexturePath(texture.TextureName);
                if (string.IsNullOrEmpty(path))
                    continue;

                var image = LoadTextureImage(path);
                if (image == null)
                    continue;

                var tex = new ImageTexture();
                tex.SetImage(image);

                // Check for textures that indicate special materials by name
                string lowerName = texture.TextureName.ToLower();

                // Sort by slot/type
                if (texture.TextureSlot == TextureSlot.Diffuse0)
                {
                    // Priority for primary diffuse
                    diffuse0Texture = tex;
                }
                else if (texture.TextureSlot == TextureSlot.Diffuse1)
                {
                    // Secondary diffuse for blending
                    diffuse1Texture = tex;
                }
                else if (texture.TextureSlot == TextureSlot.Normal ||
                        texture.TextureType == TextureType.Normal ||
                        texture.TextureType == TextureType.NormalDot3 ||
                        lowerName.EndsWith(".mdt"))
                {
                    // Normal map texture (include .mdt extensions as normal maps)
                    normalTexture = tex;
                }
            }

            if (diffuse0Texture != null)
            {
                result.AlbedoTexture = diffuse0Texture;
            }

            if (normalTexture != null)
            {
                result.NormalEnabled = true;
                result.NormalTexture = normalTexture;
                result.NormalScale = 1.0f;
            }

            if (diffuse1Texture != null)
            {
                result.DetailEnabled = true;
                result.DetailMask = diffuse1Texture;
                result.DetailAlbedo = diffuse1Texture;
                result.DetailUVLayer = BaseMaterial3D.DetailUV.UV2; // Use second UV set
                result.DetailBlendMode = BaseMaterial3D.BlendModeEnum.Mul;
            }
        }

        return result;
    }

    private static string ResolveTexturePath(string textureName)
    {
        return FileUtils.ResolveResourcePath(textureName, "res://assets//textures", new string[] { ".tga", ".dds", ".mdt" });
    }

    private static Image LoadTextureImage(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            // Return default placeholder for empty paths
            var p = Godot.Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
            p.Fill(new Color(0.8f, 0.8f, 0.8f));
            return p;
        }

        string normalizedPath = path.Replace('\\', '/');

        var texture = ResourceLoader.Load(normalizedPath, "Texture2D", ResourceLoader.CacheMode.Replace);
        if (texture != null && texture is Texture2D loadedTexture)
            return loadedTexture.GetImage();

        var image = new Image();
        if (image.Load(normalizedPath) == Error.Ok)
            return image;

        // Default placeholder
        var placeholder = Godot.Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
        placeholder.Fill(new Color(0.8f, 0.8f, 0.8f));
        return placeholder;
    }


    // Container for all model data
    private class ModelData
    {
        public string ModelName { get; set; } = string.Empty;
        public List<RenderObject> RenderObjects { get; set; } = new();
        public List<TriangleStrip> Strips { get; set; } = new();
        public List<Vertex> Vertices { get; set; } = new();
        public List<ushort> Indices { get; set; } = new();
        public MaterialCollection Materials { get; set; } = new();
        public List<Light> Lights { get; set; } = new();
        public List<UserPoint> UserPoints { get; set; } = new();
        public bool HasSkinnedMeshes { get; set; }


        // Collision Data
        public bool HasCollisionData { get; set; }
        public List<CollisionObject> CollisionObjects { get; set; } = new();
        public List<CollisionVertex> CollisionVertices { get; set; } = new();
        public List<CollisionFace> CollisionFaces { get; set; } = new();
        public List<BoundingVolume> BoundingVolumes { get; set; } = new();
        public List<BoundingPlane> BoundingPlanes { get; set; } = new();
    }

    // Extract all model data from the .3di file
    private ModelData ExtractModelData(ModelFile modelFile)
    {
        var modelData = new ModelData();

        try
        {
            // Extract header information
            var ghdrNode = FindNodeByPath(modelFile.RootNode, "ROOT/GHDR");
            if (ghdrNode == null)
            {
                GD.PushError("Model file has no GHDR node");
                return null;
            }

            var lightNode = FindNodeByPath(modelFile.RootNode, "ROOT/LGHT");
            if (lightNode == null)
            {
                GD.PushError("Model file has no LGHT node");
                return null;
            }

            var usrpNode = FindNodeByPath(modelFile.RootNode, "ROOT/USRP");
            if (lightNode == null)
            {
                GD.PushError("Model file has no USRP node");
                return null;
            }

            var header = ModelHeader.FromNode(ghdrNode);
            modelData.ModelName = header.Name;

            // Get MTRL node for materials
            var mtrlNode = FindNodeByPath(modelFile.RootNode, "ROOT/MTRL");
            if (mtrlNode != null)
            {
                modelData.Materials = MaterialCollection.FromNode(mtrlNode);
            }

            // Find RDTA for LOD information
            var rdtaNode = FindNodeByPath(modelFile.RootNode, "ROOT/RDTA");
            if (rdtaNode == null || rdtaNode.Children.Count == 0)
            {
                GD.PushError("Model has no RDTA node or no LODs");
                return null;
            }

            // Get the first LOD node
            ModelNode lodNode = null;
            foreach (var child in rdtaNode.Children)
            {
                if (child.Identifier == "RLOD")
                {
                    lodNode = child;
                    break;
                }
            }

            if (lodNode == null)
            {
                GD.PushError("Model has no RLOD nodes");
                return null;
            }

            // Get all required nodes for rendering
            var vertNode = FindNodeInChildren(lodNode, "VERT");
            var indxNode = FindNodeInChildren(lodNode, "INDX");
            var strpNode = FindNodeInChildren(lodNode, "STRP");
            var robjNode = FindNodeInChildren(lodNode, "ROBJ");

            if (vertNode == null || indxNode == null || strpNode == null || robjNode == null)
            {
                GD.PushError("LOD is missing essential nodes (VERT, INDX, STRP, or ROBJ)");
                return null;
            }

            // Parse LOD data
            modelData.Vertices = Vertex.FromNode(vertNode, out var vertexCount);
            modelData.Indices = IndexBuffer.FromNode(indxNode, out var indexCount);
            modelData.Strips = TriangleStrip.FromNode(strpNode);
            modelData.RenderObjects = RenderObject.FromNode(robjNode);
            modelData.Lights = Light.FromNode(lightNode);
            modelData.UserPoints = UserPoint.FromNode(usrpNode);

            // Extract collision data if available
            var cdtaNode = FindNodeByPath(modelFile.RootNode, "ROOT/CDTA");
            if (cdtaNode != null)
            {
                modelData.HasCollisionData = true;

                var cvertNode = FindNodeInChildren(cdtaNode, "CVRT");
                var cfacNode = FindNodeInChildren(cdtaNode, "CFAC");
                var cobjNode = FindNodeInChildren(cdtaNode, "COBJ");
                var bvolNode = FindNodeInChildren(cdtaNode, "BVOL");
                var bplnNode = FindNodeInChildren(cdtaNode, "BPLN");

                if (cvertNode != null)
                    modelData.CollisionVertices = CollisionVertex.FromNode(cvertNode);
                if (cfacNode != null)
                    modelData.CollisionFaces = CollisionFace.FromNode(cfacNode);
                if (cobjNode != null)
                    modelData.CollisionObjects = CollisionObject.FromNode(cobjNode);
                if (bvolNode != null)
                    modelData.BoundingVolumes = BoundingVolume.FromNode(bvolNode);
                if (bplnNode != null)
                    modelData.BoundingPlanes = BoundingPlane.FromNode(bplnNode);
            }

            // Check if this model has any skinned meshes
            modelData.HasSkinnedMeshes = header.MeshType == MeshType.SKINNED;


            return modelData;
        }
        catch (Exception ex)
        {
            GD.PushError($"Error extracting model data: {ex.Message}");
            return null;
        }
    }

    // Helper method to find node by path
    private ModelNode FindNodeByPath(ModelNode root, string path)
    {
        if (root == null)
            return null;

        string[] parts = path.Split('/');
        if (parts.Length == 0 || parts[0] != root.Identifier)
            return null;

        ModelNode current = root;

        for (int i = 1; i < parts.Length; i++)
        {
            bool found = false;
            foreach (var child in current.Children)
            {
                if (child.Identifier == parts[i])
                {
                    current = child;
                    found = true;
                    break;
                }
            }

            if (!found)
                return null;
        }

        return current;
    }

    // Helper method to find node in children
    private ModelNode FindNodeInChildren(ModelNode parent, string identifier)
    {
        if (parent == null)
            return null;

        foreach (var child in parent.Children)
        {
            if (child.Identifier == identifier)
                return child;
        }

        return null;
    }

    // Helper method to set owner recursively for a node and all its children
    private void SetOwnerRecursively(Node node, Node owner)
    {
        if (node == null)
            return;

        // Set the owner for this node
        node.Owner = owner;

        // Process all children
        foreach (var child in node.GetChildren())
        {
            // Only set ownership if not already set
            if (child.Owner == null)
            {
                SetOwnerRecursively(child, owner);
            }
        }
    }

    /// <summary>
    /// Adds collision objects using bounding volumes and planes positioned with AbsX,Y,Z coordinates
    /// </summary>
    private void ProcessBoundingVolumeCollisions(ModelData modelData, Node3D rootNode, Dictionary<int, Node3D> parts)
    {
        // Extract all collision data from the model file
        var collisionObjectsData = GetCollisionObjectsFromModelData(modelData);
        var boundingVolumes = GetBoundingVolumesFromModelData(modelData);
        var boundingPlanes = GetBoundingPlanesFromModelData(modelData);

        if (collisionObjectsData.Count == 0 || boundingVolumes.Count == 0)
            return;

        // Track the current bounding volume and plane indices
        int currentVolumeIndex = 0;
        int currentPlaneIndex = 0;
        int processedCount = 0;

        // Track counters for each node type within each collision object
        var nodeTypeCounters = new Dictionary<int, Dictionary<NodeType, int>>();

        // Process each collision object
        for (var i = 0; i < collisionObjectsData.Count; i++)
        {
            var cobj = collisionObjectsData[i];

            // Skip objects with zero radius
            if (cobj.Radius <= 0.0001f)
            {
                GD.Print($"Skipping collision object {i + 1} - zero radius");
                continue;
            }

            // Check if parent index is valid
            if (cobj.ParentSubObjectIndex < 0 || cobj.ParentSubObjectIndex >= parts.Count)
            {
                GD.Print($"Skipping collision object {i + 1} - invalid parent index: {cobj.ParentSubObjectIndex}");
                continue;
            }

            // Get the parent part node
            if (!parts.TryGetValue(cobj.ParentSubObjectIndex, out var parentPart))
            {
                GD.Print($"Skipping collision object {i + 1} - parent part not found: {cobj.ParentSubObjectIndex}");
                continue;
            }

            // Initialize counter for this collision object if not exists
            if (!nodeTypeCounters.ContainsKey(i))
            {
                nodeTypeCounters[i] = new Dictionary<NodeType, int>();
            }

            // Create matrix for local positioning like ProcessUserPoints
            var mat = System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(
                -modelData.RenderObjects[cobj.ParentSubObjectIndex].AbsX,
                modelData.RenderObjects[cobj.ParentSubObjectIndex].AbsY,
                modelData.RenderObjects[cobj.ParentSubObjectIndex].AbsZ
            ));
            System.Numerics.Matrix4x4.Invert(mat, out var inv);

            processedCount++;

            // Process any bounding volumes that belong to this collision object
            int volumeCount = (int)cobj.NumBoundingVolumes;
            if (volumeCount > 0 && currentVolumeIndex < boundingVolumes.Count)
            {
                // Process the bounding volumes for this collision object
                for (int v = 0; v < volumeCount && currentVolumeIndex < boundingVolumes.Count; v++)
                {
                    var bvol = boundingVolumes[currentVolumeIndex];

                    // Get the planes for this volume
                    var volumePlanes = new List<BoundingPlane>();
                    if (bvol.NumBoundingPlanes > 0 && currentPlaneIndex < boundingPlanes.Count)
                    {
                        var endPlaneIndex = Math.Min(currentPlaneIndex + (int)bvol.NumBoundingPlanes, boundingPlanes.Count);
                        volumePlanes = boundingPlanes.GetRange(currentPlaneIndex, endPlaneIndex - currentPlaneIndex);
                        currentPlaneIndex = endPlaneIndex;
                    }

                    if (bvol.CollidableType != NodeType.CB &&
                        bvol.CollidableType != NodeType.CL &&
                        bvol.CollidableType != NodeType.CA)
                        continue;

                    // Increment counter for this node type
                    if (!nodeTypeCounters[i].ContainsKey(bvol.CollidableType))
                    {
                        nodeTypeCounters[i][bvol.CollidableType] = 0;
                    }
                    nodeTypeCounters[i][bvol.CollidableType]++;

                    // Create appropriate collision node based on type
                    string nodeTypeName = $"{bvol.CollidableType}{cobj.ParentSubObjectIndex + 1:D2} {nodeTypeCounters[i][bvol.CollidableType]}";

                    Node3D rigidBody;
                    if (bvol.CollidableType == NodeType.CA)
                    {
                        // CA types are areas (armory, triggers, etc.)
                        var areaNode = new Area3D { Name = nodeTypeName };
                        areaNode.BodyEntered += (body) =>
                        {
                            GD.Print($"Body entered area: {nodeTypeName}");
                        };
                        rigidBody = areaNode;
                    }
                    else
                    {
                        // CB, CL, etc. are solid collision
                        rigidBody = new StaticBody3D { Name = nodeTypeName };
                    }

                    {
                        // Calculate center in world space, then transform to local space
                        var worldCenter = (ConvertCollisionPosition(bvol.Min) +
                                         ConvertCollisionPosition(bvol.Max)) * 0.5f;

                        var snCenter = new Godot.Vector3(worldCenter.X, worldCenter.Y, worldCenter.Z);

                        // Get a point cloud for convex hull from the planes
                        var points = PointCloudHelper.GetPointCloudVertices("", snCenter, volumePlanes);

                        // Create a convex polygon shape with these points
                        var convexShape = new ConvexPolygonShape3D();
                        convexShape.Points = points;

                        // Add collision shape to the body
                        var collisionShape = new CollisionShape3D
                        {
                            Shape = convexShape,
                            Name = "Shape"
                        };
                        ConfigureCollisionForVolumeType(collisionShape, bvol.CollidableType);
                        rigidBody.AddChild(collisionShape);
                    }

                    // Configure physics properties based on volume type

                    // Position the rigid body using local coordinates
                    var volumeCenter = (ConvertCollisionPosition(bvol.Min) +
                                       ConvertCollisionPosition(bvol.Max)) * 0.5f;
                    var snVolumeCenter = new System.Numerics.Vector3(volumeCenter.X, volumeCenter.Y, volumeCenter.Z);
                    var localVolumePos = System.Numerics.Vector3.Transform(snVolumeCenter, inv);

                    // Add rigid body directly to part node
                    parentPart.AddChild(rigidBody);

                    // Move to the next volume
                    currentVolumeIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Extract collision objects from ModelData
    /// </summary>
    private static List<CollisionObject> GetCollisionObjectsFromModelData(ModelData modelData)
    {
        return modelData.CollisionObjects ?? new List<CollisionObject>();
    }

    /// <summary>
    /// Extract bounding volumes from ModelData
    /// </summary>
    private static List<BoundingVolume> GetBoundingVolumesFromModelData(ModelData modelData)
    {
        return modelData.BoundingVolumes ?? new List<BoundingVolume>();
    }

    /// <summary>
    /// Extract bounding planes from ModelData
    /// </summary>
    private static List<BoundingPlane> GetBoundingPlanesFromModelData(ModelData modelData)
    {
        return modelData.BoundingPlanes ?? new List<BoundingPlane>();
    }

    /// <summary>
    /// Configure collision properties based on volume type
    /// </summary>
    private static void ConfigureCollisionForVolumeType(CollisionShape3D collisionShape, NodeType volumeType)
    {
        collisionShape.DebugColor = GetColorForNodeType(volumeType);
    }

    /// <summary>
    /// Get color for different collision types for visual debugging
    /// </summary>
    private static Color GetColorForNodeType(NodeType nodeType)
    {
        return nodeType switch
        {
            NodeType.CB => new Color(0.0f, 1.0f, 0.0f), // Green - Generic collision
            NodeType.CL => new Color(1.0f, 1.0f, 0.0f), // Yellow - Ladder
            NodeType.CA => new Color(0.0f, 0.0f, 1.0f), // Blue - Armory
            NodeType.CD => new Color(0.8f, 0.4f, 0.0f), // Orange - Door
            NodeType.CT => new Color(1.0f, 0.0f, 1.0f), // Magenta - Team change
            NodeType.CF => new Color(0.0f, 1.0f, 1.0f), // Cyan - Special function
            NodeType.CP => new Color(0.5f, 1.0f, 0.5f), // Light green - Player only
            NodeType.VC => new Color(0.5f, 0.5f, 1.0f), // Light blue - Vehicle
            NodeType.BB => new Color(1.0f, 0.5f, 0.5f), // Light red - Blink box
            NodeType.CS => new Color(0.7f, 0.7f, 0.7f), // Gray - CS type
            NodeType.CC => new Color(0.6f, 0.3f, 0.6f), // Purple - CC type
            NodeType.CV => new Color(0.3f, 0.6f, 0.3f), // Dark green - CV type
            NodeType.CM => new Color(0.6f, 0.6f, 0.3f), // Olive - CM type
            NodeType.VK => new Color(0.3f, 0.3f, 0.6f), // Dark blue - VK type
            NodeType.LP => new Color(0.8f, 0.8f, 0.0f), // Bright yellow - LP type
            _ => new Color(1.0f, 0.0f, 0.0f) // Red - Unknown/unrecognized
        };
    }
}
#endif

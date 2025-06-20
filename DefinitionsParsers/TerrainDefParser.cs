using Godot;
using OpenNova.Definition;
using OpenNova.Terrain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenNova.DefinitionsParsers
{
    public class TerrainDefParser
    {
        public TerrainDefinition Parse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Terrain definition file not found: {filePath}");
            }

            var content = File.ReadAllBytes(filePath);
            return Parse(content);
        }

        public TerrainDefinition Parse(byte[] data)
        {
            var content = Encoding.Default.GetString(data);
            var terrainDef = new TerrainDefinition();
            terrainDef.Init();

            var lines = Regex.Split(content, "\r\n|\r|\n");
            var processedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                var commentIndex = trimmedLine.IndexOf(';');
                if (commentIndex >= 0)
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();

                if (!string.IsNullOrEmpty(trimmedLine))
                    processedLines.Add(trimmedLine);
            }

            var sectorsList = new List<int[]>();
            bool inFoliageBlock = false;
            FoliageDef currentFoliage = null;

            foreach (var line in processedLines)
            {
                if (inFoliageBlock)
                {
                    if (line.Equals("end", StringComparison.OrdinalIgnoreCase))
                    {
                        terrainDef.Foliages.Add(currentFoliage);
                        inFoliageBlock = false;
                    }
                    else
                    {
                        var key = line.Substring(0, line.IndexOfAny(new[] { ' ', '\t' })).Trim();
                        var value = GetValue(line);

                        switch (key.ToLower())
                        {
                            case "graphic":
                                currentFoliage.Graphic = value;
                                break;
                            case "color_lower":
                                currentFoliage.ColorLower = int.Parse(value);
                                break;
                            case "color_upper":
                                currentFoliage.ColorUpper = int.Parse(value);
                                break;
                            case "match":
                                currentFoliage.Match = int.Parse(value);
                                break;
                            case "attrib":
                                var attribs = value.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var attrib in attribs)
                                {
                                    currentFoliage.Attrib.Add(attrib);
                                }
                                break;
                        }
                    }
                }
                else if (line.StartsWith("foliage", StringComparison.OrdinalIgnoreCase))
                {
                    inFoliageBlock = true;
                    currentFoliage = new FoliageDef();
                    currentFoliage.Init();
                }
                else
                {
                    var key = line.Substring(0, line.IndexOfAny(new[] { ' ', '\t' })).Trim();
                    var value = GetValue(line);

                    switch (key.ToLower())
                    {
                        case "terrain_name":
                            terrainDef.TerrainName = value;
                            if (string.IsNullOrEmpty(terrainDef.Name))
                                terrainDef.Name = value;
                            break;
                        case "terrain_creator":
                            terrainDef.TerrainCreator = value;
                            break;
                        case "water_height":
                            terrainDef.WaterHeight = float.Parse(value);
                            break;
                        case "polytrn_detaildensity":
                            terrainDef.DetailDensity = int.Parse(value);
                            break;
                        case "polytrn_detaildensity2":
                            terrainDef.DetailDensity2 = int.Parse(value);
                            break;
                        case "polytrn_sectorcount":
                            terrainDef.SectorCount = int.Parse(value);
                            break;
                        case "polytrn_wrapx":
                            terrainDef.WrapX = int.Parse(value) != 0;
                            break;
                        case "polytrn_wrapy":
                            terrainDef.WrapY = int.Parse(value) != 0;
                            break;
                        case "polytrn_origin":
                            {
                                var values = GetValues(line);
                                terrainDef.Origin = new Godot.Vector2I(int.Parse(values[0]), int.Parse(values[1]));
                            }
                            break;
                        case "polytrn_sectors":
                            {
                                var values = GetValues(line);
                                var intValues = Array.ConvertAll(values, int.Parse);
                                sectorsList.Add(intValues);
                            }
                            break;
                        case "polytrn_colormap":
                            terrainDef.PolytrnColormap = LoadTexture(value);
                            break;
                        case "polytrn_detailmap":
                            terrainDef.PolytrnDetailmap = LoadTexture(value);
                            break;
                        case "polytrn_detailmap_c1":
                            terrainDef.PolytrnDetailmapC1 = LoadTexture(value);
                            break;
                        case "polytrn_detailmap_c2":
                            terrainDef.PolytrnDetailmapC2 = LoadTexture(value);
                            break;
                        case "polytrn_detailmap_c3":
                            terrainDef.PolytrnDetailmapC3 = LoadTexture(value);
                            break;
                        case "polytrn_detailmap2":
                            terrainDef.PolytrnDetailmap2 = LoadTexture(value);
                            break;
                        case "polytrn_detailmapdist":
                            terrainDef.PolytrnDetailmapdist = LoadTexture(value);
                            break;
                        case "polytrn_polydata":
                            terrainDef.TerrainPolyDataFile = LoadTerrainPolyData(value);
                            break;
                        case "polytrn_tilestrip":
                            terrainDef.PolytrnTilestrip = LoadTexture(value);
                            break;
                        case "polytrn_charmap":
                            terrainDef.PolytrnCharmap = LoadTexture(value);
                            break;
                        case "polytrn_foliagemap":
                            terrainDef.PolytrnFoliagemap = LoadTexture(value);
                            break;
                        case "polytrn_detailblendmap":
                            terrainDef.PolytrnDetailblendmap = LoadTexture(value);
                            break;
                        case "water_rgb":
                            terrainDef.WaterRgb = Array.ConvertAll(value.Split(','), int.Parse);
                            break;
                        case "water_murk":
                            terrainDef.WaterMurk = float.Parse(value);
                            break;
                        case "lock_topleft":
                            {
                                var values = GetValues(line);
                                terrainDef.LockTopLeft = new Godot.Vector2I(int.Parse(values[0]), int.Parse(values[1]));
                            }
                            break;
                        case "lock_topright":
                            {
                                var values = GetValues(line);
                                terrainDef.LockTopRight = new Godot.Vector2I(int.Parse(values[0]), int.Parse(values[1]));
                            }
                            break;
                        case "lock_bottomleft":
                            {
                                var values = GetValues(line);
                                terrainDef.LockBottomLeft = new Godot.Vector2I(int.Parse(values[0]), int.Parse(values[1]));
                            }
                            break;
                        case "lock_bottomright":
                            {
                                var values = GetValues(line);
                                terrainDef.LockBottomRight = new Godot.Vector2I(int.Parse(values[0]), int.Parse(values[1]));
                            }
                            break;
                    }
                }
            }

            if (sectorsList.Count > 0)
            {
                terrainDef.SectorsHeight = sectorsList.Count;
                terrainDef.SectorsWidth = sectorsList[0].Length;

                var flatSectors = new List<int>();
                foreach (var row in sectorsList)
                {
                    flatSectors.AddRange(row);
                }
                terrainDef.Sectors = flatSectors.ToArray();
            }

            if (string.IsNullOrEmpty(terrainDef.Name) && !string.IsNullOrEmpty(terrainDef.TerrainName))
            {
                terrainDef.Name = terrainDef.TerrainName;
            }
            else if (string.IsNullOrEmpty(terrainDef.Name))
            {
                terrainDef.Name = Path.GetFileNameWithoutExtension(data.ToString());
            }

            return terrainDef;
        }

        private static string GetValue(string line)
        {
            var index = line.IndexOfAny(new[] { ' ', '\t' });
            if (index >= 0)
                return line.Substring(index).Trim(new[] { ' ', '\t', '"' });
            return string.Empty;
        }

        private static string[] GetValues(string line)
        {
            var value = GetValue(line);
            return value.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static Texture2D LoadTexture(string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
                return null;

            var resolvedPath = FileUtils.ResolveResourcePath(texturePath, "res://assets/textures/", new string[] { ".tga", ".dds", ".mdt" });
            if (!string.IsNullOrEmpty(resolvedPath))
            {
                var texture = ResourceLoader.Load<Texture2D>(resolvedPath);
                if (texture != null)
                {
                    return texture;
                }
                else
                {
                    GD.PrintErr($"Failed to load texture: {resolvedPath}");
                }
            }
            else
            {
                GD.PrintErr($"Could not resolve texture path: {texturePath}");
            }
            return null;
        }

        private static TerrainPolyDataFile LoadTerrainPolyData(string polydataPath)
        {
            if (string.IsNullOrEmpty(polydataPath))
                return null;

            var resolvedPath = FileUtils.ResolveResourcePath(
                FileUtils.EnsureExtension(polydataPath, "cpt")
            );
            if (!string.IsNullOrEmpty(resolvedPath))
            {
                var polydata = ResourceLoader.Load<TerrainPolyDataFile>(resolvedPath);
                if (polydata != null)
                {
                    return polydata;
                }
                else
                {
                    GD.PrintErr($"Failed to load polydata: {resolvedPath}");
                }
            }
            else
            {
                GD.PrintErr($"Could not resolve polydata path: {polydataPath}");
            }
            return null;
        }
    }
}

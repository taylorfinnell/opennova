using Godot;
using OpenNova.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenNova.Terrain
{

    [Tool]
    [GlobalClass]
    public partial class TerrainPolyDataFile : Resource
    {
        internal static readonly float SECTOR_SIZE = 512f;

        [Export] public TerrainPolyDataHeader Header { get; private set; }
        [Export] public TerrainPolyDataDepthMap DepthMap { get; private set; }
        [Export] public QuadTreeNode RootNode { get; private set; }

        public void Parse(string filePath)
        {
            Parse(File.ReadAllBytes(filePath));
        }

        public void Parse(byte[] data)
        {
            using (BitReader bitReader = new BitReader(data))
            {
                Header = new TerrainPolyDataHeader();
                Header.Deserialize(bitReader);

                while (true)
                {
                    byte[] chunkMagic = bitReader.ReadBytes(4);

                    if (chunkMagic[0] == 0 && chunkMagic[1] == 0 && chunkMagic[2] == 0 && chunkMagic[3] == 0)
                    {
                        break;
                    }

                    string chunkName = System.Text.Encoding.ASCII.GetString(chunkMagic);
                    switch (chunkName)
                    {
                        case "POLY":
                            ParsePolyChunk(bitReader);
                            break;
                        case "CDEP":
                            DepthMap = new TerrainPolyDataDepthMap();
                            DepthMap.Init(Header.InitialSize, Header.InitialSize);
                            DepthMap.Deserialize(bitReader);
                            break;
                        default:
                            GD.Print($"Unknown chunk: {chunkName}");
                            break;
                    }

                    bitReader.MaybeAlignToNextByte();
                }
            }
        }

        public int GetNumLodLevels()
        {
            return Header.NumLodLevels;
        }

        public int GetDepthMapWidth()
        {
            return Header.InitialSize;
        }

        public int GetDepthMapHeight()
        {
            return Header.InitialSize;
        }

        public float GetHeightAt(int x, int y)
        {
            if (DepthMap == null || x < 0 || y < 0 || x >= GetDepthMapWidth() || y >= GetDepthMapHeight())
                return 0f;

            int index = y * GetDepthMapWidth() + x;
            if (index >= DepthMap.DepthMap.Count)
                return 0f;

            return DepthMap.DepthMap[index] / 256f;
        }

        public float GetInterpolatedHeightAt(float worldX, float worldY)
        {
            // Convert world coordinates to grid coordinates
            float gridX = worldX;
            float gridY = worldY;

            // Get the four surrounding grid points
            int x0 = Mathf.FloorToInt(gridX);
            int y0 = Mathf.FloorToInt(gridY);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            // Get fractional parts for interpolation
            float fx = gridX - x0;
            float fy = gridY - y0;

            // Get heights at the four corners
            float h00 = GetHeightAt(x0, y0);
            float h10 = GetHeightAt(x1, y0);
            float h01 = GetHeightAt(x0, y1);
            float h11 = GetHeightAt(x1, y1);

            // Bilinear interpolation
            float h0 = Mathf.Lerp(h00, h10, fx);
            float h1 = Mathf.Lerp(h01, h11, fx);
            return Mathf.Lerp(h0, h1, fy);
        }

        // Map sector index (1-4) to heightmap coordinates
        public Vector2I GetSectorOffset(int sectorIndex)
        {
            return sectorIndex switch
            {
                1 => new Vector2I(0, 0),       // Top-left
                2 => new Vector2I(0, 512),     // Bottom-left  
                3 => new Vector2I(512, 0),     // Top-right
                4 => new Vector2I(512, 512),   // Bottom-right
                _ => new Vector2I(0, 0)
            };
        }

        // Get height for a specific sector and local coordinates
        public float GetSectorHeightAt(int sectorIndex, int localX, int localY)
        {
            var offset = GetSectorOffset(sectorIndex);
            return GetHeightAt(offset.X + localX, offset.Y + localY);
        }

        // Find the appropriate LOD level based on distance
        public int GetLodLevel(float distance, float maxDistance = 2048f)
        {
            if (GetNumLodLevels() <= 1) return 0;

            float normalizedDistance = Mathf.Clamp(distance / maxDistance, 0f, 1f);
            int lodLevel = Mathf.FloorToInt(normalizedDistance * (GetNumLodLevels() - 1));
            return Mathf.Clamp(lodLevel, 0, GetNumLodLevels() - 1);
        }

        // Get quadtree nodes at a specific LOD level
        public QuadTreeNode[] GetNodesAtLod(int lodLevel)
        {
            if (RootNode == null || lodLevel < 0 || lodLevel >= GetNumLodLevels())
                return new QuadTreeNode[0];

            var nodes = new List<QuadTreeNode>();
            CollectNodesAtLevel(RootNode, lodLevel, 0, nodes);
            return nodes.ToArray();
        }

        private void CollectNodesAtLevel(QuadTreeNode node, int targetLevel, int currentLevel, List<QuadTreeNode> result)
        {
            if (currentLevel == targetLevel)
            {
                result.Add(node);
                return;
            }

            if (node.HasChildren && currentLevel < targetLevel)
            {
                foreach (var child in node.Children)
                {
                    if (child != null)
                        CollectNodesAtLevel(child, targetLevel, currentLevel + 1, result);
                }
            }
        }

        // Get all nodes that intersect with a world space bounding box
        public QuadTreeNode[] GetNodesInBounds(Vector2 minBounds, Vector2 maxBounds, int maxLodLevel = -1)
        {
            if (RootNode == null) return new QuadTreeNode[0];

            if (maxLodLevel < 0) maxLodLevel = GetNumLodLevels() - 1;

            var nodes = new List<QuadTreeNode>();
            CollectNodesInBounds(RootNode, minBounds, maxBounds, maxLodLevel, 0, nodes);
            return nodes.ToArray();
        }

        private void CollectNodesInBounds(QuadTreeNode node, Vector2 minBounds, Vector2 maxBounds,
                                        int maxLodLevel, int currentLevel, List<QuadTreeNode> result)
        {
            // Check if node intersects with bounds
            float nodeMinX = node.SectorX;
            float nodeMinY = node.SectorY;
            float nodeMaxX = node.SectorX + node.Size;
            float nodeMaxY = node.SectorY + node.Size;

            bool intersects = !(nodeMaxX < minBounds.X || nodeMinX > maxBounds.X ||
                               nodeMaxY < minBounds.Y || nodeMinY > maxBounds.Y);

            if (!intersects) return;

            // Add this node if we're at max LOD or it has no children
            if (currentLevel >= maxLodLevel || !node.HasChildren)
            {
                result.Add(node);
                return;
            }

            // Recurse to children
            foreach (var child in node.Children)
            {
                if (child != null)
                    CollectNodesInBounds(child, minBounds, maxBounds, maxLodLevel, currentLevel + 1, result);
            }
        }

        private void ParsePolyChunk(BitReader bitReader)
        {
            int expectedNodeCount = Header.GetTotalNodeCount();

            int minNodeSize = Header.InitialSize / (int)Math.Pow(2, Header.NumLodLevels - 1);

            Queue<QuadTreeNode> nodesQueue = new Queue<QuadTreeNode>();

            RootNode = new QuadTreeNode();
            RootNode.Init(0, 0, Header.InitialSize, 0);

            nodesQueue.Enqueue(RootNode);

            while (nodesQueue.Count > 0)
            {
                QuadTreeNode currentNode = nodesQueue.Dequeue();

                ParseNodeData(bitReader, currentNode);

                if (currentNode.Size > minNodeSize)
                {
                    int childSize = currentNode.Size / 2;
                    int childLevel = currentNode.Level + 1;

                    for (int i = 0; i < 4; i++)
                    {
                        int offsetX = i % 2 == 1 ? childSize : 0;
                        int offsetY = i / 2 == 1 ? childSize : 0;

                        QuadTreeNode childNode = new QuadTreeNode();
                        childNode.Init(
                            currentNode.SectorX + offsetX,
                            currentNode.SectorY + offsetY,
                            childSize,
                            childLevel
                         );

                        childNode.Parent = currentNode;
                        currentNode.Children[i] = childNode;
                        currentNode.HasChildren = true;

                        nodesQueue.Enqueue(childNode);
                    }
                }
            }
        }

        private void ParseNodeData(BitReader bitReader, QuadTreeNode node)
        {
            bitReader.AlignTo4Bytes();

            int sectorX = bitReader.ReadBits(10);
            int sectorY = bitReader.ReadBits(10);
            int numVertices = bitReader.ReadBits(16);
            int vertBitSize = bitReader.ReadBits(4);
            int numStrips = bitReader.ReadBits(7);
            bool isFlagged = bitReader.ReadBits(1) != 0;
            int stripChunkSize = bitReader.ReadBits(8);

            node.SectorX = sectorX;
            node.SectorY = sectorY;

            if (numVertices > 0)
            {
                for (int i = 0; i < numVertices; i++)
                {
                    int x = bitReader.ReadBits(vertBitSize);
                    int y = bitReader.ReadBits(vertBitSize);

                    int row = sectorY + y & 1023;
                    int col = sectorX + x & 1023;
                    int idx = 1024 * row + col;

                    var vert = new TerrainVertex
                    {
                        X = x,
                        Y = y,
                        Z = (float)DepthMap.DepthMap[idx] / 256
                    };

                    node.Vertices.Add(vert);
                }
            }

            bitReader.AlignTo4Bytes();

            if (numStrips > 0)
            {
                for (int t = 0; t < numStrips; ++t)
                {
                    bitReader.MaybeAlignToNextByte();

                    int numBits = bitReader.ReadBits(16);
                    int stripVertCount = bitReader.ReadBits(15);
                    int flagged = bitReader.ReadBits(1);

                    TerrainTriangleList strip = new TerrainTriangleList { NumVertices = stripVertCount };
                    strip.Indices = new Godot.Collections.Array<int>();

                    int indexCount = 0;
                    for (int i = numBits; i > 0; i -= stripChunkSize)
                    {
                        int s = Math.Min(stripChunkSize, i);

                        int numBitsNeeded = bitReader.ReadBits(4);
                        int m = bitReader.ReadBits(12);

                        for (int q = 0; q < s; q++)
                        {
                            int idx = bitReader.ReadBits(numBitsNeeded);
                            strip.Indices.Add(idx + m);
                            indexCount++;
                        }
                    }

                    strip.NumIndices = indexCount;
                    node.Strips.Add(strip);
                }
            }
        }
    }
}


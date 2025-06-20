using Godot;
using MIConvexHull;
using OpenNova.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNova.Utils;

public static class PointCloudHelper
{
    private const float EPSILON = 1e-6f;

    private struct Halfspace
    {
        public Vector3 Normal;
        public float D;
        public override string ToString()
        {
            return $"Normal: {Normal}, D: {D}";
        }
    }

    private class ConvexVertex : IVertex
    {
        public double[] Position { get; set; }

        public ConvexVertex(Vector3 v)
        {
            Position = new[] { v.X, v.Y, (double)v.Z };
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
        }
    }

    public static Vector3[] GetPointCloudVertices(string shapeClass, Vector3 interiorPoint, List<BoundingPlane> planes)
    {
        if (planes == null || planes.Count == 0)
        {
            GD.PrintErr($"No planes provided for {shapeClass}, using default box");
            return CreateDefaultBox(interiorPoint);
        }

        Vector3 center = interiorPoint;

        var halfspaces = new List<Halfspace>();
        foreach (var plane in planes)
        {
            float D = -plane.Radius;
            var halfspace = new Halfspace { Normal = ConvertCollisionVectorToGodot(plane.Normal), D = D };
            halfspaces.Add(halfspace);
        }

        var vertices = FindIntersectionVertices(halfspaces);

        if (vertices.Count < 4)
        {
            GD.PrintErr($"Not enough vertices for {shapeClass} ({vertices.Count}), using default box");
            return CreateDefaultBox(interiorPoint);
        }
        var convexVertices = vertices.Select(v => new ConvexVertex(v)).ToList();

        var hullCreation = ConvexHull.Create<ConvexVertex, DefaultConvexFace<ConvexVertex>>(convexVertices);

        if (hullCreation.Outcome != ConvexHullCreationResultOutcome.Success)
        {
            return CreateDefaultBox(interiorPoint);
        }

        var hull = hullCreation.Result;

        var points = hull.Points.Select(p => p.ToVector3()).ToArray();

        return points;
    }

    private static List<Vector3> FindIntersectionVertices(IList<Halfspace> halfspaces)
    {
        var pts = new List<Vector3>();
        int n = halfspaces.Count;

        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                for (int k = j + 1; k < n; k++)
                {
                    var p1 = halfspaces[i];
                    var p2 = halfspaces[j];
                    var p3 = halfspaces[k];

                    var c23 = p2.Normal.Cross(p3.Normal);
                    float denom = p1.Normal.Dot(c23);

                    if (Math.Abs(denom) < EPSILON)
                        continue;

                    var num = p1.D * c23
                            + p2.D * p3.Normal.Cross(p1.Normal)
                            + p3.D * p1.Normal.Cross(p2.Normal);

                    var X = num / denom;

                    if (halfspaces.Any(pl => pl.Normal.Dot(X) > pl.D + EPSILON))
                        continue;

                    pts.Add(X);
                }


        return pts;
    }

    private static Vector3[] CreateDefaultBox(Vector3 center, float size = 2.0f)
    {
        float halfSize = size * 0.5f;
        return new[] {
            center + new Vector3(-halfSize, -halfSize, -halfSize),
            center + new Vector3(-halfSize, -halfSize, halfSize),
            center + new Vector3(-halfSize, halfSize, -halfSize),
            center + new Vector3(-halfSize, halfSize, halfSize),
            center + new Vector3(halfSize, -halfSize, -halfSize),
            center + new Vector3(halfSize, -halfSize, halfSize),
            center + new Vector3(halfSize, halfSize, -halfSize),
            center + new Vector3(halfSize, halfSize, halfSize)
        };
    }

    private static Vector3 ConvertCollisionVectorToGodot(System.Numerics.Vector3 vector)
    {
        var transformed = CoordinateTransformer.TransformCollisionPosition(vector);
        return new Vector3(transformed.X, transformed.Y, transformed.Z);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenNova.Model;

internal class UserPoint
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int RotationX { get; set; }
    public int RotationY { get; set; }
    public int RotationZ { get; set; }
    public int SubObjectIndex { get; set; }
    public int UserPointType { get; set; }
    public string Name { get; set; } = "";

    public static List<UserPoint> FromNode(ModelNode node)
    {
        if (node.Identifier != "USRP") throw new ArgumentException("Node is not a USRP node");

        var points = new List<UserPoint>();

        using (var ms = new MemoryStream(node.Data))
        using (var reader = new BinaryReader(ms))
        {
            var recordCount = reader.ReadInt32();
            var recordSize = reader.ReadInt32();

            for (var i = 0; i < recordCount; i++)
            {
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                var z = reader.ReadInt32();

                var rotX = reader.ReadInt32();
                var rotY = reader.ReadInt32();
                var rotZ = reader.ReadInt32();

                var subObjectIndex = reader.ReadInt32();
                var userPointType = reader.ReadInt32();

                var nameBytes = reader.ReadBytes(16);
                var name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');

                points.Add(new UserPoint
                {
                    X = x,
                    Y = y,
                    Z = z,
                    RotationX = rotX,
                    RotationY = rotY,
                    RotationZ = rotZ,
                    SubObjectIndex = subObjectIndex,
                    UserPointType = userPointType,
                    Name = name
                });
            }
        }

        return points;
    }

    public byte[] ToBytes()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);

            writer.Write(RotationX);
            writer.Write(RotationY);
            writer.Write(RotationZ);

            writer.Write(SubObjectIndex);
            writer.Write(UserPointType);

            var nameBytes = Encoding.UTF8.GetBytes(Name.PadRight(16, '\0'));
            writer.Write(nameBytes, 0, Math.Min(nameBytes.Length, 16));

            var padding = 16 - Math.Min(nameBytes.Length, 16);
            if (padding > 0) writer.Write(new byte[padding]);

            return ms.ToArray();
        }
    }

    public static ModelNode ToNode(List<UserPoint> points)
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(points.Count);
            writer.Write(48);

            foreach (var point in points)
            {
                var pointData = point.ToBytes();
                writer.Write(pointData);
            }

            return new ModelNode("USRP", ms.ToArray());
        }
    }
}
using System.Numerics;

namespace OpenNova.Utils
{
    public static class CoordinateTransformer
    {
        internal static Vector3 TransformCollisionPosition(Vector3 min)
        {
            return new Vector3(
                min.Y,
                min.Z,
                min.X
            );
        }
    }
}
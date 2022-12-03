namespace FixterJail.Client.Extensions
{
    internal static class PositionExtensions
    {
        public static Vector3 AsVector(this Position position) => new Vector3(position.x, position.y, position.z);

        public static Vector3 FindClosestPoint(this Vector3 startingPoint, IEnumerable<Vector3> points)
        {
            if (points.Count() == 0) return Vector3.Zero;

            return points.OrderBy(x => Vector3.Distance(startingPoint, x)).First();
        }
        public static Vector3 FindClosestPoint(this Vector3 startingPoint, IEnumerable<Position> points)
        {
            if (points.Count() == 0) return Vector3.Zero;

            IEnumerable<Vector3> vectorPoints = points.Select(x => x.AsVector());

            return vectorPoints.OrderBy(x => Vector3.Distance(startingPoint, x)).First();
        }
    }
}

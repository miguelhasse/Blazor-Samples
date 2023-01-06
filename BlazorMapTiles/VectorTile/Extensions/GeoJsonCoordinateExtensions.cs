using System;
using System.Collections.Generic;
using System.Linq;

namespace Hasseware.VectorTile
{
    internal static class GeoJsonCoordinateExtensions
    {
        public static NetTopologySuite.Geometries.Coordinate Project(this NetTopologySuite.Geometries.Coordinate coordinate)
        {
            var sin = Math.Sin(coordinate.Y * Math.PI / 180);
            var y2 = 0.5 - 0.25 * Math.Log((1 + sin) / (1 - sin)) / Math.PI;

            var x = coordinate.X / 360 + 0.5;
            var y = Math.Max(Math.Min(y2, 1), 0);
            
            return new NetTopologySuite.Geometries.Coordinate(x, y);
        }

        public static IEnumerable<NetTopologySuite.Geometries.Coordinate> Project(this IEnumerable<NetTopologySuite.Geometries.Coordinate> coordinates, double tolerance)
        {
            var points = coordinates.Select(c => c.Project()).ToList();

            if (points.Count < 3)
            {
                return points;
            }

            var results = new List<NetTopologySuite.Geometries.Coordinate>();
            RamerDouglasPeucker(points, tolerance, results);

            return results;
        }

        private static void RamerDouglasPeucker(List<NetTopologySuite.Geometries.Coordinate> pointList, double tolerance, List<NetTopologySuite.Geometries.Coordinate> output)
        {
            if (pointList.Count < 2)
            {
                return;
            }

            // Find the point with the maximum distance from line between the start and end
            double dmax = 0.0;
            int index = 0;
            int end = pointList.Count - 1;

            for (int i = 1; i < end; ++i)
            {
                double d = PerpendicularDistance(pointList[i], pointList[0], pointList[end]);

                if (d > dmax)
                {
                    index = i;
                    dmax = d;
                }
            }

            // If max distance is greater than tolerance, recursively simplify
            if (dmax > tolerance)
            {
                var recResults1 = new List<NetTopologySuite.Geometries.Coordinate>();
                var recResults2 = new List<NetTopologySuite.Geometries.Coordinate>();
                var firstLine = pointList.Take(index + 1).ToList();
                var lastLine = pointList.Skip(index).ToList();

                RamerDouglasPeucker(firstLine, tolerance, recResults1);
                RamerDouglasPeucker(lastLine, tolerance, recResults2);

                // build the result list
                output.AddRange(recResults1.Take(recResults1.Count - 1));
                output.AddRange(recResults2);

                if (output.Count < 2)
                    throw new Exception("Problem assembling output");
            }
            else
            {
                // Just return start and end points
                output.Clear();
                output.Add(pointList[0]);
                output.Add(pointList[end]);
            }
        }

        /// <summary>
        /// The distance of a point from a line made from point1 and point2.
        /// </summary>
        public static double PerpendicularDistance(NetTopologySuite.Geometries.Coordinate point, NetTopologySuite.Geometries.Coordinate start, NetTopologySuite.Geometries.Coordinate end)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;

            // Normalize
            double mag = Math.Sqrt(dx * dx + dy * dy);

            if (mag > 0.0)
            {
                dx /= mag;
                dy /= mag;
            }
            double pvx = point.X - start.X;
            double pvy = point.Y - start.Y;

            // Get dot product (project pv onto normalized direction)
            double pvdot = dx * pvx + dy * pvy;

            // Scale line direction vector and subtract it from pv
            double ax = pvx - pvdot * dx;
            double ay = pvy - pvdot * dy;

            return Math.Sqrt(ax * ax + ay * ay);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Hasseware.VectorTile
{
    public static class GeoJsonFeatureExtensions
    {
        public static VectorTileLayer ToVectorTile(this FeatureCollection features, int x, int y, int z, uint extent = 4096, double buffer = 64, double tolerance = 3, string promoteId = null)
        {
            return new VectorTileLayer
            {
                Features = features.Select((feature, index) => feature.ToVectorTile(x, y, z, extent, buffer, tolerance, promoteId, index + 1)).Where(f => f.Geometry.Count > 0).ToList(),
                Extent = extent
            };
        }

        public static VectorTileFeature ToVectorTile(this IFeature feature, int x, int y, int z, uint extent = 4096, double buffer = 64, double tolerance = 3, string promoteId = null, int? index = null)
        {
            var id = (promoteId != null ? feature.Attributes[promoteId] : index ?? throw new ArgumentNullException(nameof(index))).ToString();

            var z2 = 1 << z;
            var p = buffer / extent;

            var type = DecodeGeometryType(feature.Geometry);
            var coordinates = Convert(feature.Geometry, tolerance / (z2 * extent));

            coordinates = Wrap(coordinates, type, p);
            coordinates = Clip(coordinates, type, (x - p) / z2, (x + 1 + p) / z2, -1, 2, false);
            coordinates = Clip(coordinates, type, (y - p) / z2, (y + 1 + p) / z2, -1, 2, true);

            var geometry = coordinates.Select(c1 => c1.Select(c2 => new Coordinate
            {
                X = (long)Math.Round((c2.X * z2 - x) * extent),
                Y = (long)Math.Round((c2.Y * z2 - y) * extent)
            })
            .ToList()).ToList();

            var attributes = feature.Attributes.GetNames().Where(attrib => attrib != promoteId)
                .Select(attrib => new KeyValuePair<string, object>(attrib, feature.Attributes[attrib]))
                .ToList();

            return new VectorTileFeature
            {
                Id = id,
                Type = type,
                Attributes = attributes,
                Geometry = geometry
            };
        }

        private static List<List<NetTopologySuite.Geometries.Coordinate>> Convert(Geometry geometry, double tolerance)
        {
            return (geometry is IEnumerable<Geometry> enumerable)
                ? enumerable.Select(g => g.Coordinates.Project(tolerance).ToList()).ToList()
                : new List<List<NetTopologySuite.Geometries.Coordinate>> { geometry.Coordinates.Project(tolerance).ToList() };
        }

        private static List<List<NetTopologySuite.Geometries.Coordinate>> Wrap(List<List<NetTopologySuite.Geometries.Coordinate>> coordinates, GeomType type, double buffer)
        {
            var merged = coordinates;

            var left = Clip(coordinates, type, -1 - buffer, buffer, -1, 2, false);
            var right = Clip(coordinates, type, 1 - buffer, 2 + buffer, -1, 2, false);

            if (left != null || right != null)
            {
                merged = Clip(coordinates, type, -buffer, 1 + buffer, -1, 2, false);

                if (left != null)
                {
                    merged = ShiftCoordinates(left, 1).Concat(merged).ToList();
                }

                if (right != null)
                {
                    merged = merged.Concat(ShiftCoordinates(right, -1)).ToList();
                }
            }

            return merged;
        }

        private static List<List<NetTopologySuite.Geometries.Coordinate>> Clip(List<List<NetTopologySuite.Geometries.Coordinate>> coordinates, GeomType type, double k1, double k2, double minAll, double maxAll, bool vertical)
        {
            var clipped = new List<List<NetTopologySuite.Geometries.Coordinate>>();

            if (coordinates == null || coordinates.Count == 0)
                return clipped;

            if (minAll >= k1 && maxAll < k2) // trivial accept
                return coordinates;

            if (maxAll < k1 || minAll >= k2) // trivial reject
                return clipped;

            var min = vertical ? coordinates.Min(c1 => c1.Min(c2 => c2.Y)) : coordinates.Min(c1 => c1.Min(c2 => c2.X));
            var max = vertical ? coordinates.Max(c1 => c1.Max(c2 => c2.Y)) : coordinates.Max(c1 => c1.Max(c2 => c2.X));

            if (min >= k1 && max < k2) // trivial accept
                return coordinates;

            if (max < k1 || min >= k2) // trivial reject
                return clipped;

            if (type == GeomType.Point)
            {
                var points = coordinates.Select(c => ClipPoints(c, k1, k2, vertical)).Where(c => c.Count() > 0);

                foreach (var coords in points)
                {
                    clipped.Add(coords.ToList());
                }
            }
            else
            {
                var lines = coordinates.SelectMany(c => ClipLine(c, k1, k2, vertical, type == GeomType.Polygon)).Where(l => l.Count() > 0);

                foreach (var coords in lines)
                {
                    clipped.Add(coords.ToList());
                }
            }

            return clipped;
        }

        private static IEnumerable<NetTopologySuite.Geometries.Coordinate> ClipPoints(List<NetTopologySuite.Geometries.Coordinate> coordinates, double k1, double k2, bool vertical)
        {
            return coordinates.Where(c =>
            {
                var a = vertical ? c.Y : c.X;
                return a >= k1 && a <= k2;
            });
        }

        private static IEnumerable<IEnumerable<NetTopologySuite.Geometries.Coordinate>> ClipLine(List<NetTopologySuite.Geometries.Coordinate> coordinates, double k1, double k2, bool vertical, bool polygon)
        {
            var count = coordinates.Count;
            var slice = new List<NetTopologySuite.Geometries.Coordinate>();
            bool exited = false;

            if (count < 2)
                yield break;

            for (int i = 0; i < count - 1; ++i)
            {
                var a = coordinates[i];
                var b = coordinates[i + 1];
                var ak = vertical ? a.Y : a.X;
                var bk = vertical ? b.Y : b.X;

                if (ak < k1)
                {
                    if (bk > k1) // ---|-->  | (line enters the clip region from the left)
                    {
                        slice.Add(Intersect(a, b, k1, vertical));
                    }
                }
                else if (ak > k2)
                {
                    if (bk < k2)// |  <--|--- (line enters the clip region from the right)
                    {
                        slice.Add(Intersect(a, b, k2, vertical));
                    }
                }
                else
                {
                    slice.Add(a);
                }

                if (bk < k1 && ak >= k1) // <--|---  | or <--|-----|--- (line exits the clip region on the left)
                {
                    slice.Add(Intersect(a, b, k1, vertical));
                    exited = true;
                }

                if (bk > k2 && ak <= k2) // |  ---|--> or ---|-----|--> (line exits the clip region on the right)
                {
                    slice.Add(Intersect(a, b, k2, vertical));
                    exited = true;
                }

                if (!polygon && exited)
                {
                    yield return slice;
                    slice = new List<NetTopologySuite.Geometries.Coordinate>();
                }
            }

            var l = coordinates[count - 1];
            var lk = vertical ? l.Y : l.X;

            if (lk >= k1 && lk <= k2)
            {
                slice.Add(l);
            }

            if (slice.Count > 0)
            {
                var fsc = slice[0];
                var lsc = slice[slice.Count - 1];

                if (polygon && (fsc.X != lsc.X || fsc.Y != lsc.Y))
                {
                    slice.Add(fsc);
                }

                yield return slice;
            }

            yield break;
        }

        private static NetTopologySuite.Geometries.Coordinate Intersect(NetTopologySuite.Geometries.Coordinate a, NetTopologySuite.Geometries.Coordinate b, double k, bool vertical)
        {
            return vertical
                ? new NetTopologySuite.Geometries.Coordinate(a.X + (b.X - a.X) * (k - a.Y) / (b.Y - a.Y), k)
                : new NetTopologySuite.Geometries.Coordinate(k, a.Y + (b.Y - a.Y) * (k - a.X) / (b.X - a.X));
        }

        private static List<List<NetTopologySuite.Geometries.Coordinate>> ShiftCoordinates(List<List<NetTopologySuite.Geometries.Coordinate>> coordinates, double offset)
        {
            return coordinates.Select(c1 => c1.Select(c2 => new NetTopologySuite.Geometries.Coordinate(c2.X + offset, c2.Y)).ToList()).ToList();
        }

        private static GeomType DecodeGeometryType(Geometry geometry)
        {
            if (geometry is IPuntal)
            {
                return GeomType.Point;
            }
            else if (geometry is ILineal)
            {
                return GeomType.LineString;
            }
            else if (geometry is IPolygonal)
            {
                return GeomType.Polygon;
            }

            throw new NotSupportedException(geometry.GeometryType);
        }
    }
}

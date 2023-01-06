using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Hasseware.VectorTile
{
    internal static class VectorTileFeatureExtensions
    {
        public static Feature ToGeoJSON(this VectorTileFeature vectortileFeature, int x, int y, int z)
        {
            var geometry = GetGeometry(vectortileFeature, x, y, z);

            var attributes = new AttributesTable(vectortileFeature.Attributes)
            {
                { "id", vectortileFeature.Id }
            };

            return new Feature(geometry, attributes);
        }

        private static Geometry GetGeometry(VectorTileFeature vectortileFeature, int x, int y, int z)
        {
            switch (vectortileFeature.Type)
            {
                case GeomType.Point: return GetPointGeometry(vectortileFeature.Geometry, x, y, z, vectortileFeature.Extent);
                case GeomType.LineString: return GetLineGeometry(vectortileFeature.Geometry, x, y, z, vectortileFeature.Extent);
                case GeomType.Polygon: return GetPolygonGeometry(Classify(vectortileFeature.Geometry), x, y, z, vectortileFeature.Extent);
                default: throw new NotSupportedException();
            }
        }

        private static Geometry GetPointGeometry(IEnumerable<IEnumerable<Coordinate>> geometry, int x, int y, int z, uint extent)
        {
            geometry = geometry.Where(g => g.Any());

            var single = geometry.Count() == 1;
            var points = geometry.Select(g => g.First().ToPosition(x, y, z, extent));

            return single
                ? (Geometry) new Point(points.First())
                : new MultiPoint(points.Select(p => new Point(p)).ToArray());
        }

        private static Geometry GetLineGeometry(IEnumerable<IEnumerable<Coordinate>> geometry, int x, int y, int z, uint extent)
        {
            geometry = geometry.Where(g => g.Any());

            var single = geometry.Count() == 1;
            var lines = geometry.Select(g => Project(g, x, y, z, extent));

            return single
                ? (Geometry) new LineString(lines.First().ToArray())
                : new MultiLineString(lines.Select(p => new LineString(p.ToArray())).ToArray());
        }

        private static Geometry GetPolygonGeometry(IEnumerable<IEnumerable<IEnumerable<Coordinate>>> geometry, int x, int y, int z, uint extent)
        {
            geometry = geometry.Where(g => g.Any(g1 => g1.Any()));

            var single = geometry.Count() == 1;
            var polygons = geometry.Select(g => g.Select(g1 => Project(g1, x, y, z, extent)));

            return single
                ? (Geometry) GetPolygon(polygons.First())
                : new MultiPolygon(polygons.Select(p => GetPolygon(p)).ToArray());
        }

        private static Polygon GetPolygon(IEnumerable<IEnumerable<NetTopologySuite.Geometries.Coordinate>> lines)
        {
            var rings = lines.Select(l => new LinearRing(l.ToArray())).Where(l => l.IsRing && l.IsClosed);
            return rings.Count() == 1 ? new Polygon(rings.First()) : new Polygon(rings.First(), rings.Skip(1).ToArray());
        }

        private static IEnumerable<NetTopologySuite.Geometries.Coordinate> Project(IEnumerable<Coordinate> coords, int x, int y, int z, uint extent)
        {
            return coords.Select(coord => coord.ToPosition(x, y, z, extent)).ToList();
        }

        // docs for inner/outer rings https://www.mapbox.com/vector-tiles/specification/
        private static IEnumerable<IEnumerable<IEnumerable<Coordinate>>> Classify(List<List<Coordinate>> rings)
        {
            var polygons = new List<List<List<Coordinate>>>();
            List<List<Coordinate>> newpoly = null;

            foreach (var ring in rings)
            {
                if (SignedArea(ring) > 0)
                {
                    newpoly = new List<List<Coordinate>>() { ring };
                    polygons.Add(newpoly);
                }
                else
                {
                    newpoly?.Add(ring);
                }
            }

            return polygons;
        }

        private static double SignedArea(IReadOnlyList<Coordinate> points)
        {
            double sum = 0.0;
            for (int i = 0; i < points.Count; ++i)
            {
                sum += points[i].X * (((i + 1) == points.Count) ? points[0].Y : points[i + 1].Y);
                sum -= points[i].Y * (((i + 1) == points.Count) ? points[0].X : points[i + 1].X);
            }
            return 0.5 * sum;
        }
    }
}

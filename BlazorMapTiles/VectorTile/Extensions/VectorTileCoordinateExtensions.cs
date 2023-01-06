using System;

namespace Hasseware.VectorTile
{
    internal static class VectorTileCoordinateExtensions
    {
        /// <summary>
        /// Pixel coordinate into a geospatial coordinate at a specified zoom level.
        /// </summary>
        public static NetTopologySuite.Geometries.Coordinate ToPosition(this Coordinate coordinate, int x, int y, int z, uint extent)
        {
            var size = extent * Math.Pow(2, z);
            var x0 = extent * x;
            var y0 = extent * y;

            var y2 = 180 - (coordinate.Y + y0) * 360 / size;

            var lon = (coordinate.X + x0) * 360 / size - 180;
            var lat = 360 / Math.PI * Math.Atan(Math.Exp(y2 * Math.PI / 180)) - 90;

            return new NetTopologySuite.Geometries.Coordinate(lat, lon);
        }
    }
}

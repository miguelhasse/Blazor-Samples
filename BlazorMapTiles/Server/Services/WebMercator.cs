using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlazorMapTiles.Services
{
    /// <summary>
    /// Various utility functions for EPSG:3857 / Web Mercator SRS and tile system.
    /// </summary>
    internal static class WebMercator
    {
        /* Assumes SRS = EPSG:3857 / Web Mercator / Spherical Mercator
        ** Based on https://docs.microsoft.com/en-us/bingmaps/articles/bing-maps-tile-system
        ** and https://docs.microsoft.com/en-us/azure/azure-maps/zoom-levels-and-tile-grid
        */
        public const int TileSize = 256;

        private const double EarthRadius = 6378137.0;
        private const double MinLatitude = -85.05112878;
        private const double MaxLatitude = 85.05112878;
        private const double MinLongitude = -180;
        private const double MaxLongitude = 180;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Longitude(double x)
        {
            return x / (EarthRadius * Math.PI / 180.0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Latitude(double y)
        {
            var fi = (2.0 * Math.Atan(Math.Exp(y / EarthRadius))) - (Math.PI / 2.0);
            return RadiansToDegrees(fi);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double X(double longitude)
        {
            return EarthRadius * DegreesToRadians(longitude);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Y(double latitude)
        {
            var lat = Math.Max(Math.Min(MaxLatitude, latitude), -MaxLatitude);
            return EarthRadius * Artanh(Math.Sin(DegreesToRadians(lat)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double TileXtoEpsg3857X(int tileX, int zoomLevel)
        {
            var mapSize = (double)MapSize(zoomLevel);
            var pixelX = tileX * TileSize;
            var x = (Clip(pixelX, 0.0, mapSize) / mapSize) - 0.5;
            var longitude = 360.0 * x;

            return EarthRadius * DegreesToRadians(longitude);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double TileYtoEpsg3857Y(int tileY, int zoomLevel)
        {
            var mapSize = (double)MapSize(zoomLevel);
            var pixelY = tileY * TileSize;
            var y = 0.5 - (Clip(pixelY, 0.0, mapSize) / mapSize);
            var latitude = 90.0 - (360.0 * Math.Atan(Math.Exp(-y * 2.0 * Math.PI)) / Math.PI);

            return EarthRadius * Artanh(Math.Sin(DegreesToRadians(latitude)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TileCount(int zoomLevel)
        {
            return 1 << zoomLevel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MapSize(int zoomLevel)
        {
            return TileSize << zoomLevel;
        }

        /// <summary>
        /// Calculates width and height of the map in pixels at a specific zoom level from -180 degrees to 180 degrees.
        /// </summary>
        /// <param name="zoom">Zoom Level to calculate width at</param>
        /// <param name="tileSize">The size of the tiles in the tile pyramid.</param>
        /// <returns>Width and height of the map in pixels</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MapSize(double zoom, int tileSize)
        {
            return Math.Ceiling(tileSize * Math.Pow(2, zoom));
        }

        /// <summary>
        /// Flips tile Y coordinate (according to XYZ/TMS coordinate systems conversion).
        /// </summary>
        /// <param name="y">Tile Y coordinate.</param>
        /// <param name="zoomLevel">Tile zoom level.</param>
        /// <returns>Flipped tile Y coordinate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FlipYCoordinate(int y, int zoomLevel)
        {
            return (1 << zoomLevel) - y - 1;
        }

        public static double TileCoordinateXAtZoom(double longitude, int zoomLevel)
        {
            return LongitudeToPixelXAtZoom(longitude, zoomLevel) / (double)TileSize;
        }

        public static double TileCoordinateYAtZoom(double latitude, int zoomLevel)
        {
            return LatitudeToPixelYAtZoom(latitude, zoomLevel) / (double)TileSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LongitudeToPixelXAtZoom(double longitude, int zoomLevel)
        {
            return LongitudeToPixelX(longitude, (double)MapSize(zoomLevel));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LatitudeToPixelYAtZoom(double latitude, int zoomLevel)
        {
            return LatitudeToPixelY(latitude, (double)MapSize(zoomLevel));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LongitudeToPixelX(double longitude, double mapSize)
        {
            return ((longitude + 180.0) / 360.0) * mapSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LatitudeToPixelY(double latitude, double mapSize)
        {
            var sinLatitude = Math.Sin(DegreesToRadians(latitude));
            return (0.5 - (Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * Math.PI))) * mapSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double PixelXToLongitude(double pixelX, int zoomLevel)
        {
            var mapSize = (double)MapSize(zoomLevel);
            var x = (Clip(pixelX, 0.0, mapSize) / mapSize) - 0.5;

            return 360.0 * x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double PixelYToLatitude(double pixelY, int zoomLevel)
        {
            var mapSize = (double)MapSize(zoomLevel);
            var y = 0.5 - (Clip(pixelY, 0.0, mapSize) / mapSize);

            return 90.0 - (360.0 * Math.Atan(Math.Exp(-y * 2.0 * Math.PI)) / Math.PI);
        }

        /// <summary>
        /// Global Converts a Pixel coordinate into a geospatial coordinate at a specified zoom level. 
        /// Global Pixel coordinates are relative to the top left corner of the map (90, -180)
        /// </summary>
        /// <param name="pixelX">Pixel coordinate for X.</param>  
        /// <param name="pixelY">Pixel coordinate for Y.</param>  
        /// <param name="zoom">Zoom level</param>
        /// <param name="tileSize">The size of the tiles in the tile pyramid.</param>
        public static void GlobalPixelToPosition(double pixelX, double pixelY, double zoom, int tileSize, out double longitude, out double latitude)
        {
            var mapSize = MapSize(zoom, tileSize);

            var x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
            var y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);

            longitude = 360 * x;
            latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
        }

        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees) into pixel XY coordinates at a specified level of detail.
        /// </summary>
        /// <param name="position">Position coordinate in the format [longitude, latitude]</param>
        /// <param name="zoom">Zoom level.</param>
        /// <param name="tileSize">The size of the tiles in the tile pyramid.</param> 
        public static void PositionToGlobalPixel(double longitude, double latitude, int zoom, int tileSize, out double pixelX, out double pixelY)
        {
            latitude = Clip(latitude, MinLatitude, MaxLatitude);
            longitude = Clip(longitude, MinLongitude, MaxLongitude);

            var x = (longitude + 180) / 360;
            var sinLatitude = Math.Sin(latitude * Math.PI / 180);
            var y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            var mapSize = MapSize(zoom, tileSize);

            pixelX = Clip(x * mapSize + 0.5, 0, mapSize - 1);
            pixelY = Clip(y * mapSize + 0.5, 0, mapSize - 1);
        }

        /// <summary>
        /// Calculates the bounding box of a tile.
        /// </summary>
        /// <param name="tileX">Tile X coordinate</param>
        /// <param name="tileY">Tile Y coordinate</param>
        /// <param name="zoom">Zoom level</param>
        /// <param name="tileSize">The size of the tiles in the tile pyramid.</param>
        /// <returns>A bounding box of the tile defined as an array of numbers in the format of [west, south, east, north].</returns>
        public static double[] TileXYToBoundingBox(int tileX, int tileY, double zoom, int tileSize)
        {
            //Top left corner pixel coordinates
            var x1 = (double)(tileX * tileSize);
            var y1 = (double)(tileY * tileSize);

            //Bottom right corner pixel coordinates
            var x2 = (double)(x1 + tileSize);
            var y2 = (double)(y1 + tileSize);

            GlobalPixelToPosition(x1, y1, zoom, tileSize, out var nwx, out var nwy);
            GlobalPixelToPosition(x2, y2, zoom, tileSize, out var sex, out var sey);

            return new double[] { nwx, sey, sex, nwy };
        }

        /// <summary>
        /// Converts tile XY coordinates into a quadkey at a specified level of detail.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>A string containing the quadkey.</returns>
        public static string TileXYToQuadKey(int tileX, int tileY, int zoom)
        {
            var quadKey = new StringBuilder();

            for (int i = zoom; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);

                if ((tileX & mask) != 0)
                {
                    digit++;
                }

                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }

                quadKey.Append(digit);
            }

            return quadKey.ToString();
        }

        /// <summary>
        /// Converts a quadkey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">Quadkey of the tile.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        /// <param name="zoom">Output parameter receiving the zoom level.</param>
        public static bool QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int zoom)
        {
            tileX = tileY = 0;
            zoom = quadKey.Length;

            for (int i = zoom; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                switch (quadKey[zoom - i])
                {
                    case '0':
                        break;

                    case '1':
                        tileX |= mask;
                        break;

                    case '2':
                        tileY |= mask;
                        break;

                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;

                    default:
                        return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Clip(double value, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(value, minValue), maxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Artanh(double x)
        {
            // https://en.wikipedia.org/wiki/Inverse_hyperbolic_function#Inverse_hyperbolic_tangent
            return 0.5 * Math.Log((1.0 + x) / (1.0 - x));
        }
    }
}

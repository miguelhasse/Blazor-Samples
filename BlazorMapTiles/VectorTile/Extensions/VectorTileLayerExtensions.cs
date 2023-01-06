using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Hasseware.VectorTile
{
	public static class VectorTileLayerExtensions
	{
		public static FeatureCollection ToGeoJSON(this VectorTileLayer vectortileLayer, int x, int y, int z)
		{
		    var featureCollection = new FeatureCollection();
            var bb = featureCollection.BoundingBox = new Envelope();

            foreach (var feature in vectortileLayer.Features)
            {
                var geojsonFeature = feature.ToGeoJSON(x,y,z);

                if (geojsonFeature.Geometry != null)
                {
                    featureCollection.Add(geojsonFeature);

                    foreach (var coordinate in geojsonFeature.Geometry.Coordinates)
                    {
                        bb.ExpandToInclude(coordinate);
                    }
                }
            }

		    return featureCollection;
		}
    }
}


using System.Collections.Generic;

namespace Hasseware.VectorTile
{
	public class VectorTileFeature
	{
		public string Id { get; set; }

		public List<List<Coordinate>> Geometry {get;set;}

		public List<KeyValuePair<string, object>> Attributes { get; set; }

		public GeomType Type { get; set; }

		public uint Extent { get; set; }
	}
}


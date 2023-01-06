using System.Collections.Generic;
using System.IO;

namespace Hasseware.VectorTile
{
	public class VectorTileLayer
	{
		public IEnumerable<VectorTileFeature> Features { get; set; }

		public string Name { get; set; }

		public uint Version { get; set; }

		public uint Extent { get;  set; }

        public static void Encode(Stream stream, IEnumerable<VectorTileLayer> layers) => Encoder.Encode(stream, layers);

		public static IEnumerable<VectorTileLayer> Decode(Stream stream) => Decoder.Decode(stream);
	}
}


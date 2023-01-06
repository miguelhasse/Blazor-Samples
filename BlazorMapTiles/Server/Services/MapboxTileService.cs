using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazorMapTiles.Storage;
using Hasseware.VectorTile;

namespace BlazorMapTiles.Services
{
    internal sealed class MapboxTileService : IVectorTileService
    {
        private readonly MapboxTilesRepository _repository;
        private static readonly VectorTileLayer Empty = new () { Name = "Empty", Features = Enumerable.Empty<VectorTileFeature>() };

        public MapboxTileService(MapboxTilesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<VectorTileLayer>> GetVectorTile(IEnumerable<string> layers, int x, int y, int z, CancellationToken cancellationToken)
        {
            var tileData = await this._repository.GetTileData(x, WebMercator.FlipYCoordinate(y, z), z);

            if (tileData != null && !cancellationToken.IsCancellationRequested)
            {
                using var zipStream = new GZipStream(tileData, CompressionMode.Decompress);
                var tileLayers = VectorTileLayer.Decode(zipStream);

                if (layers != null && layers.Any())
                {
                    tileLayers = tileLayers.Where(s => layers.Contains(s.Name, StringComparer.OrdinalIgnoreCase));
                }

                return tileLayers.ToList()
                    .GroupBy(s => s.Name.ToLower())
                    .Select(g => new VectorTileLayer
                    {
                        Name = g.Key,
                        Extent = 4096,
                        Version = 2,
                        Features = g.SelectMany(s => s.Features)
                    });
            }

            return Enumerable.Empty<VectorTileLayer>();
        }
    }
}

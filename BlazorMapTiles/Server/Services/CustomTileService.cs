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
    internal sealed class CustomTileService : IVectorTileService
    {
        private readonly CustomTilesRepository _repository;

        public CustomTileService(CustomTilesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<VectorTileLayer>> GetVectorTile(IEnumerable<string> layers, int x, int y, int z, CancellationToken cancellationToken)
        {
            var tileLayers = (await this._repository.GetTileData(x, WebMercator.FlipYCoordinate(y, z), z, cancellationToken: cancellationToken))
                .SelectMany(stream =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Enumerable.Empty<VectorTileLayer>();

                    using var zipStream = new GZipStream(stream, CompressionMode.Decompress);
                    return VectorTileLayer.Decode(zipStream);
                });

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
    }
}

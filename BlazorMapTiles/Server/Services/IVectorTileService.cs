using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hasseware.VectorTile;

namespace BlazorMapTiles.Services
{
    public interface IVectorTileService
    {
        Task<IEnumerable<VectorTileLayer>> GetVectorTile(IEnumerable<string> layers, int x, int y, int z, CancellationToken cancellationToken = default);
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BlazorMapTiles.Services;
using Hasseware.VectorTile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlazorMapTiles.Server.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("Tiles")]
    public class TilesController : Controller
    {
        private readonly IVectorTileService _vectorTileService;
        private readonly ILogger<TilesController> _logger;

        public TilesController(IVectorTileService vectorTileService, ILogger<TilesController> logger)
        {
            this._vectorTileService = vectorTileService ?? throw new ArgumentNullException(nameof(vectorTileService));
            this._logger = logger;
        }

        [HttpGet("{zoom}/{x}/{y}.{format}")]
        [Produces("image/pbf", "image/png")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTile([FromQuery(Name = "layer")] IEnumerable<string> layers, string format, int x, int y, int zoom, CancellationToken cancellationToken)
        {
            if (ValidTile(x, y, zoom))
            {

                if (string.Equals(format, "pbf", StringComparison.OrdinalIgnoreCase))
                {
                    return await GetTilePBF(layers, x, y, zoom, cancellationToken);
                }

                if (string.Equals(format, "png", StringComparison.OrdinalIgnoreCase))
                {
                    return await GetTilePNG(layers, x, y, zoom, cancellationToken);
                }
            }

            return BadRequest();
        }

        [HttpGet("{layer}/{quadkey}.{format}")]
        [Produces("image/pbf", "image/png")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByQuadKey(IEnumerable<string> layers, string format, string quadkey, CancellationToken cancellationToken)
        {
            if (WebMercator.QuadKeyToTileXY(quadkey, out int x, out int y, out int zoom))
            {
                return await GetTile(layers, format, x, y, zoom, cancellationToken);
            }

            return BadRequest("Invalid QuadKey digit sequence.");
        }

        private async Task<IActionResult> GetTilePBF(IEnumerable<string> layers, int x, int y, int zoom, CancellationToken cancellationToken)
        {
            var vtl = await _vectorTileService.GetVectorTile(layers, x, y, zoom, cancellationToken);

            Response.Headers.Expires = DateTime.UtcNow.AddMinutes(1)
                .ToString("r", CultureInfo.InvariantCulture);

            if (vtl.Any() && vtl.Any(l => l.Features.Any(f => f.Geometry.Count > 0)))
            {
                using var stream = new MemoryStream();
                VectorTileLayer.Encode(stream, vtl);
                stream.Position = 0;

                var content = stream.GetBuffer();

                if (content != null)
                {
                    var etag = GenerateEtag(content);

                    if (Request.Headers.IfNoneMatch == etag)
                    {
                        return StatusCode(StatusCodes.Status304NotModified);
                    }

                    Response.Headers.ETag = etag;

                    return File(content, "image/pbf", false);
                }
            }

            return NoContent();
        }

        private Task<IActionResult> GetTilePNG(IEnumerable<string> layers, int x, int y, int zoom, CancellationToken cancellationToken)
        {
            return Task.FromResult((IActionResult)NoContent());
        }

        private static string GenerateEtag(byte[] content)
        {
            using var crypto = MD5.Create();
            var checksum = string.Concat(Array.ConvertAll(crypto.ComputeHash(content), x => x.ToString("x2")));
            return $"W/\"{checksum}\"";
        }

        private static bool ValidTile(int x, int y, int zoom)
        {
            if (x >= 0 && y >= 0 && zoom >= 0 && zoom <= 24)
            {
                var tileCount = Math.Pow(2, zoom);
                return x < tileCount && y < tileCount;
            }

            return false;
        }
    }
}

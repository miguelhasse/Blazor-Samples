using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BlazorMapTiles.Storage
{
    public sealed class MapboxTilesRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapboxTilesRepository"/> class.
        /// </summary>
        /// <param name="path">Full path to MBTiles database file.</param>
        /// <param name="fullAccess">Allows database modification if true.</param>
        public MapboxTilesRepository(string path)
        {
            this._connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,
            }
            .ToString();
        }

        /// <summary>
        /// Reads all metadata key/value items from database.
        /// </summary>
        /// <returns>Metadata records.</returns>
        public async Task<IDictionary<string, string>> GetMetadata(CancellationToken cancellationToken = default)
        {
            using var connection = new SqliteConnection(this._connectionString);
            using var command = new SqliteCommand("SELECT name, value FROM metadata", connection);

            try
            {
                await connection.OpenAsync(cancellationToken);
                using var dr = await command.ExecuteReaderAsync(cancellationToken);
                var result = new Dictionary<string, string>();

                while (await dr.ReadAsync(cancellationToken))
                {
                    if (!dr.IsDBNull(0) && !dr.IsDBNull(1))
                    {
                        result.Add(dr.GetString(0), dr.GetString(1));
                    }
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Reads tile image contents with given coordinates from database.
        /// </summary>
        /// <param name="column">Tile X coordinate (column).</param>
        /// <param name="row">Tile Y coordinate (row), Y axis goes up from the bottom (TMS scheme).</param>
        /// <param name="zoomLevel">Tile Z coordinate (zoom level).</param>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/async">Async Limitations</seealso>
        /// <returns>Tile image contents.</returns>
        public async Task<Stream> GetTileData(int column, int row, int zoomLevel, CancellationToken cancellationToken = default)
        {
            using var connection = new SqliteConnection(this._connectionString);
            using var command = new SqliteCommand("SELECT tile_data FROM tiles WHERE ((zoom_level = @zoom) AND (tile_column = @column) AND (tile_row = @row))", connection);
            
            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@column", column),
                new SqliteParameter("@row", row),
                new SqliteParameter("@zoom", zoomLevel),
            });

            try
            {
                await connection.OpenAsync(cancellationToken);
                using var dr = await command.ExecuteReaderAsync(cancellationToken);
                return await dr.ReadAsync(cancellationToken) ? dr.GetStream(0) : null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}

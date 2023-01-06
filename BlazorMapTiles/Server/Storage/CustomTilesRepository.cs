using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BlazorMapTiles.Storage
{
    public sealed class CustomTilesRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTilesRepository"/> class.
        /// </summary>
        /// <param name="path">Full path to MBTiles database file.</param>
        public CustomTilesRepository(string path)
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
        public async Task<IEnumerable<(int Country, string Name, string Value)>> GetMetadata(CancellationToken cancellationToken = default)
        {
            using var connection = new SqliteConnection(this._connectionString);
            using var command = new SqliteCommand("SELECT Country, Name, Value FROM Metadata", connection);

            try
            {
                await connection.OpenAsync(cancellationToken);
                using var dr = await command.ExecuteReaderAsync(cancellationToken);
                var result = new List<(int, string, string)>();

                while (await dr.ReadAsync(cancellationToken))
                {
                    if (!dr.IsDBNull(0) && !dr.IsDBNull(1))
                    {
                        result.Add(new(dr.GetInt16(0), dr.GetString(1), dr.GetString(2)));
                    }
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                return Enumerable.Empty<(int, string, string)>();
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
        public async Task<IEnumerable<Stream>> GetTileData(int column, int row, int zoomLevel, IEnumerable<int> countries = null, CancellationToken cancellationToken = default)
        {
            var commandBuilder = new StringBuilder("SELECT Data FROM Tiles WHERE (ZoomLevel = @zoom) AND (Column = @column) AND (Row = @row)");

            if (countries != null)
            {
                commandBuilder.Append(" AND (Country IN (");
                commandBuilder.AppendJoin(',', countries.Select((s, n) => $"@country_{n}"));
                commandBuilder.Append("))");
            }

            using var connection = new SqliteConnection(this._connectionString);
            using var command = new SqliteCommand(commandBuilder.ToString(), connection);

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@column", column),
                new SqliteParameter("@row", row),
                new SqliteParameter("@zoom", zoomLevel),
            });

            if (countries != null)
            {
                command.Parameters.AddRange(countries.Select((s, n) => new SqliteParameter($"@country_{n}", s)));
            }

            try
            {
                await connection.OpenAsync(cancellationToken);
                using var dr = await command.ExecuteReaderAsync(cancellationToken);
                var result = new List<Stream>();

                while (await dr.ReadAsync(cancellationToken))
                {
                    var stream = dr.GetStream(0);

                    if (stream.CanRead)
                    {
                        result.Add(stream);
                    }
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                return Enumerable.Empty<Stream>();
            }
        }
    }
}

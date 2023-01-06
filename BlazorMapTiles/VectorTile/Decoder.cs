using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using ProtoBuf;

namespace Hasseware.VectorTile
{
    internal static class Decoder
    {
        public static IEnumerable<VectorTileLayer> Decode(Stream stream)
        {
            var tile = Serializer.Deserialize<Contracts.Tile>(stream);

            return tile.Layers.Select(layer =>
            {
                var vectorTileFeatures = layer.Features.Select(feature => new VectorTileFeature
                {
                    Id = feature.Id.ToString(CultureInfo.InvariantCulture),
                    Attributes = DecodeAttributes(layer.Keys, layer.Values, feature.Tags),
                    Type = ConvertFeatureType(feature.Type),
                    Geometry = DecodeGeometry(feature.Geometry, feature.Type),
                    Extent = layer.Extent,
                });

                return new VectorTileLayer
                {
                    Name = layer.Name,
                    Features = vectorTileFeatures,
                    Version = layer.Version,
                    Extent = layer.Extent
                };
            });
        }

        private static List<List<Coordinate>> DecodeGeometry(List<uint> commands, Contracts.GeomType type)
        {
            const uint cmdMoveTo = 1;
            const uint cmdLineTo = 2;
            const uint cmdSegEnd = 7;

            var coordinateList = new List<List<Coordinate>>();
            var coordinates = new List<Coordinate>();

            long x = 0;
            long y = 0;
            int count = commands.Count;

            for (int i = 0; i < count; i++)
            {
                uint g = commands[i];
                uint command = g & 0x7;
                uint length = g >> 3;

                if (command == cmdMoveTo || command == cmdLineTo)
                {
                    for (int j = 0; j < length; j++)
                    {
                        x += ZigZag.Decode(commands[i + 1]);
                        y += ZigZag.Decode(commands[i + 2]);
                        i += 2;

                        if (command == cmdMoveTo && coordinates.Count > 0)
                        {
                            coordinateList.Add(coordinates);
                            coordinates = new List<Coordinate>();
                        }

                        coordinates.Add(new Coordinate { X = x, Y = y });
                    }
                }

                if (command == cmdSegEnd)
                {
                    if (type != Contracts.GeomType.Point && coordinates.Count > 0)
                    {
                        coordinates.Add(coordinates[0]);
                    }
                }
            }

            if (coordinates.Count > 0)
            {
                coordinateList.Add(coordinates);
            }

            return coordinateList;
        }

        private static List<KeyValuePair<string, object>> DecodeAttributes(List<string> keys, List<Contracts.Value> values, List<uint> tags)
        {
            var result = new List<KeyValuePair<string, object>>();
            var odds = tags.GetOdds().ToList();
            var evens = tags.GetEvens().ToList();

            for (var i = 0; i < evens.Count; i++)
            {
                var key = keys[(int)evens[i]];
                var val = values[(int)odds[i]];

                result.Add(new KeyValuePair<string, object>(key, ConvertToAttribute(val)));
            }

            return result;
        }

        private static GeomType ConvertFeatureType(Contracts.GeomType type)
        {
            switch (type)
            {
                case Contracts.GeomType.Point: return GeomType.Point;
                case Contracts.GeomType.LineString: return GeomType.LineString;
                case Contracts.GeomType.Polygon: return GeomType.Polygon;
                default: throw new NotSupportedException();
            }
        }

        private static object ConvertToAttribute(Contracts.Value value)
        {
            if (value.HasBoolValue)
            {
                return value.BoolValue;
            }
            else if (value.HasDoubleValue)
            {
                return value.DoubleValue;
            }
            else if (value.HasFloatValue)
            {
                return value.FloatValue;
            }
            else if (value.HasIntValue)
            {
                return value.IntValue;
            }
            else if (value.HasStringValue)
            {
                return value.StringValue;
            }
            else if (value.HasSIntValue)
            {
                return value.SintValue;
            }
            else if (value.HasUIntValue)
            {
                return value.UintValue;
            }

            return null;
        }
    }
}

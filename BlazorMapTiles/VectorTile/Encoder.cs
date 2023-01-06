using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Hasseware.VectorTile
{
    internal static class Encoder
    {
        public static void Encode(Stream stream, IEnumerable<VectorTileLayer> layers)
        {
            var tile = new Contracts.Tile();
            ulong reference = 0;

            foreach (var sourceLayer in layers)
            {
                var targetLayer = new Contracts.Layer
                {
                    Name = sourceLayer.Name,
                    Version = sourceLayer.Version,
                    Extent = sourceLayer.Extent
                };

                tile.Layers.Add(targetLayer);

                foreach (var sourceFeature in sourceLayer.Features)
                {
                    var targetFeature = new Contracts.Feature
                    {
                        Id = ulong.TryParse(sourceFeature.Id, out ulong id) ? id : ++reference,
                        Type = ConvertFeatureType(sourceFeature.Type),
                    };

                    targetLayer.Features.Add(targetFeature);
                    EncodeAttributes(sourceFeature, targetFeature, targetLayer);
                    EncodeGeometry(sourceFeature, targetFeature);
                }
            }

            Serializer.Serialize(stream, tile);
        }

        private static void EncodeGeometry(VectorTileFeature source, Contracts.Feature target)
        {
            long x = 0;
            long y = 0;
            int rings = source.Geometry.Count;
            
            for (int r = 0; r < rings; r++)
            {
                var ring = source.Geometry[r];
                int count = source.Type == GeomType.Point ? ring.Count : 1;
                int lineCount = source.Type == GeomType.Polygon ? ring.Count - 1 : ring.Count;

                target.Geometry.Add(((uint)count << 3) + ((uint)1 & 0x7));

                for (int i = 0; i < lineCount; i++)
                {
                    if (i == 1 && source.Type != GeomType.Point)
                    {
                        target.Geometry.Add(((uint)lineCount - 1 << 3) + ((uint)2 & 0x7));
                    }

                    long dx = ring[i].X - x;
                    long dy = ring[i].Y - y;

                    target.Geometry.Add((uint)ZigZag.Encode(dx));
                    target.Geometry.Add((uint)ZigZag.Encode(dy));

                    x += dx;
                    y += dy;
                }

                if (source.Type == GeomType.Polygon)
                {
                    target.Geometry.Add(((uint)1 << 3) + ((uint)7 & 0x7));
                }
            }
        }

        private static void EncodeAttributes(VectorTileFeature source, Contracts.Feature target, Contracts.Layer layer)
        {
            foreach (var attrib in source.Attributes)
            {
                int keypos = layer.Keys.FindIndex(key => attrib.Key == key);

                if (keypos < 0)
                {
                    keypos = layer.Keys.Count;
                    layer.Keys.Add(attrib.Key);
                }

                target.Tags.Add((uint)keypos);

                var value = ConvertFromAttribute(attrib.Value);
                int valpos = layer.Values.FindIndex(val =>
                    (value.HasBoolValue && value.HasBoolValue == val.HasBoolValue && value.BoolValue == val.BoolValue)
                    || (value.HasDoubleValue && value.HasDoubleValue == val.HasDoubleValue && value.DoubleValue == val.DoubleValue)
                    || (value.HasFloatValue && value.HasFloatValue == val.HasFloatValue && value.FloatValue == val.FloatValue)
                    || (value.HasIntValue && value.HasIntValue == val.HasIntValue && value.IntValue == val.IntValue)
                    || (value.HasSIntValue && value.HasSIntValue == val.HasSIntValue && value.SintValue == val.SintValue)
                    || (value.HasStringValue && value.HasStringValue == val.HasStringValue && value.StringValue == val.StringValue)
                    || (value.HasUIntValue && value.HasUIntValue == val.HasUIntValue && value.UintValue == val.UintValue));

                if (valpos < 0)
                {
                    valpos = layer.Values.Count;
                    layer.Values.Add(value);
                }

                target.Tags.Add((uint)valpos);
            }
        }

        private static Contracts.GeomType ConvertFeatureType(GeomType type)
        {
            switch (type)
            {
                case GeomType.Point: return Contracts.GeomType.Point;
                case GeomType.LineString: return Contracts.GeomType.LineString;
                case GeomType.Polygon: return Contracts.GeomType.Polygon;
                default: return Contracts.GeomType.Unknown;
            }
        }

        private static Contracts.Value ConvertFromAttribute(object attrib)
        {
            if (attrib != null)
            {
                switch (Type.GetTypeCode(attrib.GetType()))
                {
                    case TypeCode.Boolean:
                        return new Contracts.Value
                        {
                            HasBoolValue = true,
                            BoolValue = (bool)attrib
                        };
                    case TypeCode.String:
                        return new Contracts.Value
                        {
                            HasStringValue = true,
                            StringValue = (string)attrib
                        };
                    case TypeCode.Single:
                        return new Contracts.Value
                        {
                            HasFloatValue = true,
                            FloatValue = (float)attrib
                        };
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                        return new Contracts.Value
                        {
                            HasDoubleValue = true,
                            DoubleValue = (double)attrib
                        };
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        return new Contracts.Value
                        {
                            HasIntValue = true,
                            IntValue = (long)attrib
                        };
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return new Contracts.Value
                        {
                            HasUIntValue = true,
                            UintValue = (ulong)attrib
                        };
                }
            }

            return null;
        }
    }
}

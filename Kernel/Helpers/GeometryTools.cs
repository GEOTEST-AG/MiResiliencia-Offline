using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace ResTB_API.Helpers
{
    public class GeometryTools
    {
        /// <summary>
        /// Convert hex-coded WKB to geometry object
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static IGeometry WKBhex2Geom(string hexString)
        {
            if (String.IsNullOrWhiteSpace(hexString))
                return null;

            try
            {
                var reader = new NetTopologySuite.IO.WKBReader();
                var geometryBinary = NetTopologySuite.IO.WKBReader.HexToBytes(hexString);
                IGeometry result = reader.Read(geometryBinary);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string Geom2WKBhex(IGeometry geometry)
        {
            // writing WKB (Well Known Binary)
            var writer = new NetTopologySuite.IO.WKBWriter(GeoAPI.IO.ByteOrder.BigEndian, true);

            var wkbBin = writer.Write(geometry);

            // writing hex-coded WKB, used in POSTGIS DB geometery column
            string hexString = NetTopologySuite.IO.WKBWriter.ToHex(wkbBin);

            return hexString;
        }

        public static string Geom2GeoJSON(IGeometry geometry)
        {
            var geoWriter = new GeoJsonWriter();

            var geoJSON = geoWriter.Write(geometry);

            return geoJSON;
        }

        /// <summary>
        /// CONVERSION of geometry: Polygon to Multipolygon
        /// </summary>
        public static IMultiPolygon Polygon2Multipolygon(IGeometry polygon)
        {
            if (polygon is Polygon)
            {
                var factory = new GeometryFactory();
                Polygon[] polygons = new Polygon[] { polygon as Polygon };
                IMultiPolygon multiPolygon = factory.CreateMultiPolygon(polygons);

                return multiPolygon;
            }
            else if (polygon is MultiPolygon)
            {
                return (IMultiPolygon)polygon;
            }
            else
            {
                throw new ArgumentException($"Error: GeometryType not supported! input is of type {polygon.GeometryType}");
            }        
        }
    }
}
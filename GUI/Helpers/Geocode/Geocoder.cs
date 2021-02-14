using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ResTB.GUI.Helpers.Geocode
{
    public class Place
    {
        //geonameid         : integer id of record in geonames database
        //name              : name of geographical point(utf8) varchar(200)
        //asciiname         : name of geographical point in plain ascii characters, varchar(200)
        //alternatenames    : alternatenames, comma separated, ascii names automatically transliterated, convenience attribute from alternatename table, varchar(10000)
        //latitude          : latitude in decimal degrees(wgs84)
        //longitude         : longitude in decimal degrees(wgs84)
        //feature class     : see http://www.geonames.org/export/codes.html, char(1)
        //feature code      : see http://www.geonames.org/export/codes.html, varchar(10)
        //country code      : ISO-3166 2-letter country code, 2 characters

        public int geonameid { get; set; }
        public string name { get; set; }
        public string asciiname { get; set; }
        public string alternatenames { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string featureclass { get; set; }
        public string featurecode { get; set; }
        public string countrycode { get; set; }

        public static Place FromCsv(string csvLine)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            string[] values = csvLine.Split('\t');
            Place place = new Place();
            place.geonameid = Convert.ToInt32(values[0]);
            place.name = Convert.ToString(values[1]);
            place.asciiname = Convert.ToString(values[2]);
            place.alternatenames = Convert.ToString(values[3]);
            place.latitude = Convert.ToDouble(values[4], nfi);
            place.longitude = Convert.ToDouble(values[5], nfi);
            place.featureclass = Convert.ToString(values[6]);
            place.featurecode = Convert.ToString(values[7]);
            place.countrycode = Convert.ToString(values[8]);

            return place;
        }
    }

    // Handler for geonames dumps
    public class Geocoder
    {
        //http://download.geonames.org/export/dump/

        public List<Place> Places { get; set; }

        /// <summary>
        /// Import the geonames txt file in Helpers\Geocode\ folder
        /// </summary>
        /// <param name="countryCode">CH or HN so far</param>
        public Geocoder(string countryCode)
        {
            string filename = $@"Helpers\Geocode\{countryCode}.txt";
            if (File.Exists(filename))
            {
                Places = File.ReadAllLines(filename)
                                              .Skip(0)
                                              .Select(v => Place.FromCsv(v))
                                              .OrderBy(v => v.name).ThenBy(v => v.featurecode)
                                              //.GroupBy(v => new { v.name, v.featurecode })
                                              //.Select(g => g.First())
                                              .ToList();
            }
        }
    }
}

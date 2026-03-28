using SearchAndBook.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SearchAndBook.Services
{
    public class GeoService
    {
        private readonly Dictionary<string, City> _lookup = new();

        public GeoService() {}

        public static async Task<GeoService> CreateAsync()
        {
            var service = new GeoService();
            await service.InitializeAsync();
            return service;
        }

        public async Task InitializeAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/RO.txt"));

            var lines = await FileIO.ReadLinesAsync(file);

            foreach (var line in lines)
            {
                var parts = line.Split('\t');

                if (parts.Length < 7) continue;

                var featureClass = parts[6];
                if (featureClass != "P") continue;

                var name = parts[1];
                var name2 = parts[2];
                var alternateNames = parts[3];

                if (!double.TryParse(parts[4], out var lat)) continue;
                if (!double.TryParse(parts[5], out var lon)) continue;

                var city = new City
                {
                    MainName = name,
                    Latitude = lat,
                    Longitude = lon,
                    Names = new List<string>()
                };

                AddName(city, name);
                AddName(city, name2);

                if (!string.IsNullOrWhiteSpace(alternateNames))
                {
                    foreach (var alt in alternateNames.Split(','))
                    {
                        AddName(city, alt);
                    }
                }
            }
        }

        private void AddName(City city, string rawName)
        {
            var normalized = Normalize(rawName);

            if (string.IsNullOrWhiteSpace(normalized))
                return;

            city.Names.Add(normalized);

            if (!_lookup.ContainsKey(normalized))
            {
                _lookup[normalized] = city;
            }
        }

        public (bool found, string name, double lat, double lon) GetCityDetails(string cityName)
        {
            var key = Normalize(cityName);

            if (_lookup.TryGetValue(key, out var city))
            {
                return (true, city.MainName, city.Latitude, city.Longitude);
            }

            return (false, "", 0, 0);
        }

        private string Normalize(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return string.Empty;

            return city
                .Trim()
                .ToLower()
                .Replace("-", " ")
                .Replace("ă", "a")
                .Replace("â", "a")
                .Replace("î", "i")
                .Replace("ș", "s")
                .Replace("ţ", "t")
                .Replace("ț", "t");
        }
    }
}

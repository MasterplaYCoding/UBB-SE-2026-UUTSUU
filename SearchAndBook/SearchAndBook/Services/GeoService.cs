using SearchAndBook.Domain;
using SearchAndBook.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SearchAndBook.Services
{
    public class GeoService : InterfaceGeographicalService
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

                if (parts.Length < 15) continue;

                var featureClass = parts[6];
                if (featureClass != "P" && featureClass != "PPLC") continue;

                long.TryParse(parts[14], out var pop);

                if (pop < 5000) continue;

                var name = parts[1];
                var name2 = parts[2];
                var alternateNames = parts[3];

                if (!double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var lat)) continue;
                if (!double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var lon)) continue;

                var city = new City
                {
                    MainName = name,
                    Latitude = lat,
                    Longitude = lon,
                    Names = new List<string>(),
                };

                AddName(city, name);
                AddName(city, name2);

                if (name2.Trim().Equals("Bucuresti", StringComparison.OrdinalIgnoreCase))
                {
                    AddName(city, "Bucharest");
                }

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

        public double? GetDistanceBetweenCities(string city1, string city2)
        {
            var firstCity = GetCityDetails(city1);
            var secondCity = GetCityDetails(city2);

            if (!firstCity.found || !secondCity.found)
            {
                return null;
            }

            return GeographicDistance.CalculateDistance(
                firstCity.lat, firstCity.lon,
                secondCity.lat, secondCity.lon);
        }

        public List<string> GetCitySuggestions(string partialName)
        {
            if (string.IsNullOrWhiteSpace(partialName))
                return new List<string>();

            var normalizedInput = Normalize(partialName);

            return _lookup
                .Where(kvp => kvp.Key.Contains(normalizedInput))
                .Select(kvp => kvp.Value.MainName)
                .Distinct()
                .Take(10)
                .ToList();
        }
    }
}

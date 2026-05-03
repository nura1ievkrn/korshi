using System.Text.Json;

namespace korshi.Services
{
    public class WeatherService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public WeatherService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Weather:ApiKey"] ?? string.Empty;
        }

        public async Task<WeatherData?> GetWeatherAsync(string city = "Astana")
        {
            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/weather" +
                          $"?q={city}&appid={_apiKey}&units=metric&lang=ru";

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new WeatherData
                {
                    City = root.GetProperty("name").GetString() ?? city,
                    Temp = (int)Math.Round(root.GetProperty("main").GetProperty("temp").GetDouble()),
                    Description = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? "",
                    Icon = GetIcon(root.GetProperty("weather")[0].GetProperty("main").GetString() ?? "")
                };
            }
            catch
            {
                return null;
            }
        }

        private static string GetIcon(string main) => main switch
        {
            "Clear" => "bi-sun",
            "Clouds" => "bi-cloud",
            "Rain" => "bi-cloud-rain",
            "Drizzle" => "bi-cloud-drizzle",
            "Thunderstorm" => "bi-lightning",
            "Snow" => "bi-snow",
            "Mist" or "Fog" or "Haze" => "bi-cloud-fog2",
            _ => "bi-thermometer-half"
        };
    }

    public class WeatherData
    {
        public string City { get; set; } = string.Empty;
        public int Temp { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-sun";
    }
}
using korshi.Services;
using Microsoft.AspNetCore.Mvc;

namespace korshi.Controllers
{
    public class WeatherController : Controller
    {
        private readonly WeatherService _weatherService;

        public WeatherController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        // GET /Weather/Get?city=Astana
        [HttpGet]
        public async Task<IActionResult> Get(string city = "Astana")
        {
            var data = await _weatherService.GetWeatherAsync(city);
            if (data == null)
                return Json(new { temp = "--", icon = "bi-thermometer-half", city = city });

            return Json(new
            {
                temp = data.Temp + "°C",
                icon = data.Icon,
                city = data.City,
                desc = data.Description
            });
        }
    }
}
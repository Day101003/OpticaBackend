using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ProyectoFinal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IStringLocalizer<Messages> _localizer;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IStringLocalizer<Messages> localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = _localizer["Summary"]
            })
            .ToArray();
        }
    }
}

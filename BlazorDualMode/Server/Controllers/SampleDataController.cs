using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorDualMode.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BlazorDualMode.Server.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private readonly IWeatherForecastService _forecastService;

        public SampleDataController(IWeatherForecastService forecastService)
        {
            this._forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<WeatherForecast>> WeatherForecasts()
        {
            return await this._forecastService.GetForecastAsync();
        }
    }
}

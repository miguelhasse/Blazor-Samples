using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorDualMode.Shared;

namespace BlazorDualMode.Client
{
    internal class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient _http;

        public WeatherForecastService(HttpClient httpClient)
        {
            this._http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<WeatherForecast[]> GetForecastAsync()
        {
            return await this._http.GetFromJsonAsync<WeatherForecast[]>("api/SampleData/WeatherForecasts");
        }
    }
}

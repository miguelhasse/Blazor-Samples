using System.Threading.Tasks;

namespace BlazorDualMode.Shared
{
    public interface IWeatherForecastService
    {
        Task<WeatherForecast[]> GetForecastAsync();
    }
}

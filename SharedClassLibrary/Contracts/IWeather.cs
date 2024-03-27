using SharedClassLibrary.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClassLibrary.Contracts
{
    public interface IWeather
    {
        Task<WeatherForecast[]> GetWeatherForecasts();
    }
}

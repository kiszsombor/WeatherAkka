using System;
using System.Collections.Generic;

namespace WeatherAkka.Models
{
    public class WeatherForecast
    {
        public WeatherForecast()
        {
            Times = new List<DateTime>();
            Temperature_2m = new List<double>();
        }

        public List<DateTime> Times { get; set; }
        public List<double> Temperature_2m { get; set; }
    }
}

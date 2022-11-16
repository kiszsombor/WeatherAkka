using System;

namespace WeatherAkka.Models
{
    public class CurrentWeather
    {
        public double Temperature { get; set; }
        public double Windspeed { get; set; }
        public double Winddirection { get; set; }
        public int Weathercode { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return "Temperature = " + Temperature + "°C, Windspeed = " + Windspeed +
                "km/h, Winddirection = " + Winddirection + "°, Weathercode = " + Weathercode + " WMO code, Time = " + Time;
        }
    }
}

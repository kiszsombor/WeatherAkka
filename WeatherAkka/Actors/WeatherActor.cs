using Akka.Actor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace WeatherAkka.Models
{
    public class WeatherActor : ReceiveActor
    {
        private readonly Geocoding geocoding;
        private readonly CurrentWeather currentWeather;

        public WeatherActor(IActorRef fileWriter, IActorRef weather)
        {
            // _countryName = countryName;
            // geocodingList = new List<Geocoding>();
            geocoding = new Geocoding();
            currentWeather = new CurrentWeather();

            Receive<string>(x =>
            {
                GetWeatherAsync(x).Wait();
                // System.Diagnostics.Debug.WriteLine(currentWeather);
                string data = currentWeather.ToString();
                weather.Tell(currentWeather);
                fileWriter.Tell(x + ": " + data);
                // Sender.Tell(x + ": " + data);
            });
        }

        public async Task GetWeatherAsync(string cityName)
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://geocoding-api.open-meteo.com/v1/search?name=" + cityName);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Throw an exception if error

            // var body = await response.ReadAsStringAsync();
            var body = await response.Content.ReadAsStringAsync();

            // geocodingList = JsonConvert.DeserializeObject<List<Geocoding>>(body);
            // geocoding = JsonConvert.DeserializeObject<Geocoding>(body);
            var data = (JObject)JsonConvert.DeserializeObject(body);
            geocoding.Latitude = data.SelectToken("results[0].latitude").Value<double>();
            geocoding.Longitude = data.SelectToken("results[0].longitude").Value<double>();

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            request = new HttpRequestMessage(HttpMethod.Get,
            "https://api.open-meteo.com/v1/forecast?latitude=" + geocoding.Latitude.ToString(nfi) + "&longitude=" + geocoding.Longitude.ToString(nfi)
                + "&hourly=temperature_2m&current_weather=true");

            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Throw an exception if error

            body = await response.Content.ReadAsStringAsync();
            // CurrentWeather currentWeather = JsonConvert.DeserializeObject<CurrentWeather>(body);
            data = (JObject)JsonConvert.DeserializeObject(body);
            // System.Diagnostics.Debug.WriteLine(data);

            // System.Diagnostics.Debug.WriteLine(data.SelectToken("current_weather"));
            // currentWeather = data.SelectToken("current_weather").Value<CurrentWeather>();
            currentWeather.Temperature = data.SelectToken("current_weather.temperature").Value<double>();
            currentWeather.Windspeed = data.SelectToken("current_weather.windspeed").Value<double>();
            currentWeather.Winddirection = data.SelectToken("current_weather.winddirection").Value<double>();
            currentWeather.Weathercode = data.SelectToken("current_weather.weathercode").Value<int>();
            currentWeather.Time = data.SelectToken("current_weather.time").Value<DateTime>();
        }
    }

    public class Geocoding
    {
        private double latitude;
        private double longitude;

        public double Latitude { get => latitude; set => latitude = value; }
        public double Longitude { get => longitude; set => longitude = value; }

        public override string ToString()
        {
            return Latitude + " " + Longitude;
        }
    }

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

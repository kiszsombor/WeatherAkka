using Akka.Actor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherAkka.Actors;

namespace WeatherAkka.Models
{
    public class WeatherActor : ReceiveActor
    {
        private readonly Geocoding geocoding;
        private readonly CurrentWeather currentWeather;
        private readonly WeatherForecast weatherForecast;
        private readonly IActorRef da;
        readonly IActorRef mba;
        // private string body;
        private Tuple<string, string> cityNameAndJson;

        public WeatherActor(IActorRef fileWriter, IActorRef weather)
        {
            geocoding = new Geocoding();
            currentWeather = new CurrentWeather();
            weatherForecast = new WeatherForecast();

            // var da = Context.System.ActorOf(Props.Create(() => new DataBaseActor()), "DataBaseActor");
            da = Context.System.ActorOf(Props.Create(() => new DataBaseActor()), "DataBaseActor");

            mba = Context.System.ActorOf(Props.Create(() => new ModbusActor()), "ModbusActor");

            Receive<string>(cityName =>
            {
                if (cityName != null)
                {
                    GetWeatherAsync(cityName).Wait();
                    // System.Diagnostics.Debug.WriteLine(currentWeather);
                    string data = currentWeather.ToString();
                    weather.Tell(currentWeather);
                    fileWriter.Tell(data);
                    // Sender.Tell(x + ": " + data); // ???
                    weather.Tell(weatherForecast);

                    // da.Tell("fullList");
                    // da.Tell(currentWeather);
                    // da.Tell(weatherForecast);
                    da.Tell(cityNameAndJson);
                    mba.Tell(currentWeather);
                }
            });

            Receive<(string, int)>(commandAndId =>
            {
                // System.Diagnostics.Debug.WriteLine(commandAndId.Item2);
                if (commandAndId.Item1.Equals("ask_currentWeather"))
                {
                    Sender.Tell(new Tuple<CurrentWeather, int>(currentWeather, commandAndId.Item2));
                }
            });

        }

        public CurrentWeather CurrentWeather => currentWeather;

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

            NumberFormatInfo nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            };

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
            currentWeather.Cityname = cityName;
            currentWeather.Temperature = data.SelectToken("current_weather.temperature").Value<double>();
            currentWeather.Windspeed = data.SelectToken("current_weather.windspeed").Value<double>();
            currentWeather.Winddirection = data.SelectToken("current_weather.winddirection").Value<double>();
            currentWeather.Weathercode = data.SelectToken("current_weather.weathercode").Value<int>();
            currentWeather.Time = data.SelectToken("current_weather.time").Value<DateTime>();

            cityNameAndJson = new Tuple<string, string>(cityName, body);

            weatherForecast.CityName = cityName;
            weatherForecast.Times.Clear();
            weatherForecast.Temperature_2m.Clear();
            foreach (var singleProp in data.SelectToken("hourly.time"))
            {
                weatherForecast.Times.Add(singleProp.Value<DateTime>());
            }

            foreach (var singleProp in data.SelectToken("hourly.temperature_2m"))
            {
                weatherForecast.Temperature_2m.Add(singleProp.Value<double>());
            }

            // System.Diagnostics.Debug.WriteLine(weatherForecast.Times[0] + " " + weatherForecast.Temperature_2m[0]);
        }
    }
}

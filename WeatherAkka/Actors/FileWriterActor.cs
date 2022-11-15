using Akka.Actor;
using System.IO;

namespace WeatherAkka.Actors
{
    class FileWriterActor : ReceiveActor
    {
        public FileWriterActor()
        {
            Receive<string>(x =>
            {
                // System.Diagnostics.Debug.WriteLine(x);
                // File.WriteAllText("Current_weather_data.txt", x);
                File.WriteAllText("..\\..\\Current_weather_data.txt", x);
                
            });
        }
    }
}

using Akka.Actor;
using Akka.IO;
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
                // var da = Context.System.ActorOf(Props.Create(() => new DataBaseActor()), "DataBaseActor");
                // da.Tell("start");
            });
        }
    }
}

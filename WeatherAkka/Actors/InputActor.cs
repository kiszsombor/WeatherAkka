using Akka.Actor;
using WeatherAkka.Models;
using WeatherAkka.ViewModels;

namespace WeatherAkka.Actors
{
    public class InputActor : ReceiveActor
    {
        private readonly IActorRef weather;
        private readonly InputWindowViewModel inputWindowViewModel;
        // private readonly SectionsWeatherChartViewModel sectionsWeatherChartViewModel;

        private WeatherForecast weatherForecast;

        public InputActor(InputWindowViewModel inputWindowViewModel/*, SectionsWeatherChartViewModel sectionsWeatherChartViewModel*/)
        {
            this.inputWindowViewModel = inputWindowViewModel;
            // this.sectionsWeatherChartViewModel = sectionsWeatherChartViewModel;
            inputWindowViewModel.PropertyChanged += InputWindowViewModel_PropertyChanged;
            // sectionsWeatherChartViewModel.PropertyChanged += SectionsWeatherChartViewModel_PropertyChanged;

            weatherForecast = new WeatherForecast();

            var fw = Context.System.ActorOf(Props.Create(() => new FileWriterActor()), "FileWriterActor");
            weather = Context.System.ActorOf(Props.Create(() => new WeatherActor(fw, Self)), "WeatherActor");
            // this.weather.Tell(inputWindowViewModel.City);

            Receive<CurrentWeather>(x =>
            {
                // System.Diagnostics.Debug.WriteLine(x);
                inputWindowViewModel.RefreshLabel(x.ToString());
            });

            Receive<WeatherForecast>(x =>
            {
                weatherForecast = x;
                inputWindowViewModel.SectionsWeatherChartViewModel.RefreshData(weatherForecast);
                // sectionsWeatherChartViewModel.RefreshData(weatherForecast);
            });

            var tla = Context.System.ActorOf(Props.Create(() => new TcpListenerActor(weather)), "TcpListenerActor");
            tla.Tell("start");
        }

        private void InputWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                if (inputWindowViewModel.Name != null)
                {
                    weather.Tell(inputWindowViewModel.Name);
                }   
            }
        }

        /*
        private void SectionsWeatherChartViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SeriesCollection")
            {
                if (sectionsWeatherChartViewModel.SeriesCollection != null)
                {
                    sectionsWeatherChartViewModel.RefreshData(weatherForecast);
                }
            }
        }
        */
    }
}

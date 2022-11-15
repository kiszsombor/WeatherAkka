using Akka.Actor;
using System;
using WeatherAkka.Models;
using WeatherAkka.ViewModels;

namespace WeatherAkka.Actors
{
    public class InputActor : ReceiveActor
    {
        private readonly IActorRef weather;
        private readonly InputWindowViewModel inputWindowViewModel;

        public InputActor(InputWindowViewModel inputWindowViewModel)
        {
            this.inputWindowViewModel = inputWindowViewModel;
            inputWindowViewModel.PropertyChanged += InputWindowViewModel_PropertyChanged;

            
            var fw = Context.System.ActorOf(Props.Create(() => new FileWriterActor()), "FileWriterActor");
            weather = Context.System.ActorOf(Props.Create(() => new WeatherActor(fw, Self)), "WeatherActor");
            // this.weather.Tell(inputWindowViewModel.City);

            Receive<CurrentWeather>(x =>
            {
                // System.Diagnostics.Debug.WriteLine(x);
                inputWindowViewModel.RefreshLabel(x.ToString());
                // inputWindowViewModel.PropertyChanged += InputWindowViewModel_PropertyChanged;
            });
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
    }
}

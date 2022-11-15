using Akka.Actor;
using WeatherAkka.ViewModels;

namespace WeatherAkka.Actors
{
    public class InputActor : ReceiveActor
    {
        private readonly IActorRef weather;
        private readonly InputWindowViewModel inputWindowViewModel;

        public InputActor(IActorRef weather, InputWindowViewModel inputWindowViewModel)
        {
            this.inputWindowViewModel = inputWindowViewModel;
            inputWindowViewModel.PropertyChanged += InputWindowViewModel_PropertyChanged;
            this.weather = weather;
            // this.weather.Tell(inputWindowViewModel.City);
        }

        private void InputWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                weather.Tell(inputWindowViewModel.Name);
            }
        }
    }
}

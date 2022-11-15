using Akka.Actor;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WeatherAkka.Models;

namespace WeatherAkka.ViewModels
{
    public class InputWindowViewModel : INotifyPropertyChanged
    {
        public InputWindowViewModel()
        {
            PrintCurrentWeather = new MyICommand(OnPrint);
        }

        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return name; }

            set
            {
                if (name != value)
                {
                    name = value;
                    // OnPropertyChanged();
                }
            }
        }

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public MyICommand PrintCurrentWeather { get; set; }

        private void OnPrint()
        {
            RaisePropertyChanged(nameof(Name));
        }

        /*
        private void OnPropertyChanged([CallerMemberName] string propName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        */

    }
}

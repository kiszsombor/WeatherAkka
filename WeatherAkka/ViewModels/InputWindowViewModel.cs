using System.ComponentModel;
using System.Data;
using System.Windows.Data;

namespace WeatherAkka.ViewModels
{
    public class InputWindowViewModel : INotifyPropertyChanged
    {
        private string name;
        private string weatherData;

        public event PropertyChangedEventHandler PropertyChanged;

        public InputWindowViewModel()
        {
            PrintCurrentWeather = new MyICommand(OnPrint);
            WeatherData = "*";
        }

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

        public string WeatherData { get => weatherData; set => weatherData = value; }

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void RefreshLabel(string label)
        {
            WeatherData= label;
            
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

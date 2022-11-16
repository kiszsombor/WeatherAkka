using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeatherAkka.ViewModels
{
    public class InputWindowViewModel : INotifyPropertyChanged
    {
        private string name;
        private string weatherData;

        public event PropertyChangedEventHandler PropertyChanged;

        private SectionsWeatherChartViewModel sectionsWeatherChartViewModel;

        public MyICommand PrintCurrentWeather { get; set; }

        public SectionsWeatherChartViewModel SectionsWeatherChartViewModel 
        { 
            get => sectionsWeatherChartViewModel; 
            set => sectionsWeatherChartViewModel = value; 
        }

        public InputWindowViewModel(System.Windows.Threading.Dispatcher dispatcher)
        {
            PrintCurrentWeather = new MyICommand(OnPrint);
            WeatherData = "*";
            SectionsWeatherChartViewModel = new SectionsWeatherChartViewModel(dispatcher);
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

        public string WeatherData
        {
            get => weatherData;
            set
            {
                weatherData = value;
                RaisePropertyChanged();
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void RefreshLabel(string label)
        {
            WeatherData = Name + ": " + label;
        }

        private void OnPrint()
        {
            RaisePropertyChanged(nameof(Name));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}

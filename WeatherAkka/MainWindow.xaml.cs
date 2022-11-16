using Akka.Actor;
using System.Windows;
using WeatherAkka.Actors;
using WeatherAkka.ViewModels;

namespace WeatherAkka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var input = new InputWindowViewModel(Dispatcher);
            DataContext = input;
            // var chart = new SectionsWeatherChartViewModel();

            var system = ActorSystem.Create("MySystem");

            // var fw = system.ActorOf(Props.Create(() => new FileWriterActor()), "FileWriterActor");
            // var wa = system.ActorOf(Props.Create(() => new WeatherActor(fw)), "WeatherActor");
            // var ia = system.ActorOf(Props.Create(() => new InputActor(wa, input)), "InputActor");
            system.ActorOf(Props.Create(() => new InputActor(input/*, chart*/)), "InputActor");
        }
    }
}

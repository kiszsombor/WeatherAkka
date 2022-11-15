using Akka.Actor;
using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeatherAkka.Actors;
using WeatherAkka.Models;
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
            var system = ActorSystem.Create("MySystem");

            var fw = system.ActorOf(Props.Create(() => new FileWriterActor()), "FileWriterActor");
            var wa = system.ActorOf(Props.Create(() => new WeatherActor(fw)), "WeatherActor");
            var input = new InputWindowViewModel();
            DataContext = input;
            system.ActorOf(Props.Create(() => new InputActor(wa, input)), "InputActor");
        }
    }
}

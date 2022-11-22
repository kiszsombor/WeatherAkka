using LiveCharts.Defaults;
using LiveCharts;
using System;
using System.Linq;
using System.Windows;
using LiveCharts.Wpf;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WeatherAkka.Models;
using System.Collections.Generic;
using System.Windows.Threading;

namespace WeatherAkka.ViewModels
{
    public class SectionsWeatherChartViewModel : INotifyPropertyChanged
    {
        private SeriesCollection seriesCollection;

        private List<string> labels;
        private readonly Dispatcher dispatcher;

        public List<string> Labels { get => labels; set => labels = value; }

        public SeriesCollection SeriesCollection
        {
            get => seriesCollection;
            set
            {
                seriesCollection = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SectionsWeatherChartViewModel(Dispatcher dispatcher)
        {
            labels = new List<string>() { "10", "20", "30", "40", "50", "60" };

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(0),
                        new ObservableValue(1),
                        new ObservableValue(1),
                        new ObservableValue(1),
                        new ObservableValue(1),
                        new ObservableValue(1)
                    },
                    PointGeometrySize = 0,
                    StrokeThickness = 4,
                    Fill = Brushes.Transparent
                }
            };
            this.dispatcher = dispatcher;
        }

        private void RaisePropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void UpdateAllOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();

            foreach (var series in SeriesCollection)
            {
                foreach (var observable in series.Values.Cast<ObservableValue>())
                {
                    observable.Value = r.Next(0, 10);
                }
            }
        }

        public void RefreshData(WeatherForecast weatherForecast)
        {
            // System.Diagnostics.Debug.WriteLine("OK");
            // RaisePropertyChanged(nameof(SeriesCollection));
            // RaisePropertyChanged();
            // var times = new ChartValues<ObservableValue

            var temps = new ChartValues<ObservableValue>();

            labels.Clear();
            weatherForecast.Times.ForEach(time => labels.Add(time.ToString()));
            weatherForecast.Temperature_2m.ForEach(temp => temps.Add(new ObservableValue(temp)));

            dispatcher.Invoke(() =>
            {
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Temperature_2m",
                        Values = temps,
                        ScalesYAt = 0
                    }
                };
            });

            
        }
    }
}

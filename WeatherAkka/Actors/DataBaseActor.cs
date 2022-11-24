using Akka.Actor;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using WeatherAkka.Models;

namespace WeatherAkka.Actors
{
    class DataBaseActor : ReceiveActor
    {
        private readonly SqlConnection connection;

        public DataBaseActor()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = "localhost,1434",
                    UserID = "SA",
                    Password = "1234@jelszo",
                    InitialCatalog = "WeatherTest",
                    TrustServerCertificate = true
                };

                connection = new SqlConnection(builder.ConnectionString);
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine("***ERROR***");
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            Receive<string>(x =>
            {
                if (x.Equals("fullList"))
                {
                    // System.Diagnostics.Debug.WriteLine(x);
                    ListCurrentWeather();
                }
            });

            Receive<Tuple<string, string>>(x =>
            {
                using (var command = new SqlCommand("InsertCurrentWeatherAndWeatherForecast", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    Parameters = { new SqlParameter("@cityname", x.Item1), new SqlParameter("@jsonVariable", x.Item2) }
                })
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            });

            Receive<CurrentWeather>(x =>
            {
                InsertCurrentWeather(x);
            });

            Receive<WeatherForecast>(x =>
            {
                InsertWeatherForecast(x);
            });
        }

        public void ListCurrentWeather()
        {
            String sql = "SELECT cityname, temperature, windspeed, winddirection, weathercode, time " +
                         "FROM CurrentWeather";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} {4} {5}",
                            reader.GetString(0), reader.GetDouble(1), reader.GetDouble(2),
                            reader.GetDouble(3), reader.GetInt32(4), reader.GetDateTime(5)
                            );
                    }
                }
                connection.Close();
            }
            /*
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = "localhost,1434",
                    UserID = "SA",
                    Password = "1234@jelszo",
                    InitialCatalog = "WeatherTest",
                    TrustServerCertificate = true
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    System.Diagnostics.Debug.WriteLine("\nQuery data example:");
                    System.Diagnostics.Debug.WriteLine("=========================================\n");

                    String sql = "SELECT cityname, temperature FROM CurrentWeather"
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                System.Diagnostics.Debug.WriteLine("{0} {1}", reader.GetString(0), reader.GetDouble(1));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine("***ERROR***");
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            // Console.ReadLine();
            */
        }

        public void InsertCurrentWeather(CurrentWeather currentWeather)
        {
            NumberFormatInfo nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            };

            String sql = "INSERT CurrentWeather(cityname, temperature, windspeed, winddirection, weathercode, time) "
                        + "VALUES(\'" + currentWeather.Cityname + "\', " 
                        + currentWeather.Temperature.ToString(nfi) + ", " 
                        + currentWeather.Windspeed.ToString(nfi) 
                        + ", " + currentWeather.Winddirection.ToString(nfi) 
                        + ", " + currentWeather.Weathercode.ToString(nfi) + ", " 
                        + currentWeather.Time.ToString("\\'yyyy-MM-ddTHH:mm:ss\\'") + ");";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                // System.Diagnostics.Debug.WriteLine(sql);
                command.ExecuteNonQuery();
                connection.Close();
            }
            // ListCurrentWeather();
        }

        public void InsertWeatherForecast(WeatherForecast weatherForecast)
        {
            NumberFormatInfo nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            };

            var temperaturesAndTimes = weatherForecast.Temperature_2m.Zip(weatherForecast.Times, (n, w) => new { Temperature = n, Time = w });
            String sql = null;
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                foreach (var tt in temperaturesAndTimes)
                {
                    sql = "INSERT WeatherForecast(cityname, time, temperature_2m) "
                            + "VALUES(\'" + weatherForecast.CityName + "\', "
                            + tt.Time.ToString("\\'yyyy-MM-ddTHH:mm:ss\\'") + ", "
                            + tt.Temperature.ToString(nfi)
                            + ");";

                    new SqlCommand(sql, connection).ExecuteNonQuery();
                }
                
                connection.Close();
            }
            /*foreach (var tt in temperaturesAndTimes)
            {
                // System.Diagnostics.Debug.WriteLine(nw.Temperature + " " + nw.Time);

                String sql = "INSERT WeatherForecast(cityname, time, temperature_2m) "
                            + "VALUES(\'" + weatherForecast.CityName + "\', " 
                            + tt.Time.ToString("\\'yyyy-MM-ddTHH:mm:ss\\'") + ", "
                            + tt.Temperature.ToString(nfi)
                            + ");";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    // System.Diagnostics.Debug.WriteLine(sql);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }*/
        }
    }
}
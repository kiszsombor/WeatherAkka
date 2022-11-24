using Akka.Actor;
using Akka.Util;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using WeatherAkka.Models;

namespace WeatherAkka.Actors
{
    public class TcpListenerActor : ReceiveActor
    {
        private readonly IActorRef weather;
        private TcpListener server = null;
        private TcpClient client;

        public TcpListenerActor(IActorRef weather)
        {
            this.weather = weather;

            Receive<string>(x =>
            {
                if (x.Equals("start") || x.Equals("Start"))
                {
                    // System.Diagnostics.Debug.WriteLine("START");
                    if (server == null)
                    {
                        Start();
                    }
                }
                else if (x.Equals("stop") || x.Equals("Stop"))
                {
                    // System.Diagnostics.Debug.WriteLine("STOP");
                    server.Stop();
                    System.Diagnostics.Debug.WriteLine("\nDone...");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(x);
                }
            });

            Receive<(StreamReader, string)>(x =>
            {
                NetworkStream stream = client.GetStream();

                StreamReader reader = x.Item1;
                if (x.Item2 != null)
                {
                    weather.Tell(x.Item2);

                    using (var task = weather.Ask("ask_currentWeather"))
                    {
                        task.Wait();
                        CurrentWeather currentWeather = (CurrentWeather)task.Result;

                        byte[] msg = Encoding.ASCII.GetBytes(currentWeather.ToString());
                        stream.Write(msg, 0, msg.Length);
                    }

                    reader.ReadLineAsync().PipeTo(Self, null, (s) => (reader, s));
                    // server.AcceptTcpClientAsync().PipeTo(Self);
                }
            });

            Receive<TcpClient>(x =>
            {
                client = x;

                NetworkStream stream = client.GetStream();

                StreamReader reader = new StreamReader(stream);
                reader.ReadLineAsync().PipeTo(Self, null, (s) => (reader, s));

                server.AcceptTcpClientAsync().PipeTo(Self);
            });
        }

        public void Start()
        {
            try
            {
                // Set the TcpListener on port 13000.
                int port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                server.AcceptTcpClientAsync().PipeTo(Self);
            }
            catch (SocketException e)
            {
                System.Diagnostics.Debug.WriteLine("SocketException: {0}", e);
            }
        }
    }
}

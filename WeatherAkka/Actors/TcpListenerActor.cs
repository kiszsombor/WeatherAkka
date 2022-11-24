using Akka.Actor;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WeatherAkka.Models;

namespace WeatherAkka.Actors
{
    public class TcpListenerActor : ReceiveActor
    {
        private readonly IActorRef weather;
        private TcpListener server = null;
        // private TcpClient tcpClienttcpClient = null;
        private bool stop;

        public TcpListenerActor(IActorRef weather)
        {
            this.weather = weather;
            stop = false;

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
                if (x.Equals("stop") || x.Equals("Stop"))
                {
                    // System.Diagnostics.Debug.WriteLine("STOP");
                    stop = true;
                }
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

                // Buffer for reading data
                byte[] bytes = new byte[256];
                string data = null;
                CurrentWeather currentWeather = null;

                // Enter the listening loop.
                while (!stop)
                {
                    System.Diagnostics.Debug.WriteLine("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    using (TcpClient client = server.AcceptTcpClient())
                    // using (TcpClient client = server.AcceptTcpClientAsync())
                    {
                        System.Diagnostics.Debug.WriteLine("Connected!");

                        data = null;

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        int i;

                        // Loop to receive all the data sent by the client.
                        // stream.readline
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = Encoding.ASCII.GetString(bytes, 0, i);
                            System.Diagnostics.Debug.WriteLine("Received: {0}", data, "");

                            // Process the data sent by the client.
                            weather.Tell(data);

                            using (var a = weather.Ask("ask_currentWeather"))
                            {
                                a.Wait();
                                // System.Diagnostics.Debug.WriteLine(a.Result);
                                currentWeather = (CurrentWeather)a.Result;
                            }

                            data = currentWeather.ToString();

                            byte[] msg = Encoding.ASCII.GetBytes(data);

                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                            System.Diagnostics.Debug.WriteLine("Sent: {0}", data, "");
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                System.Diagnostics.Debug.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }

            System.Diagnostics.Debug.WriteLine("\nDone...");
        }
    }
}

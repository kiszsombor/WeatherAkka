using Akka.Actor;
using Akka.Util.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using WeatherAkka.Models;

namespace WeatherAkka.Actors
{
    public class TcpListenerActor : ReceiveActor
    {
        private TcpListener server = null;
        // private TcpClient client;
        private readonly Dictionary<int, TcpClient> clients;
        private int clientId;

        public TcpListenerActor(IActorRef weather)
        {
            clients= new Dictionary<int, TcpClient>();
            clientId = -1;

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
                    System.Diagnostics.Debug.WriteLine("_______\nDone...");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(x);
                }
            });

            Receive<(StreamReader, string, int)>(x =>
            {
                StreamReader reader = x.Item1;
                if (x.Item2 != null)
                {
                    weather.Tell(x.Item2);
                    weather.Ask(("ask_currentWeather", x.Item3)).PipeTo(Self, null, s => (Tuple<CurrentWeather, int>)s);

                    reader.ReadLineAsync().PipeTo(Self, null, (s) => (reader, s, x.Item3)/*, ex => (ex, x.Item3)*/);
                }
                /*
                else
                {
                    clients.Remove(x.Item3);
                }
                */
            });

            Receive<TcpClient>(x =>
            {
                // client = x;
                ++clientId;
                clients.Add(clientId, x);

                // NetworkStream stream = client.GetStream();
                NetworkStream stream = clients[clientId].GetStream();

                StreamReader reader = new StreamReader(stream);
                // reader.ReadLineAsync().PipeTo(Self, null, (s) => (reader, s));
                reader.ReadLineAsync().PipeTo(Self, null, (s) => (reader, s, clientId)/*, ex => (ex, clientId)*/);

                server.AcceptTcpClientAsync().PipeTo(Self);
            });

            /*
            Receive<Tuple<Exception, int>>(exceptionAndClientId =>
            {
                System.Diagnostics.Debug.WriteLine("ok");

                // System.Diagnostics.Debug.WriteLine(exceptionAndClientId.Item1);
                // clients.Remove(exceptionAndClientId.Item2);
            });
            */

            Receive<Tuple<CurrentWeather, int>>(currentWeatherAndId =>
            {
                NetworkStream stream = clients[currentWeatherAndId.Item2].GetStream();

                byte[] msg = Encoding.ASCII.GetBytes(currentWeatherAndId.Item1.ToString());
                stream.Write(msg, 0, msg.Length);

                if (!clients[currentWeatherAndId.Item2].Connected)
                {
                    clients.Remove(currentWeatherAndId.Item2);

                }

                clients.ForEach(client => System.Diagnostics.Debug.WriteLine(client));
                ClearConnections();
            });
        }

        private void ClearConnections()
        {
            List<int> ids = new List<int>();
            foreach (var client in clients)
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x =>
                    x.LocalEndPoint.Equals(client.Value.Client.LocalEndPoint)
                    && x.RemoteEndPoint.Equals(client.Value.Client.RemoteEndPoint)).ToArray();

                TcpState stateOfConnection = tcpConnections.First().State;
                if (stateOfConnection != TcpState.Established)
                {
                    ids.Add(client.Key);
                }
            }

            ids.ForEach(id => clients.Remove(id));
            System.Diagnostics.Debug.WriteLine("cleared");
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

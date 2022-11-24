using Akka.Actor;
using NModbus;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeatherAkka.Models;

namespace WeatherAkka.Actors
{
    public class ModbusActor : ReceiveActor
    {
        private readonly TcpClient client;
        private readonly IModbusMaster master;
        private readonly bool connected;

        public ModbusActor()
        {
            connected = false;

            try
            {
                client = new TcpClient("127.0.0.1", 502);
                var factory = new ModbusFactory();
                master = factory.CreateMaster(client);

                connected = true;

                /*
                using (client = new TcpClient("127.0.0.1", 502))
                {
                    var factory = new ModbusFactory();
                    master = factory.CreateMaster(client);

                    connected = true;

                    // read five input values
                    // ushort startAddress = 1;
                    // ushort numInputs = 5;
                    // bool[] inputs = master.ReadInputs(1, startAddress, numInputs);
                    // System.Diagnostics.Debug.WriteLine("OK");

                    master.WriteSingleRegister(1, 5, 250);
                    master.WriteSingleCoil(1, 5, true);

                    // for (int i = 0; i < numInputs; ++i)
                    // {
                    //     System.Diagnostics.Debug.WriteLine($"Input {(startAddress + i)}={(inputs[i] ? 1 : 0)}");
                    // }
                }
                */
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            

            Receive<CurrentWeather>(currenWeather =>
            {
                if(connected)
                {
                    if(currenWeather.Temperature < 0) 
                    {
                        master.WriteSingleCoil(1, 5, true);
                    }
                    else
                    {
                        master.WriteSingleCoil(1, 5, false);
                    }

                    master.WriteSingleRegister(1, 5, (ushort)(Math.Abs(currenWeather.Temperature * 10)));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Modbus connection falied!");
                }
            });
        }
    }
}

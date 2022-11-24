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

namespace WeatherAkka.Actors
{
    public class ModbusActor : ReceiveActor
    {
        private TcpClient client;
        public ModbusActor()
        {
            using (client = new TcpClient("127.0.0.1", 502))
            {
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                // read five input values
                ushort startAddress = 1;
                ushort numInputs = 5;

                // System.Diagnostics.Debug.WriteLine("OK");

                // bool[] inputs = master.ReadInputs(0, startAddress, numInputs);
                // bool[] inputs = master.ReadInputsAsync(0, startAddress, numInputs).Wait();

                master.WriteSingleRegister(1, 5, 250);
                master.WriteSingleCoil(1, 5, true);

                /*
                for (int i = 0; i < numInputs; ++i)
                {
                    System.Diagnostics.Debug.WriteLine($"Input {(startAddress + i)}={(inputs[i] ? 1 : 0)}");
                }
                */
            }
        }
    }
}

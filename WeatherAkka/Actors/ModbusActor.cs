using Akka.Actor;
using NModbus;
using System;
using System.Net.Sockets;
using WeatherAkka.Models;

namespace WeatherAkka.Actors
{
    public class Check { };

    public class ModbusActor : ReceiveActor, IWithTimers
    {
        private readonly TcpClient client;
        private readonly IModbusMaster master;
        private readonly bool connected;

        public ITimerScheduler Timers { get; set; }

        public ModbusActor()
        {
            connected = false;

            try
            {
                client = new TcpClient("127.0.0.1", 502);
                var factory = new ModbusFactory();
                master = factory.CreateMaster(client);

                connected = true;

            } catch(Exception ex)
            {
                connected = false;
                Timers.Cancel("canDelte?");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            Receive<CurrentWeather>(currenWeather =>
            {
                if(connected)
                {
                    master.WriteSingleCoil(1, 4, true);
                    if (currenWeather.Temperature < 0) 
                    {
                        master.WriteSingleCoil(1, 5, true);
                    }
                    else
                    {
                        master.WriteSingleCoil(1, 5, false);
                    }

                    master.WriteSingleRegister(1, 5, (ushort)(Math.Abs(currenWeather.Temperature * 10)));
                    // System.Diagnostics.Debug.WriteLine(master.ReadHoldingRegisters(1, 5, 1)[0]);
                    // Timers.StartPeriodicTimer("add", 1, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(20));
                }
                else
                {
                    Timers.Cancel("canDelte?");
                    System.Diagnostics.Debug.WriteLine("Modbus connection falied!");
                }
            });

            Receive<Check>(_ =>
            {
                if (connected)
                {
                    if(master.ReadCoils(1, 3, 1)[0])
                    {
                        master.WriteSingleCoil(1, 4, false);
                        master.WriteSingleCoil(1, 5, false);
                        master.WriteSingleRegister(1, 5, 0);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Modbus connection falied!");
                    Timers.Cancel("canDelte?");
                }
            });
        }

        protected override void PreStart()
        {
            Timers.StartPeriodicTimer("canDelte?", new Check(), TimeSpan.FromMilliseconds(3000));
        }
    }
}

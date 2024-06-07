using MQTT_Publisher;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


class Program
{
    static void Main(string[] args)
    {
      var mqttClient = new MqttClient();
        mqttClient.Connect("localhost", 8000);
        Console.WriteLine("Connected to MQTT Broker");
        Thread.Sleep(1000);
        mqttClient.Disconnect();
    }
}

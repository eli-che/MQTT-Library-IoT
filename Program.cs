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
        string topic = "test/topic";
        string message = "Hello MQTT!";
        byte[] payload = Encoding.UTF8.GetBytes(message);

        mqttClient.Publish(topic, payload, qos: 0, retain: false, dup: false);
        Console.WriteLine($"Published message to {topic}: {message}");
        Thread.Sleep(1000);
        mqttClient.Disconnect();
    }
}

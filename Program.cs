using MQTT_AIO;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


class Program
{
    static void Main(string[] args)
    {
        var mqttClient = new MqttClient();
        mqttClient.OnMessageReceived += (topic, payload) =>
        {
            Console.WriteLine($"Received message on topic '{topic}': {Encoding.UTF8.GetString(payload)}");
        };

        mqttClient.Connect("localhost", 8000);
        Console.WriteLine("Connected to MQTT Broker");
        string topic = "rainfall";
        string message = "Hello MQTT!";
        byte[] payload = Encoding.UTF8.GetBytes(message);

        mqttClient.Publish(topic, payload, qos: 0, retain: true, dup: false);
        Console.WriteLine($"Published message to {topic}: {message}");

        mqttClient.Subscribe(topic, qos: 0);
        Console.WriteLine($"Subscribed to {topic}");

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        mqttClient.Disconnect();
    }
}

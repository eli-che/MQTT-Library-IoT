using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace MQTT_Publisher
{
    public class MqttClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public void Connect(string host, int port)
        {
            _client = new TcpClient(host, port);
            _stream = _client.GetStream();

            var connectPacket = new MqttConnectPacket
            {
                clientId = "TestClient",
                cleanSession = true,
                keepAlive = 60
            };
            
            var packetBytes = connectPacket.ToByteArray();
            _stream.Write(packetBytes, 0, packetBytes.Length);

            var response = new byte[4];
            _stream.Read(response, 0, response.Length);
        }

        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
        }
    }

}

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

        }

        public void Disconnect()
        {

        }
    }

}

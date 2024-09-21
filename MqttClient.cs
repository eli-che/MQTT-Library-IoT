using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using MQTTnet.Packets;
using System.Threading;
using MQTTnet.Protocol;

namespace MQTT_AIO
{
    public class MqttClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected;
        private Thread _receiveThread;

        public event Action<string, byte[]> OnMessageReceived;
        public void Connect(string host, int port)
        {
            try { 
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

            //Read the connack reponse
            var response = new byte[4];
            _stream.Read(response, 0, response.Length);
            var connackResponse = new MqttConnackReponsePacket(response);
            if (connackResponse.ConnectReasonCode != 0)
            {
                Console.WriteLine($"Connection failed with return code: {connackResponse.ConnectReasonCode}");
                //Log the flags
                Console.WriteLine($"Connect Acknowledge Flags: {connackResponse.ConnectAcknowledgeFlags}");

            }
            else
            {
                Console.WriteLine("Connack Succesfully recieved!");
                    _isConnected = true; // Set _isConnected to true
                    StartReceiveThread();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void StartReceiveThread()
        {
            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        private void ReceiveLoop()
        {
            while (_isConnected)
            {
                try
                {
                    if (_stream.DataAvailable)
                    {
                        var fixedHeaderByte = _stream.ReadByte();
                        if (fixedHeaderByte == -1)
                        {
                            continue;
                        }

                        var remainingLength = ReadRemainingLength(_stream);

                        var packet = new byte[1 + remainingLength];
                        packet[0] = (byte)fixedHeaderByte;
                        _stream.Read(packet, 1, remainingLength);

                        HandleIncomingPacket(packet);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving data: {ex.Message}");
                    _isConnected = false;
                }
            }

        }

        private int ReadRemainingLength(NetworkStream stream)
        {
            int multiplier = 1;
            int value = 0;
            byte encodedByte;
            const int maxRemainingLength = 268435455; // The maximum value for the remaining length (256 MB)

            do
            {
                encodedByte = (byte)stream.ReadByte();
                if (encodedByte == -1)
                {
                    throw new EndOfStreamException("Stream ended while reading remaining length");
                }

                value += (encodedByte & 127) * multiplier;

                // Check for overflow
                if (value > maxRemainingLength)
                {
                    throw new OverflowException("Remaining length value overflowed");
                }

                multiplier *= 128;

                // Check for multiplier overflow
                if (multiplier > maxRemainingLength)
                {
                    throw new OverflowException("Multiplier overflowed");
                }

            } while ((encodedByte & 128) != 0);

            return value;
        }


        private void HandleIncomingPacket(byte[] packet)
        {
            var packetType = (MqttControlPacketType)(packet[0] >> 4);

            try
            {
                switch (packetType)
                {
                    case MqttControlPacketType.Publish:
                        Console.WriteLine("Handling PUBLISH packet...");
                        HandlePublishPacket(packet);
                        break;
                    case MqttControlPacketType.SubAck:
                        Console.WriteLine("Handling SUBACK packet...");
                        //HandleSubackPacket(packet);
                        break;
                    // Handle other packet types if needed
                    default:
                        Console.WriteLine($"Unhandled packet type: {packetType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling packet: {ex.Message}");
            }
        }

        private void HandlePublishPacket(byte[] packet)
        {
            try
            {
                Console.WriteLine("Handling PUBLISH packet...");
                var publishPacket = new MqttPublishPacket(packet);
                OnMessageReceived?.Invoke(publishPacket.Topic, publishPacket.Payload);
                Console.WriteLine($"Received message on topic {publishPacket.Topic}: {Encoding.UTF8.GetString(publishPacket.Payload)}");

                // Send PUBACK if QoS 1
                if (publishPacket.QosLevel == 1)
                {
                    // var pubackPacket = new MqttPubackPacket(publishPacket.PacketIdentifier);
                    // var pubackBytes = pubackPacket.ToByteArray();
                    // _stream.Write(pubackBytes, 0, pubackBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling PUBLISH packet: {ex.Message}");
            }
        }



        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
        }

        public void Publish(string topic, byte[] payload, byte qos = 0, bool retain = false, bool dup = false)
        {
            var publishPacket = new MqttPublishPacket
            {
                Topic = topic,
                Payload = payload,
                QosLevel = qos,
                Retain = retain,
                DupFlag = dup,
                PacketIdentifier = qos > 0 ? (ushort)new Random().Next(1, 65535) : (ushort)0
            };

            var packetBytes = publishPacket.ToByteArray();
            _stream.Write(packetBytes, 0, packetBytes.Length);

            if (qos == 1)
            {
                var response = new byte[4];
                _stream.Read(response, 0, response.Length);
                var pubackPacket = new MqttPubackReponsePacket(response);
                Console.WriteLine($"Received PUBACK with Packet Identifier: {pubackPacket.PacketIdentifier}");
            }


        }

        public void Subscribe(string topic, byte qos = 0)
        {
            var subscribePacket = new MqttSubscribePacket
            {
                Topic = topic,
                QosLevel = qos,
                PacketIdentifier = (ushort)new Random().Next(1, 65535)
            };

            var packetBytes = subscribePacket.ToByteArray();
            _stream.Write(packetBytes, 0, packetBytes.Length);

            //var response = new byte[4];
           // _stream.Read(response, 0, response.Length);
            // var subackPacket = new MqttSubackReponsePacket(response);
            // Console.WriteLine($"Received SUBACK with Packet Identifier: {subackPacket.PacketIdentifier}");
        }
    }
}

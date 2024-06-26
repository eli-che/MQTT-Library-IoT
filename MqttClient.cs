﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using MQTTnet.Packets;

namespace MQTT_AIO
{
    public class MqttClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

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
            }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

            var response = new byte[4];
            _stream.Read(response, 0, response.Length);
            var pubackPacket = new MqttPubackReponsePacket(response);
            Console.WriteLine($"Received PUBACK with Packet Identifier: {pubackPacket.PacketIdentifier}");
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

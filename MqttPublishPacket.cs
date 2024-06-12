using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_AIO
{
    public class MqttPublishPacket
    {
        public MqttFixedHeader FixedHeader { get; set; }
        public string Topic { get; set; }
        public ushort PacketIdentifier { get; set; }
        public byte[] Payload { get; set; }
        public bool DupFlag { get; set; }
        public byte QosLevel { get; set; } = 0;
        public bool Retain { get; set; }

        public MqttPublishPacket()
        {
            FixedHeader = new MqttFixedHeader
            {
                PacketType = mqttControlPacketType.PUBLISH,
                Flags = 0,
                RemainingLength = 0

            };
        }

        public MqttPublishPacket(byte[] packet)
        {
            using (var dataStream = new MemoryStream(packet))
            {
                //Read the fixed header
                byte firstByte = (byte)dataStream.ReadByte();
                FixedHeader = new MqttFixedHeader
                {
                    PacketType = (mqttControlPacketType)(firstByte >> 4),
                    Flags = (byte)(firstByte & 0x0F)
                };

                // Read the remaining length
                int multiplier = 1; // Multiplier for the position of the byte being processed (1, 128, 16384, etc.)
                int value = 0; // The decoded remaining length value
                byte encodedByte; // The current byte being processed
                do
                {
                    encodedByte = (byte)dataStream.ReadByte(); // Read the next byte from the stream
                    value += (encodedByte & 127) * multiplier; // Extract the 7 LSBs and add to the value
                    multiplier *= 128; // Increase the multiplier for the next byte
                } while ((encodedByte & 128) != 0);  // If the MSB is 1, continue reading the next byte
               
                FixedHeader.RemainingLength = value;

                //now we read the variable header which is the next byte to be read
                Topic = ReadString(dataStream);

                //if Fixedheader.Flags has qos level 1 or 2, we need to read the packet identifier

                if (FixedHeader.QoS > 0)
                {
                    PacketIdentifier = (ushort)((dataStream.ReadByte() << 8) | dataStream.ReadByte());
                }

                // Read the payload
                Payload = new byte[FixedHeader.RemainingLength - dataStream.Position];
                dataStream.Read(Payload, 0, Payload.Length);


            }
        }

        public byte[] ToByteArray()
        {
            using (var dataStream = new MemoryStream())
            {
                // Variable header
                // Write the topic name
                WriteString(dataStream, Topic);

                // If QoS level is 1 or 2, write the packet identifier
                if (QosLevel > 0)
                {
                    dataStream.WriteByte((byte)(PacketIdentifier >> 8)); // MSB
                    dataStream.WriteByte((byte)(PacketIdentifier & 0xFF)); // LSB
                }

                // Write the payload
                dataStream.Write(Payload, 0, Payload.Length);


                // Calculate the remaining length
                int variableHeaderAndPayloadLength = (int)dataStream.Length;

                // Create the fixed header with updated remaining length
                FixedHeader.RemainingLength = variableHeaderAndPayloadLength;

                FixedHeader.Flags = (byte)((DupFlag ? 0x08 : 0x00) | (QosLevel << 1) | (Retain ? 0x01 : 0x00));


                var fixedHeaderBytes = FixedHeader.ToByteArray();

                // Combine fixed header and data
                using (var finalStream = new MemoryStream())
                {
                    // Write the fixed header
                    finalStream.Write(fixedHeaderBytes, 0, fixedHeaderBytes.Length);

                    // Write the variable header and payload
                    finalStream.Write(dataStream.ToArray(), 0, variableHeaderAndPayloadLength);
                    return finalStream.ToArray();
                }
            }
        }

        private void WriteString(MemoryStream dataStream, string value)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            dataStream.WriteByte((byte)(buffer.Length >> 8)); //MSB One byte
            dataStream.WriteByte((byte)buffer.Length); //LSB One byte
            dataStream.Write(buffer, 0, buffer.Length); //Write the bytes of the string
        }

        private string ReadString(MemoryStream dataStream)
        {
            var length = (dataStream.ReadByte() << 8) | dataStream.ReadByte();
            var buffer = new byte[length];
            dataStream.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

    }
}

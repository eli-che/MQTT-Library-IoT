using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Publisher
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

    }
}

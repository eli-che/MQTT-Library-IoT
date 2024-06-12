using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MQTT_AIO
{
    public class MqttSubscribePacket
    {
        public MqttFixedHeader FixedHeader { get; set; }
        public ushort PacketIdentifier { get; set; }
        public string Topic { get; set; }

        public byte QosLevel { get; set; } = 0;

        public MqttSubscribePacket()
        {
            FixedHeader = new MqttFixedHeader
            {
                PacketType = mqttControlPacketType.SUBSCRIBE,
                Flags = 0x02,
                RemainingLength = 0

            };
        }

        public byte[] ToByteArray()
        {
            using (var dataStream = new MemoryStream())
            {
                //Write the packet identifier as it comes first after the fixed header and is a part of the variable header.
                dataStream.WriteByte((byte)(PacketIdentifier >> 8)); // MSB //First byte of the packet identifier
                dataStream.WriteByte((byte)(PacketIdentifier & 0xFF)); // LSB //Second byte of the packet identifier

                //Get the topic and write it to the data stream
                WriteString(dataStream, Topic);

                //Write the QoS level
                dataStream.WriteByte(QosLevel);

                //Calculate the remaining length
                int variableHeaderAndPayloadLength = (int)dataStream.Length;

                //update the remaining length in the fixed header
                FixedHeader.RemainingLength = variableHeaderAndPayloadLength;

                //Get the fixed header bytes
                var fixedHeaderBytes = FixedHeader.ToByteArray();

                //combine the fixed header and variable header and payload
                using (var finalStream = new MemoryStream())
                {
                    //write the fixed header bytes
                    finalStream.Write(fixedHeaderBytes, 0, fixedHeaderBytes.Length);
                    //write the variable header and payload
                    finalStream.Write(dataStream.ToArray(), 0, (int)dataStream.Length);
                    //return the combined
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

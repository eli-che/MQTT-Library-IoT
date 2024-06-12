using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MQTT_AIO
{
    public class MqttFixedHeader
    {
        public mqttControlPacketType PacketType { get; set; }
        public byte Flags { get; set; }
        public int RemainingLength { get; set; }

        //Contruct the packet into a byte array to send over network
        public byte[] ToByteArray()
        {
            using (var dataStream = new MemoryStream())
            {
                //First byte is the packet type 4 bits and flags 4 bits, so we need to shift the packet type 4 bits to the left and then OR it with the flags to ensure only the lower 4 bits get used
                byte firstByte = (byte)(((int)PacketType << 4) | (Flags & 0x0F));
                //write the byte into the dataStream
                dataStream.WriteByte(firstByte);

                //Encode the remaining length
                int value = RemainingLength;
                do
                {
                    //Extract the least significant 7 bits
                    byte encodedByte = (byte)(value % 128);
                    //Shift the value 7 bits to the right
                    value /= 128;
                    //If there are more data to encode, set the top bit of this byte
                    if (value > 0)
                    {
                        //Set the MSB to 1 if there are more bytes to encode
                        encodedByte |= 128;
                    }
                    dataStream.WriteByte(encodedByte);
                } while (value > 0);
                return dataStream.ToArray();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MQTT_Publisher
{
    public class MqttPubackReponsePacket
    {
        public ushort PacketIdentifier { get; set; }

        public MqttPubackReponsePacket(byte[] packet)
        {
            using (var stream = new MemoryStream(packet))
            {
                // Skip the fixed header (first byte)
                stream.ReadByte();

                // Read the remaining length (second byte)
                stream.ReadByte();

                // Read the packet identifier
                //The packet identifier is 16 bits long, so we will take byte 3 and shift it to the left
                //and then take byte 4 and add it to the result, making a total of 16bits.
                PacketIdentifier = (ushort)((stream.ReadByte() << 8) | stream.ReadByte());
            }
        }

    }
}

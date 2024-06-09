using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MQTT_Publisher
{
    public class MqttConnackReponsePacket
    {
        //The Variable Header of the CONNACK Packet contains the following fields in the order: Connect Acknowledge Flags, Connect Reason Code, and Properties.
        public byte ConnectAcknowledgeFlags { get; set; }
        public byte ConnectReasonCode { get; set; }

        public MqttConnackReponsePacket(byte[] packet)
        {
            //use memory stream to read the packet
            using (var stream = new MemoryStream(packet))
            {
                // Skip the fixed header (first byte)
                stream.ReadByte();

                // Read the remaining length (second byte)
                stream.ReadByte();

                //read the first byte
                ConnectAcknowledgeFlags = (byte)stream.ReadByte();
                //read the second byte
                ConnectReasonCode = (byte)stream.ReadByte();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace MQTT_AIO
{
    public class MqttConnectPacket
    {
        public MqttFixedHeader FixedHeader { get; set; }
        public string clientId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool cleanSession { get; set; }
        public ushort keepAlive { get; set; }


        public MqttConnectPacket() 
        { 
            //create a new fixed header with the packet type set to CONNECT
            FixedHeader = new MqttFixedHeader 
            { 
                PacketType = mqttControlPacketType.CONNECT,
                Flags = 0,
                RemainingLength = 0
            
            };
        }

        public byte[] ToByteArray()
        {

            using (var dataStream = new MemoryStream())
            {
                var fixedHeaderBytes = FixedHeader.ToByteArray();
                dataStream.Write(fixedHeaderBytes, 0, fixedHeaderBytes.Length);

                //Variable Header
                WriteString(dataStream, "MQTT"); //Protocol Name, 4 bytes total for "MQTT"
                dataStream.WriteByte(5); //Protocol Level, 1 byte, the version is 5
                //connect flags
                byte connectFlags = 0;
                if (cleanSession)
                {
                    connectFlags |= 0x02; //Set the clean session bit
                }
                if (!string.IsNullOrEmpty(username))
                {
                    connectFlags |= 0x80; //Set the username bit
                }
                if (!string.IsNullOrEmpty(password))
                {
                    connectFlags |= 0x40; //Set the password bit
                }
                dataStream.WriteByte(connectFlags); //Connect Flags, 1 byte

                //Keep alive
                dataStream.WriteByte((byte)(keepAlive >> 8)); //MSB
                dataStream.WriteByte((byte)(keepAlive & 0xFF)); //LSB

                //Payload with the Client ID
                WriteString(dataStream, clientId);

                //The next payload is username if it exists
                if (!string.IsNullOrEmpty(username))
                {
                    WriteString(dataStream, username);
                }
                //the next payload is password if it exists
                if (!string.IsNullOrEmpty(password))
                {
                    WriteString(dataStream, password);
                }
                //Recalculate the remaining length
                var packetBytes = dataStream.ToArray();
                FixedHeader.RemainingLength = packetBytes.Length - fixedHeaderBytes.Length; //Subtract the first byte of the fixed header which contains the packet type and flags
                //Rebuild the fixed header because of the updated remaining length
                var newFixedHeaderBytes = FixedHeader.ToByteArray();
                //Copy the new fixed header bytes into the packet bytes
                //Source array, source index, destination array, destination index, length
                Array.Copy(newFixedHeaderBytes, 0, packetBytes, 0, newFixedHeaderBytes.Length);
                return packetBytes;
            }
                
                    
        }

        private void WriteString (MemoryStream dataStream, string value)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            dataStream.WriteByte((byte)(buffer.Length >> 8)); //MSB One byte
            dataStream.WriteByte((byte)(buffer.Length & 0xFF)); //LSB One byte
            dataStream.Write(buffer, 0, buffer.Length); //Write the bytes of the string
        }

    }
}

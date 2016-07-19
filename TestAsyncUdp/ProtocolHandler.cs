using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TestAsyncUdp
{
    static public class PacketType
    {
        static public byte CS_REQ = 1;
        static public byte CS_ACK = 2;
        static public byte SC_REQ = 3;
        static public byte SC_ACK = 4;
    }
    public class Packet
    {
        public byte packet;
        public UInt32 user_id;
        public UInt16 seq;
        public List<string> objects;

        public Packet(byte _packet, UInt32 _user_id, UInt16 _seq, List<string> _objects)
        {
            packet = _packet;
            user_id = _user_id;
            seq = _seq;
            objects = _objects;
        }
    }
    public class ProtocolHandler
    {
        static UInt16 packet_seq = 1;
        //static public void Process(byte[] recv_packet)
        //{
        //    if(recv_packet[0] == PacketType.CS_ACK)

        //}
        static public Packet DecryptPacket(string jsonString)
        {
            Packet p = JsonConvert.DeserializeObject<Packet>(jsonString);
            return p;
        }
        static public string EncryptPacket(byte packet, UInt32 user_id, List<Actor> _objects)
        {
            List<string> serialize_list = new List<string>();
            BinaryFormatter bf = new BinaryFormatter();
            foreach(Actor a in _objects)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(stream, a);
                    string serialize_actor = Convert.ToBase64String(stream.ToArray());
                    serialize_list.Add(serialize_actor);
                }
            }

            var p = new Packet(packet, user_id, packet_seq++, serialize_list);
            string jsonString = JsonConvert.SerializeObject(p);

            return jsonString;
        }
    }
}

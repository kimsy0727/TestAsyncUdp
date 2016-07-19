using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestAsyncUdp
{
    [Serializable]
    public class Actor : _Serializer
    {

        public UInt64 mID;
        public UInt32 mPosX;
        public UInt32 mPosY;
        public UInt64 ID
        {
            get { return mID; }
            set { mID = value; }
        }
        public UInt32 PosX
        {
            get { return mPosX; }
            set { if (value >= 0) mPosX = value; }
        }
        public UInt32 PosY
        {
            get { return mPosY; }
            set { if (value >= 0) mPosY = value; }
        }
        public Actor()
        {
        }
        public Actor(SerializationInfo _info, StreamingContext _context)
            : base(_info, _context)
        {
        }
        public override bool CheckAvailabilityToSerialize(string _name, object _now, object _before)
        {
            if (_name.Equals("mPosX") || _name.Equals("mPosY") || _name.Equals("mID"))
            {
                if (!_now.Equals(_before))
                    return true;
                else
                    return false;
            }
            else
            {
                return base.CheckAvailabilityToSerialize(_name, _now, _before);
            }
        }
        public void tick()
        {

        }
    }
    public class Common
    {
        public static string TruncateLeft(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        // Get local IP
        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    return localIP;
                }
            }
            return "127.0.0.1";
        }
    }
}

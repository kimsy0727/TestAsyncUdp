using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TestAsyncUdp
{
    public class Peer
    {
        public IPEndPoint end_point;
        public UInt32 user_id;

        public Peer(UInt32 _user_id, IPEndPoint _end_point)
        {
            user_id = _user_id;
            end_point = _end_point;
        }
    }
}

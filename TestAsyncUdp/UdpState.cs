using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestAsyncUdp
{
    public class UdpState
    {
        public IPEndPoint e;
        public Socket s;

        public UdpState(Socket _s, IPEndPoint _e)
        {
            s = _s;
            e = _e;
        }
    }
}

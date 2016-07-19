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
        public UdpClient u;

        public UdpState(UdpClient _u, IPEndPoint _e)
        {
            u = _u;
            e = _e;
        }
    }
}

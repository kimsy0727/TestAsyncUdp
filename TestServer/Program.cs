using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestAsyncUdp;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Replicator.Instance.StartHost();
            //UdpState state = TestAsyncUdp.UdpNetModule.Listen();
            //AsyncCallback callback = new AsyncCallback(TestAsyncUdp.UdpNetModule.ReceiveCallback);
            //Thread recv_thread = new Thread(() => TestAsyncUdp.UdpNetModule.ReceiveMessages(state, callback));
            //recv_thread.Start();

            //while (true)
            //{
            //    string input = Console.ReadLine();
            //    TestAsyncUdp.UdpNetModule.Send(state.u, input);
            //}
        }
    }
}

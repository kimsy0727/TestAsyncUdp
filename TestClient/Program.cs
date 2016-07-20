using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestAsyncUdp;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Replicator.Instance.StartClient();

            while (true)
            {
                // 명령어 받았으면 호스트한테 보내자
                Replicator.Instance.SendPacket(null, Console.ReadLine());
            }
            //UdpState state = TestAsyncUdp.UdpNetModule.Connect("127.0.0.1");
            //AsyncCallback callback = new AsyncCallback(TestAsyncUdp.UdpNetModule.ReceiveCallbackForClient);
            //Thread recv_thread = new Thread(() => TestAsyncUdp.UdpNetModule.ReceiveMessages(state, callback));
            //recv_thread.Start();

            //while(true)
            //{
            //    string input = Console.ReadLine();
            //    TestAsyncUdp.UdpNetModule.Send(state.u, input);
            //}
        }
    }
}

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

            Actor a = new Actor();
            Actor b = new Actor();

            while (true)
            {
                string[] input_list = Console.ReadLine().Split(' ');
                if (input_list[0] == "move")
                {
                    List<Actor> list = new List<Actor>();
                    if (input_list[1] == "1")
                    {
                        a.PosX += 10;
                        list.Add(a);
                    }
                    else if (input_list[1] == "2")
                    {
                        b.PosY += 10;
                        list.Add(b);
                    }
                    else
                    {
                        a.PosX += 10;
                        b.PosY += 10;
                        list.Add(a);
                        list.Add(b);
                    }

                    Replicator.Instance.AddObject(list);
                }
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

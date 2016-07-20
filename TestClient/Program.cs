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
        public static void Tick()
        {
            while(true)
            {
                Dictionary<Type, Dictionary<ulong, Actor>> list = Replicator.Instance.GetObjectList();
                foreach (KeyValuePair<Type, Dictionary<ulong, Actor>> actor_list in list)
                {
                    foreach (KeyValuePair<ulong, Actor> actor in actor_list.Value)
                    {
                        Console.WriteLine("actor ID:{0} type:{1} posx:{2} posy:{3}", actor.Value.ID, actor.Value.GetType().ToString(), actor.Value.PosX, actor.Value.PosY);
                    }
                }

                Thread.Sleep(5000);
            }
        }
        static void Main(string[] args)
        {
            Replicator.Instance.StartClient();
            Thread client_thread = new Thread(() => Tick());
            client_thread.Start();

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

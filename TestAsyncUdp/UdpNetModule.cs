using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestAsyncUdp
{
    public struct SendPair
    {
        public UdpState state;
        public string packet;

        public SendPair(UdpState _state, string _packet)
        {
            state = _state;
            packet = _packet;
        }
    }
    public struct BroadCastPair
    {
        public IPEndPoint endpoint;
        public byte[] recvbytes;
        public BroadCastPair(IPEndPoint _endpoint, byte[] recvpacket)
        {
            endpoint = _endpoint;
            recvbytes = recvpacket;
        }
    }
    public class UdpNetModule
    {
        public static int listenPort = 9050;
        private Queue<SendPair> send_pool;
        private Queue<BroadCastPair> broadcast_pool;
        public UInt32 mIdentity = 0;
        Dictionary<string, Peer> mClientList;

        public UdpNetModule()
        {
            send_pool = new Queue<SendPair>();
            broadcast_pool = new Queue<BroadCastPair>();
            mClientList = new Dictionary<string, Peer>();
        }
        public UdpState Listen()
        {
            // Receive a message and write it to the console.
            IPEndPoint e = new IPEndPoint(IPAddress.Any, listenPort);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UdpState s = new UdpState(sock, e);
            sock.Bind(e);

            return s;
        }
        public void SendCallback(IAsyncResult ar)
        {
            Socket u = (Socket)((UdpState)(ar.AsyncState)).s;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            Console.WriteLine("number of bytes sent: {0}", u.EndSend(ar));
        }
        public void ReceiveCallback(UdpState ar)
        {
            Socket s = (Socket)ar.s;
            EndPoint e = (EndPoint)ar.e;

            while (true)
            {
                try
                {

                    Byte[] receiveBytes = new Byte[1024];
                    int recv = s.ReceiveFrom(receiveBytes, ref e);
                    string client_key = ((IPEndPoint)e).Address.ToString() + ((IPEndPoint)e).Port.ToString();
                    string receiveString = Encoding.ASCII.GetString(receiveBytes, 0, recv);
                    Packet packet = ProtocolHandler.DecryptPacket(receiveString);
                    Console.WriteLine("Received: {0}, RemoteAddress:{1} RemotePort:{2}", receiveString, ((IPEndPoint)e).Address, ((IPEndPoint)e).Port);
                    if (!mClientList.ContainsKey(client_key))
                    {
                        mClientList.Add(client_key, new Peer(++mIdentity, (IPEndPoint)e));
                        packet.user_id = mIdentity;
                    }
                    // 받은 객체들 처리하자.
                    /*
                     * TODO: host 에서 갱신된 actor 정보까지 포함된 object 를 패킷으로 날려주자 
                     */
                    List<Actor> list = Replicator.Instance.ProcessFunc(packet.user_id, packet.func);
                    string sendString = ProtocolHandler.EncryptPacket(packet.packet, packet.user_id, list, packet.func);
                    byte[] sendBytes = Encoding.ASCII.GetBytes(sendString);

                    foreach (KeyValuePair<string, Peer> peer in mClientList)
                    {
                        broadcast_pool.Enqueue(new BroadCastPair(peer.Value.end_point, sendBytes));
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception: {0}", exception.ToString());
                    return;
                }

                Thread.Sleep(1);
            }
        }
        public void ReceiveCallbackForClient(UdpState ar)
        {
            Socket s = (Socket)ar.s;
            EndPoint e = (EndPoint)ar.e;

            while (true)
            {
                try
                {
                    Byte[] receiveBytes = new Byte[1024];
                    int recv = s.ReceiveFrom(receiveBytes, ref e);
                    string receiveString = Encoding.ASCII.GetString(receiveBytes, 0, recv);
                    Packet packet = ProtocolHandler.DecryptPacket(receiveString);
                    Replicator.Instance.ProcessObject(packet.user_id, packet.objects);
                    // 첫 패킷의 응답이라면 내 정보 세팅하자.
                    if (packet.seq == 1 && Replicator.Instance.mClient == null)
                    {
                        Replicator.Instance.mClient = new Peer(packet.user_id, (IPEndPoint)e);
                    }

                    Console.WriteLine("Received: {0}", receiveString);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception: {0}", exception.ToString());
                    return;
                }

                Thread.Sleep(1);
            }
        }
        //public void ReceiveMessages(object state, AsyncCallback callback)
        //{
        //    Console.WriteLine("listening for messages");
        //    // Do some work while we wait for a message. For this example,
        //    // we'll just sleep
        //    while (true)
        //    {
        //        ((UdpState)state).u.BeginReceive(callback, 
        //            state);
        //        Thread.Sleep(100);
        //    }
        //}
        public UdpState Connect(string server)
        {
            //u.Connect(server, listenPort);
            IPEndPoint e = new IPEndPoint(IPAddress.Parse(server), listenPort);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Connect(e);
            UdpState s = new UdpState(sock, e);

            return s;
        }
        public void AddSendQueue(UdpState to, string packet)
        {
            send_pool.Enqueue(new SendPair(to, packet));
        }
        public void BroadCastMessages(Socket server)
        {
            while (true)
            {
                if (broadcast_pool.Count == 0)
                    Thread.Sleep(100);
                else
                {
                    BroadCastPair pair = broadcast_pool.Dequeue();
                    int send = server.SendTo(pair.recvbytes, pair.endpoint);
                    Console.WriteLine("number of bytes sent: {0}", send);
                    // send the message
                    //server.BeginSend(pair.recvbytes, pair.recvbytes.Length, pair.endpoint, SendCallback, new UdpState(server, pair.endpoint));
                }
            }
        }
        public void SendMessages()
        {
            while(true)
            {
                if (send_pool.Count == 0)
                    Thread.Sleep(100);
                else {
                    SendPair pair = send_pool.Dequeue();
                    Console.WriteLine("Send: {0}", pair.packet);
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(pair.packet);
                    // send the message
                    // the destination is defined by the call to .Connect()
                    int send = pair.state.s.Send(sendBytes);
                    Console.WriteLine("number of bytes sent: {0}", send);
                }
            }
        }
    }
}

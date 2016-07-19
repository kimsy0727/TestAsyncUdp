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
            UdpClient u = new UdpClient(e);
            UdpState s = new UdpState(u, e);

            return s;
        }
        public void SendCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            Console.WriteLine("number of bytes sent: {0}", u.EndSend(ar));
        }
        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            try
            {
                Byte[] receiveBytes = u.EndReceive(ar, ref e);
                string client_key = e.Address.ToString() + e.Port.ToString();
                string receiveString = Encoding.ASCII.GetString(receiveBytes);
                Packet packet = ProtocolHandler.DecryptPacket(receiveString);
                Console.WriteLine("Received: {0}, RemoteAddress:{1} RemotePort:{2}", receiveString, e.Address, e.Port);
                if (!mClientList.ContainsKey(client_key))
                {
                    mClientList.Add(client_key, new Peer(++mIdentity, e));
                    packet.user_id = mIdentity;
                }
                // 받은 객체들 처리하자.
                /*
                 * TODO: host 에서 갱신된 actor 정보까지 포함된 object 를 패킷으로 날려주자 
                 */
                List<Actor> list = Replicator.Instance.ProcessObject(packet.user_id, packet.objects);
                string sendString = ProtocolHandler.EncryptPacket(packet.packet, packet.user_id, list);
                byte[] sendBytes = Encoding.ASCII.GetBytes(sendString);

                foreach (KeyValuePair<string, Peer> peer in mClientList)
                {
                    broadcast_pool.Enqueue(new BroadCastPair(peer.Value.end_point, sendBytes));
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine("Exception: {0}", exception.ToString());
                return;
            }
        }
        public void ReceiveCallbackForClient(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            Byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);
            Packet packet = ProtocolHandler.DecryptPacket(receiveString);
            Replicator.Instance.ProcessObject(packet.user_id, packet.objects);
            // 첫 패킷의 응답이라면 내 정보 세팅하자.
            if (packet.seq == 1 && Replicator.Instance.mClient == null)
            {
                Replicator.Instance.mClient = new Peer(packet.user_id, e);
            }

            Console.WriteLine("Received: {0}", receiveString);
        }
        public void ReceiveMessages(object state, AsyncCallback callback)
        {
            Console.WriteLine("listening for messages");
            // Do some work while we wait for a message. For this example,
            // we'll just sleep
            while (true)
            {
                ((UdpState)state).u.BeginReceive(callback, 
                    state);
                Thread.Sleep(100);
            }
        }
        public UdpState Connect(string server)
        {
            //u.Connect(server, listenPort);
            IPEndPoint e = new IPEndPoint(IPAddress.Parse(server), listenPort);
            // create the udp socket
            UdpClient u = new UdpClient();
            u.Connect(e);
            
            UdpState s = new UdpState(u, e);

            return s;
        }
        public void SendCallbackForClient(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
            
            Console.WriteLine("number of bytes sent: {0}", u.EndSend(ar));
        }
        public void AddSendQueue(UdpState to, string packet)
        {
            send_pool.Enqueue(new SendPair(to, packet));
        }
        public void BroadCastMessages(UdpClient server)
        {
            while (true)
            {
                if (broadcast_pool.Count == 0)
                    Thread.Sleep(100);
                else
                {
                    BroadCastPair pair = broadcast_pool.Dequeue();
                    // send the message
                    server.BeginSend(pair.recvbytes, pair.recvbytes.Length, pair.endpoint, SendCallback, new UdpState(server, pair.endpoint));
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
                    pair.state.u.BeginSend(sendBytes, sendBytes.Length, new AsyncCallback(SendCallbackForClient), pair.state);
                }
            }
        }
    }
}

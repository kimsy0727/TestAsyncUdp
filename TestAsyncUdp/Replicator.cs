using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TestAsyncUdp;

namespace TestAsyncUdp
{
    public class Replicator : Singleton<Replicator>
    {
        //public Peer mClient;
        public Peer mClient;
        UdpState mHost;
        UdpNetModule mNetModule;
        // Actor List 
        Dictionary<Type, Dictionary<UInt64, Actor>> mActorList;

        public Replicator()
        {
            mNetModule = new UdpNetModule();
            mActorList = new Dictionary<Type, Dictionary<ulong, Actor>>();
        }

        public bool StartClient()
        {
            UdpState state = mNetModule.Connect("127.0.0.1");
            mHost = state;
            AsyncCallback callback = new AsyncCallback(mNetModule.ReceiveCallbackForClient);
            Thread recv_thread = new Thread(() => mNetModule.ReceiveMessages(state, callback));
            recv_thread.Start();
            Thread send_thread = new Thread(() => mNetModule.SendMessages());
            send_thread.Start();

            return true;
        }

        public bool StartHost()
        {
            UdpState state = mNetModule.Listen();
            AsyncCallback callback = new AsyncCallback(mNetModule.ReceiveCallback);
            Thread recv_thread = new Thread(() => mNetModule.ReceiveMessages(state, callback));
            recv_thread.Start();
            Thread send_thread = new Thread(() => mNetModule.SendMessages());
            send_thread.Start();
            Thread broadcast_thread = new Thread(() => mNetModule.BroadCastMessages(state.u));
            broadcast_thread.Start();

            return true;
        }
        // Actor 의 변경사항을 알리는 함수
        public Actor Update(UInt32 user_id, Actor actor)
        {
            Type type = actor.GetType();
            UInt64 objectId = actor.ID;
            // 갱신해야 하는 actor 의 type 이 Dictionary 에 존재하는지 확인하자.
            if (!mActorList.ContainsKey(type))
            {
                mActorList.Add(type, new Dictionary<ulong, Actor>());
            }

            // actor를 생성해야 하는 상황이면
            if (actor.ID == 0)
            {
                actor.ID = (ulong)mActorList[type].Count + 1;
            }

            // 갱신해야 하는 actor 의 id 가 Dictionary 에 존재하는지 확인하자.
            if (!mActorList[type].ContainsKey(actor.ID))
            {
                mActorList[type].Add(actor.ID, actor);
            }

            return actor;
        }
        public Actor GetObject(Type type, UInt64 id)
        {
            if (!mActorList.ContainsKey(type))
                return null;
            else if (!mActorList[type].ContainsKey(id))
                return null;

            return mActorList[type][id];
        }

        public void SendPacket(List<Actor> actor_list, string func)
        {
            // 클라이언트 세팅전
            UInt32 user_id;
            if (mClient == null)
                user_id = 0;
            else
                user_id = mClient.user_id;
            string packet = ProtocolHandler.EncryptPacket(1, user_id, actor_list, func);
            mNetModule.AddSendQueue(mHost, packet);
        }
        // 클라이언트가 액터 리스트를 받아 
        public void ProcessObject(UInt32 user_id, List<ObjectPacket> list)
        {
            foreach (ObjectPacket op in list)
            {
                // id를 통해 이미 액터가 존재하는지 체크
                Actor b = Replicator.Instance.GetObject(op.object_type, op.object_id);
                // 존재하면 갱신된 정보만 적용.
                if (b != null)
                {
                    b.DeserializeObject(op.object_body);
                    Console.WriteLine("actor ID:{0} posx:{1} posy:{2}", b.ID, b.PosX, b.PosY);
                    continue;
                }

                // 존재하지 않으면 새로 만든 액터를 deserialize 하자.
                Actor a = new Actor();
                a.DeserializeObject(op.object_body);
                Replicator.Instance.Update(user_id, a);

                Console.WriteLine("actor ID:{0} posx:{1} posy:{2}", a.ID, a.PosX, a.PosY);
            }
        }
        public List<Actor> ProcessFunc(UInt32 user_id, string func)
        {
            string[] param_list = func.Split(' ');
            List<Actor> list = new List<Actor>();
            UInt32 param1 = 0;
            if (param_list[0] == "move" && param_list.Length > 1 && UInt32.TryParse(param_list[1], out param1))
            {
                // 요청한 액터 가져오기
                Actor a = Replicator.Instance.GetObject(typeof(Actor), param1);
                if(a == null)
                {
                    System.Console.WriteLine("{0} object is null.", param1.ToString());
                    return list;
                }

                a.PosX += 10;
                list.Add(a);
            }
            else if (param_list[0] == "spawn")
            {
                Actor a = new Actor();
                Replicator.Instance.Update(user_id, a);
                list.Add(a);
            }

            return list;
        }
    }
}

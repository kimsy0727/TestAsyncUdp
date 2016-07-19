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
        UInt64 mActorIdentity;
        // Actor List 
        Dictionary<Type, Dictionary<UInt64, Actor>> mActorList;

        public Replicator()
        {
            mNetModule = new UdpNetModule();
            mActorList = new Dictionary<Type, Dictionary<ulong, Actor>>();
            mActorIdentity = 0;
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
            // actor 를 갱신하자.
            mActorList[type][actor.ID] = actor;

            return actor;
        }
        public Actor GetObject(Type type, UInt64 id)
        {
            return mActorList[type][id];
        }

        public void AddObject(List<Actor> actor_list)
        {
            // 클라이언트 세팅전
            UInt32 user_id;
            if (mClient == null)
                user_id = 0;
            else
                user_id = mClient.user_id;
            string packet = ProtocolHandler.EncryptPacket(1, user_id, actor_list);
            mNetModule.AddSendQueue(mHost, packet);
        }
        public List<Actor> ProcessObject(UInt32 user_id, List<string> list)
        {
            List<Actor> actor_list = new List<Actor>();
            foreach(string s in list)
            {
                // TODO: 클라가 받았을때 액터를 다시 생성하지 않고 기존에 있던 actor 를 찾아 쓸수 있는 방법 생각해보자.
                Actor a = new Actor();
                a.DeserializeObject(s);
                Update(user_id, a);
                actor_list.Add(a);
                Console.WriteLine("actor ID:{0} posx:{1} posy:{2}", a.ID, a.PosX, a.PosY);
            }

            return actor_list;
        }
    }
}

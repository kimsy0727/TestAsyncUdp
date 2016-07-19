using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// for serialization
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Reflection;
using System.IO;

namespace TestAsyncUdp
{
    public class _SerializeInfo
    {
        public List<string> m_key_list;
        public SerializationInfo m_info;

        public _SerializeInfo()
        {
            m_info = null;
            m_key_list = new List<string>();
        }
        public _SerializeInfo(Type _obj_type)
        {
            m_info = new SerializationInfo(_obj_type, new System.Runtime.Serialization.FormatterConverter());
            m_key_list = new List<string>();
        }
        public _SerializeInfo(SerializationInfo _info)
        {
            m_info = _info;
            m_key_list = new List<string>();
        }
        public bool CheckAvailableKey(string _key)
        {
            return m_key_list.Contains(_key);
        }
        public void Clear()
        {
            m_key_list.Clear();
            m_info = null;
        }
        public void SetInfo(SerializationInfo _info)
        {
            m_info = _info;
        }
        public void Add<T>(string _key, T _val)
        {
            if (m_info != null)
            {
                m_info.AddValue(_key, _val);
            }
            if (m_key_list != null)
            {
                m_key_list.Add(_key);
            }
        }
        public object GetVal(string _key, Type _type)
        {
            if (!CheckAvailableKey(_key))
            {
                return null;
            }
            else
            {
                return m_info.GetValue(_key, _type);
            }
        }
    }
    
    [Serializable]
    public class _Serializer : ISerializable
    {
        protected _SerializeInfo m_last_serialized_info;
        protected _SerializeInfo m_last_deserialized_info;

        public _Serializer(SerializationInfo _info, StreamingContext _context)
        {
            SetLastDeserializedInfo(_info);
        }
        protected void SetLastDeserializedInfo(SerializationInfo _info)
        {
            if (m_last_deserialized_info == null)
            {
                m_last_deserialized_info = new _SerializeInfo(_info);
            }
            else
            {
                m_last_deserialized_info.Clear();
                m_last_deserialized_info.SetInfo(_info);
            }
        }
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo _info, StreamingContext _context)
        {
            Serialize(_info);
        }
        public void DeserializeObject(String binaryFmt)
        {
            byte[] bytes = Convert.FromBase64String(binaryFmt);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                this.Deserialize((_Serializer)new BinaryFormatter().Deserialize(stream));
            }
        }
        public void Deserialize(_Serializer _source)
        {
            InnerDeserialize(_source.m_last_deserialized_info.m_info);
        }

        protected void InnerDeserialize(SerializationInfo _info)
        {
            if (_info == null)
                throw new System.ArgumentNullException("info");

            foreach (var entry in _info)
            {
                var _f = this.GetType().GetField(entry.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                _f.SetValue(this, entry.Value);
            }
        }
        protected void Serialize(SerializationInfo _info)
        {
            if (_info == null)
                throw new System.ArgumentNullException("info");

            _SerializeInfo _last_info = new _SerializeInfo(this.GetType());

            foreach (var _f in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                string _name = _f.Name;
                var _v = _f.GetValue(this);
                if (_v == null)
                    continue;

                if (_f.IsNotSerialized)
                    continue;

                if (_f.FieldType.Equals(typeof(_SerializeInfo)))
                    continue;

                bool _is_serializer = false;
                for (var _type = _f.FieldType; _type != null && _type != typeof(object); _type = _type.BaseType)
                {
                    if (_type.Equals(typeof(_Serializer)))
                    {
                        _is_serializer = true;
                        break;
                    }   
                }
                if (_is_serializer)
                    continue;

                if (m_last_serialized_info == null)
                {
                    return; // error
                }
                var _before_val = m_last_serialized_info.GetVal(_name, _f.FieldType);
                if (_before_val == null)
                {
                    // add when it's the first time
                    _info.AddValue(_name, _v);
                    _last_info.Add(_name, _v);
                }
                else
                {
                    if (CheckAvailabilityToSerialize(_name, _v, _before_val))
                    {
                        // add when it was modified
                        _info.AddValue(_name, _v);
                        _last_info.Add(_name, _v);
                    }
                    else
                    {
                        // nothing to do
                        _last_info.Add(_name, _v);
                    }
                }
            }
            //_last_info.SetInfo(_info);
            m_last_serialized_info = _last_info;
        }
        public _Serializer()
        {
            m_last_serialized_info = new _SerializeInfo(this.GetType());
            m_last_deserialized_info = new _SerializeInfo();
        }       
        public virtual bool CheckAvailabilityToSerialize(string _name, object _now, object _before)
        {
            if (!_now.Equals(_before))
                return true;
            return false;
        }
    }
}

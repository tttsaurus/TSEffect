using System;
using System.Runtime.Serialization;
using TS.TSLib.Serialization;
using UnityEngine;

namespace TS.TSEffect.Serialization
{
    [Serializable]
    public class SerializableObject<T> : ISerializable
    {
        public T Value { get { return _Value; } set { _Value = value; } }
        [NonSerialized]
        private T _Value;
        
        public SerializableObject(T value)
        {
            _Value = value;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (typeof(T).IsPrimitive)
            {
                info.AddValue("SerializableObject", _Value, typeof(T));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                var obj = _Value as UnityEngine.Object;
                if (obj.GetInstanceID() == 0)
                {
                    info.AddValue("SerializableObject", "null", typeof(string));
                }
                else
                {
                    string id;
                    TSEffect.Metadata.IDRefDic.TryAdd(obj, out id);
                    info.AddValue("SerializableObject", id, typeof(string));
                }
            }
            else
            {
                if (!info.TryAddValue("SerializableObject", _Value))
                {
                    Debug.LogError(string.Format("SerializableObject<{0}> failed to serialize {1}.", typeof(T).FullName, typeof(T).FullName));
                }
            }
        }
        protected SerializableObject(SerializationInfo info, StreamingContext context)
        {
            if (typeof(T).IsPrimitive)
            {
                _Value = (T)info.GetValue("SerializableObject", typeof(T));
            }
            else if(typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                string id = (string)info.GetValue("SerializableObject", typeof(string));
                _Value = (T)FormatterServices.GetUninitializedObject(typeof(T));
                if (id != "null" && !string.IsNullOrEmpty(id))
                {
                    TSEffect.AddPostDeserialization(() =>
                    {
                        UnityEngine.Object obj;
                        if (TSEffect.Metadata.IDRefDic.TryGet(id, out obj))
                        {
                            object _obj = obj;
                            _Value = (T)_obj;
                        }
                    });
                }
            }
            else
            {
                T value;
                if (info.TryGetValue("SerializableObject", out value))
                    _Value = value;
                else
                    _Value = default(T);
            }
        }
    }
}

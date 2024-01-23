using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using TS.TSEffect.Template;

namespace TS.TSEffect.Serialization
{
    [Serializable]
    public class SerializableTSEffectTemplate : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public bool SerializationTrigger { get { return _SerializationTrigger; } set { _SerializationTrigger = value; } }
        private bool _SerializationTrigger = false;
#endif

        public TSEffectTemplate Effect { get { return _Effect; } }
        [NonSerialized]
        private TSEffectTemplate _Effect = null;

        [SerializeField]
        private string _XmlText = string.Empty;
        [SerializeField]
        private string _Type = string.Empty;
        
        public void OnBeforeSerialize() 
        {
#if UNITY_EDITOR
            if (_SerializationTrigger)
            {
#endif
                Type type = Type.GetType(_Type);
                if (type != null)
                {
                    DataContractSerializer serializer = new DataContractSerializer(type);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        serializer.WriteObject(stream, _Effect);
                        stream.Position = 0;
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            _XmlText = reader.ReadToEnd();
                        }
                    }
                }
#if UNITY_EDITOR
                _SerializationTrigger = false;
            }
#endif
        }

        public void OnAfterDeserialize() 
        {
            TryCloneEffect(out _Effect);
        }

        public bool TryCloneEffect(out TSEffectTemplate effect)
        {
            effect = null;
            Type type = Type.GetType(_Type);
            if (type != null)
            {
                if (_XmlText != string.Empty)
                {
                    DataContractSerializer serializer = new DataContractSerializer(type);
                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(_XmlText)))
                    {
                        var obj = serializer.ReadObject(stream) as TSEffectTemplate;
                        if (obj != null)
                        {
                            effect = obj;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public SerializableTSEffectTemplate(TSEffectTemplate effect)
        {
            _Effect = effect;
            _Type = _Effect.GetType().FullName;
        }
    }
}

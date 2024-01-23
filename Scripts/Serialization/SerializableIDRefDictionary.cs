using System;
using UnityEngine;

namespace TS.TSEffect.Serialization
{
    [Serializable]
    public class SerializableIDRefDictionary
    {
        [SerializeField, HideInInspector]
        private SerializableDictionary<UnityEngine.Object, string> _RefIDDic = new SerializableDictionary<UnityEngine.Object, string>();
        [SerializeField]
        private SerializableDictionary<string, UnityEngine.Object> _IDRefDic = new SerializableDictionary<string, UnityEngine.Object>();
        
        public bool TryAdd(UnityEngine.Object obj, out string id) 
        {
            id = string.Empty;
            if (_RefIDDic.TryGetValue(obj, out id))
                return false;
            else
            {
                id = Guid.NewGuid().ToString();
                _RefIDDic.Add(obj, id);
                _IDRefDic.Add(id, obj);
                return true;
            }
        }
        public bool TryGet(string id, out UnityEngine.Object obj) 
        {
            return _IDRefDic.TryGetValue(id, out obj);
        }
    }
}

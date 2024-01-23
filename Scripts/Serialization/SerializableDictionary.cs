using System;
using System.Collections.Generic;
using UnityEngine;

namespace TS.TSEffect.Serialization
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> _Keys = new List<TKey>();
        [SerializeField]
        private List<TValue> _Values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            _Keys.Clear();
            _Values.Clear();
            foreach (var pair in this)
            {
                _Keys.Add(pair.Key);
                _Values.Add(pair.Value);
            }
        }
        public void OnAfterDeserialize() 
        {
            Clear();
            for (int i = 0; i < _Keys.Count; i++)
            {
                Add(_Keys[i], _Values[i]);
            }
        }
    }
}

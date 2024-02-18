using System;
using System.Collections.Generic;
using TS.TSEffect.Template;
using TS.TSEffect.Serialization;
using TS.TSEffect.Util;
using UnityEngine;

namespace TS.TSEffect.Container
{
    [Serializable]
    public class EffectContainer
    {
        public int Count { get { return _Effects.Count; } }
        [SerializeField]
        private List<SerializableTSEffectTemplate> _Effects = new List<SerializableTSEffectTemplate>();
        
        public SerializableTSEffectTemplate this[int index]
        {
            get { return _Effects[index]; }
            private set { _Effects[index] = value; }
        }

        #region Functions
        public SerializableTSEffectTemplate AddEffect(Type type)
        {
            TSEffectTemplate effect;
            if (EffectUtil.TryInstantiateEffect(type, out effect))
            {
                var ser = new SerializableTSEffectTemplate(effect);
                _Effects.Add(ser);
                return ser;
            }
            else
            {
                return null;
            }
        }
        public void RemoveEffect(SerializableTSEffectTemplate effect)
        {
            _Effects.Remove(effect);
        }
        #endregion
    }
}

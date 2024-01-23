using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using TS.TSEffect.Template;
using TS.TSEffect.Thread;
using TS.TSEffect.Util;
using TS.TSEffect.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TS.TSEffect
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TSEffectMetadata))]
    public class TSEffectMetadataEditor : UnityEditor.Editor
    {
        TSEffectMetadata Data;
        private void OnEnable()
        {
            Data = target as TSEffectMetadata;
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Reload"))
            {
                TSEffect.ReloadMetadata();
            }
        }
    }
#endif

    [Serializable]
    public class TSEffectMetadata : ScriptableObject
    {
        public struct EffectRegistration
        {
            public bool Exist;
            public TemplateRegister TemplateReg;
            public Dictionary<string, ThreadRegister> ThreadReg;
            public EffectRegistration(bool exist, TemplateRegister temp_reg, Dictionary<string, ThreadRegister> thre_reg)
            {
                Exist = exist;
                TemplateReg = temp_reg;
                ThreadReg = thre_reg;
            }
        }

        public string[] EffectTypes { get { return _EffectTypes; } }
        [SerializeField]
        private string[] _EffectTypes = new string[0];

        public List<string> BuiltinEffectCollectionsPath { get { return _BuiltinEffectCollectionsPath; } }
        [SerializeField]
        private List<string> _BuiltinEffectCollectionsPath = new List<string>();

        public Dictionary<Type, EffectRegistration> EffectCache { get { return _EffectCache; } }
        [NonSerialized]
        private Dictionary<Type, EffectRegistration> _EffectCache = new Dictionary<Type, EffectRegistration>();
        [NonSerialized]
        private bool _IsCacheInit = false;

        public SerializableIDRefDictionary IDRefDic { get { return _IDRefDic; } }
        [SerializeField]
        private SerializableIDRefDictionary _IDRefDic = new SerializableIDRefDictionary();
        
        public EffectRegistration GetEffectReg(Type type)
        {
            EffectRegistration reg;
            if (_EffectCache.TryGetValue(type, out reg))
            {
                return reg;
            }
            else
            {
                return new EffectRegistration(false, null, null);
            }
        }

        private void ForceInitCache()
        {
            _EffectCache.Clear();
            for (int i = 0; i < _EffectTypes.Length; i++)
            {
                Type type = Type.GetType(_EffectTypes[i]);
                if (type != null)
                {
                    var temp_reg = type.GetCustomAttribute(typeof(TemplateRegister)) as TemplateRegister;
                    if (temp_reg != null)
                    {
                        _EffectCache.Add(type, new EffectRegistration(true, temp_reg, ThreadUtil.GetThreadRegsFromEffect(type)));
                    }
                }
            }
            _IsCacheInit = true;
        }
        public void InitCache()
        {
            if (!_IsCacheInit)
            {
                ForceInitCache();
            }
        }
        public void RefreshTypes()
        {
            Type[] types = EffectUtil.GetEffectTypesByReflection().ToArray();
            Array.Resize(ref _EffectTypes, types.Length);
            for (int i = 0; i < _EffectTypes.Length; i++)
            {
                _EffectTypes[i] = types[i].FullName;
            }
        }
    }
}

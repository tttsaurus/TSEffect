using System;
using UnityEngine;

namespace TS.TSEffect.Thread.Universal
{
    [Serializable]
    public sealed class UniversalThread<T> : IThread
    {
#if UNITY_EDITOR
        public bool Foldout { get { return _Foldout; } set { _Foldout = value; _Varying.Foldout = value; _Triggering.Foldout = value; } }
        [SerializeField]
        private bool _Foldout = false;
#endif
        public bool Enable { get { return _Enable; } set { _Enable = value; _Varying.Enable = value; _Triggering.Enable = value; } }
        [SerializeField]
        private bool _Enable = false;

        public ThreadType ThreadType { get { return _ThreadType; } set { _ThreadType = value; } }
        [SerializeField]
        private ThreadType _ThreadType;

        [SerializeField]
        private Varying<T> _Varying = new Varying<T>();
        [SerializeField]
        private Triggering<T> _Triggering = new Triggering<T>();

        public UniversalThread(ThreadType type)
        {
            _ThreadType = type;
        }

        public void Reset()
        {
#if UNITY_EDITOR
            Foldout = false;
#endif
            Enable = false;
            switch (_ThreadType)
            {
                case ThreadType.Varying:
                    _Varying.Reset(); 
                    break;
                case ThreadType.Triggering:
                    _Triggering.Reset();
                    break;
            }
        }
        public BaseThread GetThread()
        {
            switch (_ThreadType)
            {
                case ThreadType.Varying:
                    return _Varying;
                case ThreadType.Triggering:
                    return _Triggering;
                default:
                    return null;
            }
        }
    }
}

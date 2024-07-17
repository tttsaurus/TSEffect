using System;
using UnityEngine;

namespace TS.TSEffect.Thread
{
    [Serializable]
    public class BaseThread : IThread
    {
#if UNITY_EDITOR
        public bool Foldout { get { return _Foldout; } set { _Foldout = value; } }
        [SerializeField]
        private bool _Foldout = false;
#endif
        public bool Enable { get { return _Enable; } set { _Enable = value; } }
        [SerializeField]
        private bool _Enable = false;

        public bool AutoSuspend { get { return _AutoSuspend; } set { _AutoSuspend = value; } }
        [SerializeField]
        private bool _AutoSuspend = true;

        public float Duration { get { return _Duration; } set { _Duration = value; if (_Duration < 0) _Duration = 0; } }
        [SerializeField]
        private float _Duration = 1f;

        public float InitialDelay { get { return _InitialDelay; } set { _InitialDelay = value; if (_InitialDelay < 0) _InitialDelay = 0; } }
        [SerializeField]
        private float _InitialDelay = 0f;

        public bool RecoverAfterAll { get { return _RecoverAfterAll; } set { _RecoverAfterAll = value; } }
        [SerializeField]
        private bool _RecoverAfterAll = false;

        public int Loop { get { return _Loop; } set { _Loop = value; if (_Loop < 1) _Loop = 1; } }
        [SerializeField]
        private int _Loop = 1;

        public float DelayBetweenLoops { get { return _DelayBetweenLoops; } set { _DelayBetweenLoops = value; if (_DelayBetweenLoops < 0) _DelayBetweenLoops = 0; } }
        [SerializeField]
        private float _DelayBetweenLoops = 0;
        
        public int Priority { get { return _Priority; } set { _Priority = value; } }
        [SerializeField]
        private int _Priority = 0;

        public virtual void Reset()
        {
#if UNITY_EDITOR
            _Foldout = false;
#endif
            _Enable = false;
            _AutoSuspend = true;
            _Duration = 1f;
            _InitialDelay = 0f;
            _RecoverAfterAll = false;
            _Loop = 1;
            _DelayBetweenLoops = 0;
            _Priority = 0;
        }
        public BaseThread GetThread()
        {
            return this;
        }
    }
}

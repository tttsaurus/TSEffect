using System;
using System.Collections.Generic;
using UnityEngine;
using TS.TSEffect.Thread.Cache;

namespace TS.TSEffect.Thread
{
    public class ThreadExecutor
    {
        #region Properties
        public float OverallSpan
        {
            get
            {
                return _ExeThreadCore.Thread.InitialDelay + _ExeThreadCore.Thread.Loop * (_ExeThreadCore.Thread.DelayBetweenLoops + _ExeThreadCore.Thread.Duration) - _ExeThreadCore.Thread.DelayBetweenLoops;
            }
        }
        public float RelativePerc
        {
            get
            {
                if (_IsInitDelay)
                {
                    return _RelativeTimer / _ExeThreadCore.Thread.InitialDelay;
                }
                else if (_IsLoopDelay)
                {
                    return _RelativeTimer / _ExeThreadCore.Thread.DelayBetweenLoops;
                }
                else
                {
                    return _RelativeTimer / _ExeThreadCore.Thread.Duration;
                }
            }
        }
        public float OverallPerc
        {
            get
            {
                return _OverallTimer / OverallSpan;
            }
        }
        public int Channel { get { return _Channel; } }
        private int _Channel;
        public bool IsSuspended { get { return _IsSuspended; } }
        private bool _IsSuspended = false;
        public bool IsFinished { get { return _IsFinished; } }
        private bool _IsFinished = false;
        public bool IsPaused { get { return _IsPaused; } }
        private bool _IsPaused = false;
        public float RelativeTimer { get { return _RelativeTimer; } }
        private float _RelativeTimer = 0f;
        public float OverallTimer { get { return _OverallTimer; } }
        private float _OverallTimer = 0f;
        public int LoopCounter { get { return _LoopCounter; } }
        private int _LoopCounter = 0;
        public bool IsInitDelay { get { return _IsInitDelay; } }
        private bool _IsInitDelay = true;
        public bool IsLoopDelay { get { return _IsLoopDelay; } }
        private bool _IsLoopDelay = false;
        public ExecutableThreadCore ExeThreadCore { get { return _ExeThreadCore; } }
        private ExecutableThreadCore _ExeThreadCore;
        public string ExecutorID { get { return _ExecutorID; } }
        private string _ExecutorID;
        #endregion

        private Action _RemoveCallback;
        private Action _SuspendCallback;

        private CacheDict _RuntimeCacheDict = new CacheDict();
        private Dictionary<Component, ExeFuncBundle> _RuntimeComs = new Dictionary<Component, ExeFuncBundle>();

        public void Update()
        {
            if (_IsSuspended) return;
            if (_IsPaused) return;
            if (_IsFinished) return;

            float delta = Time.deltaTime;
            _RelativeTimer += delta;
            _OverallTimer += delta;
            if (_IsInitDelay)
            {
                if (_RelativeTimer >= _ExeThreadCore.Thread.InitialDelay)
                {
                    // reset timer
                    _RelativeTimer -= _ExeThreadCore.Thread.InitialDelay;
                    _IsInitDelay = false;
                    foreach (var pair in _RuntimeComs)
                    {
                        _RuntimeCacheDict.SetUser(pair.Key.GetInstanceID());
                        pair.Value.InitExecute(_RuntimeCacheDict);
                    }
                }
            }
            else
            {
                if (_IsLoopDelay)
                {
                    if (_RelativeTimer >= _ExeThreadCore.Thread.DelayBetweenLoops)
                    {
                        // reset timer
                        _RelativeTimer -= _ExeThreadCore.Thread.DelayBetweenLoops;
                        _IsLoopDelay = false;
                        foreach (var pair in _RuntimeComs)
                        {
                            _RuntimeCacheDict.SetUser(pair.Key.GetInstanceID());
                            pair.Value.InitExecute(_RuntimeCacheDict);
                        }
                    }
                }
                else
                {
                    if (_RelativeTimer >= _ExeThreadCore.Thread.Duration)
                    {
                        foreach (var pair in _RuntimeComs)
                        {
                            _RuntimeCacheDict.SetUser(pair.Key.GetInstanceID());
                            pair.Value.FinalExecute(_RuntimeCacheDict);
                        }
                        _LoopCounter++;
                        if (_LoopCounter >= _ExeThreadCore.Thread.Loop)
                        {
                            _RelativeTimer = _ExeThreadCore.Thread.Duration;
                            _OverallTimer = _ExeThreadCore.Thread.InitialDelay + _ExeThreadCore.Thread.Loop * (_ExeThreadCore.Thread.DelayBetweenLoops + _ExeThreadCore.Thread.Duration) - _ExeThreadCore.Thread.DelayBetweenLoops;
                            _IsFinished = true;
                            if (_ExeThreadCore.Thread.AutoSuspend)
                            {
                                Suspend();
                                _SuspendCallback();
                            }
                            else
                                _RemoveCallback();
                        }
                        else
                        {
                            // reset timer
                            _RelativeTimer -= _ExeThreadCore.Thread.Duration;
                            _IsLoopDelay = true;
                        }
                    }
                    else
                    {
                        if (_ExeThreadCore.ThreadType == ThreadType.Varying)
                        {
                            foreach (var pair in _RuntimeComs)
                            {
                                _RuntimeCacheDict.SetUser(pair.Key.GetInstanceID());
                                pair.Value.OnExecute(_RelativeTimer / _ExeThreadCore.Thread.Duration, _RuntimeCacheDict);
                            }
                        }
                        if (_ExeThreadCore.ThreadType == ThreadType.Triggering)
                        {
                            foreach (var pair in _RuntimeComs)
                            {
                                _RuntimeCacheDict.SetUser(pair.Key.GetInstanceID());
                                pair.Value.OnExecute(_RelativeTimer, _RuntimeCacheDict);
                            }
                        }
                    }
                }
            }
        }
        public void Pause()
        {
            if (!_IsPaused)
            {
                _IsPaused = true;
            }
        }
        public void Continue()
        {
            if (_IsPaused)
            {
                _IsPaused = false;
            }
        }
        public void Suspend()
        {
            if (!_IsSuspended)
            {
                _IsSuspended = true;
                RuntimeInit();
            }
        }
        public void Desuspend()
        {
            if (_IsSuspended)
            {
                _IsSuspended = false;
            }
        }
        public void RuntimeInit()
        {
            _IsFinished = false;
            _IsPaused = false;
            _RelativeTimer = 0;
            _OverallTimer = 0;
            _LoopCounter = 0;
            _IsInitDelay = true;
            _IsLoopDelay = false;
            _RuntimeCacheDict.Clear();
            _RuntimeComs.Clear();
        }
        public void ResetRuntimeTargets(List<Component> coms)
        {
            _RuntimeComs.Clear();
            for (int i = 0; i < coms.Count; i++)
            {
                var com = coms[i];
                if (com.GetType().FullName == _ExeThreadCore.TargetType)
                {
                    var a = _ExeThreadCore.InitExecuteBuilder(com);
                    var b = _ExeThreadCore.FinalExecuteBuilder(com);
                    var c = _ExeThreadCore.OnExecuteBuilder(com);
                    if (a != null && b != null && c != null)
                    {
                        _RuntimeComs.Add(com, new ExeFuncBundle(a, b, c));
                    }
                }
            }
        }
        public bool AddRuntimeTarget<T>(T com, int channel) where T : Component
        {
            if (channel == _Channel)
            {
                if (typeof(T).FullName == _ExeThreadCore.TargetType)
                {
                    if (!_RuntimeComs.ContainsKey(com))
                    {
                        var a = _ExeThreadCore.InitExecuteBuilder(com);
                        var b = _ExeThreadCore.FinalExecuteBuilder(com);
                        var c = _ExeThreadCore.OnExecuteBuilder(com);
                        if (a != null && b != null && c != null)
                        {
                            _RuntimeComs.Add(com, new ExeFuncBundle(a, b, c));
                            return true;
                        }
                        else
                        { 
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        public bool RemoveRuntimeTarget<T>(T com, int channel) where T : Component
        {
            if (channel == _Channel)
            {
                if (typeof(T).FullName == _ExeThreadCore.TargetType)
                {
                    if (_RuntimeComs.ContainsKey(com))
                    {
                        _RuntimeComs.Remove(com);
                        return true;
                    }
                }
            }
            return false;
        }
        public void SetRemoveCallback(Action callback)
        {
            _RemoveCallback = callback;
        }
        public void SetSuspendCallback(Action callback)
        {
            _SuspendCallback = callback;
        }
        public ThreadExecutor(ExecutableThreadCore core, int channel, string executor_id)
        {
            _ExeThreadCore = core;
            _Channel = channel;
            _ExecutorID = executor_id;
            _IsSuspended = false;
            RuntimeInit();
        }
    }
}

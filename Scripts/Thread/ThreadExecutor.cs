using System;
using System.Collections.Generic;
using UnityEngine;
using TS.TSEffect.Thread.Cache;

namespace TS.TSEffect.Thread
{
    public class ThreadExecutorComparer : IComparer<ThreadExecutor>
    {
        public int Compare(ThreadExecutor x, ThreadExecutor y)
        {
            int res = 0;
            if (x.ExeThreadCore.Thread.Priority <= y.ExeThreadCore.Thread.Priority)
                res = -1;
            else
                res = 1;
            if (x.GetHashCode() == y.GetHashCode())
                res = 0;
            return res;
        }
    }

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
        public bool IsFinish { get { return _IsFinish; } }
        private bool _IsFinish = false;
        public bool IsPause { get { return _IsPause; } }
        private bool _IsPause = false;
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
        #endregion

        private Action _RemoveCallback;
        private Action _SuspendCallback;

        private CacheDic _RuntimeCachePool = new CacheDic();
        private Dictionary<Component, ExeFuncBundle> _RuntimeDict = new Dictionary<Component, ExeFuncBundle>();

        public void Update()
        {
            if (!_IsSuspended)
            {
                if (!_IsPause)
                {
                    if (!_IsFinish)
                    {
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
                                foreach (var pair in _RuntimeDict)
                                {
                                    _RuntimeCachePool.SetUser(pair.Key.GetInstanceID());
                                    pair.Value.InitExecute(_RuntimeCachePool);
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
                                    foreach (var pair in _RuntimeDict)
                                    {
                                        _RuntimeCachePool.SetUser(pair.Key.GetInstanceID());
                                        pair.Value.InitExecute(_RuntimeCachePool);
                                    }
                                }
                            }
                            else
                            {
                                if (_RelativeTimer >= _ExeThreadCore.Thread.Duration)
                                {
                                    foreach (var pair in _RuntimeDict)
                                    {
                                        _RuntimeCachePool.SetUser(pair.Key.GetInstanceID());
                                        pair.Value.FinalExecute(_RuntimeCachePool);
                                    }
                                    _LoopCounter++;
                                    if (_LoopCounter >= _ExeThreadCore.Thread.Loop)
                                    {
                                        _RelativeTimer = _ExeThreadCore.Thread.Duration;
                                        _OverallTimer = _ExeThreadCore.Thread.InitialDelay + _ExeThreadCore.Thread.Loop * (_ExeThreadCore.Thread.DelayBetweenLoops + _ExeThreadCore.Thread.Duration) - _ExeThreadCore.Thread.DelayBetweenLoops;
                                        _IsFinish = true;
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
                                        foreach (var pair in _RuntimeDict)
                                        {
                                            _RuntimeCachePool.SetUser(pair.Key.GetInstanceID());
                                            pair.Value.OnExecute(_RelativeTimer / _ExeThreadCore.Thread.Duration, _RuntimeCachePool);
                                        }
                                    }
                                    if (_ExeThreadCore.ThreadType == ThreadType.Triggering)
                                    {
                                        foreach (var pair in _RuntimeDict)
                                        {
                                            _RuntimeCachePool.SetUser(pair.Key.GetInstanceID());
                                            pair.Value.OnExecute(_RelativeTimer, _RuntimeCachePool);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void Pause()
        {
            _IsPause = true;
        }
        public void Continue()
        {
            _IsPause = false;
        }
        public void Suspend()
        {
            _IsSuspended = true;
            RuntimeInit();
        }
        public void Desuspend()
        {
            _IsSuspended = false;
        }
        public void RuntimeInit()
        {
            _IsFinish = false;
            _IsPause = false;
            _RelativeTimer = 0;
            _OverallTimer = 0;
            _LoopCounter = 0;
            _IsInitDelay = true;
            _IsLoopDelay = false;
            _RuntimeCachePool.Clear();
            _RuntimeDict.Clear();
        }
        public void ResetRuntimeTargets(List<Component> coms)
        {
            _RuntimeDict.Clear();
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
                        _RuntimeDict.Add(com, new ExeFuncBundle(a, b, c));
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
                    if (!_RuntimeDict.ContainsKey(com))
                    {
                        var a = _ExeThreadCore.InitExecuteBuilder(com);
                        var b = _ExeThreadCore.FinalExecuteBuilder(com);
                        var c = _ExeThreadCore.OnExecuteBuilder(com);
                        if (a != null && b != null && c != null)
                        {
                            _RuntimeDict.Add(com, new ExeFuncBundle(a, b, c));
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
                    if (_RuntimeDict.ContainsKey(com))
                    {
                        var c = com as Component;
                        _RuntimeDict.Remove(c);
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
        public ThreadExecutor(ExecutableThreadCore core, int channel)
        {
            _ExeThreadCore = core;
            _Channel = channel;
            _IsSuspended = false;
            RuntimeInit();
        }
    }
}

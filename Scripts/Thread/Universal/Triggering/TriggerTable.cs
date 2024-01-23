using System;
using System.Collections.Generic;
using UnityEngine;
using TS.TSEffect.Thread.Cache;
using TS.TSEffect.Serialization;

namespace TS.TSEffect.Thread.Universal
{
    [Serializable]
    public class TriggerTable<T>
    {
#if UNITY_EDITOR
        public bool Foldout = false;
#endif

        [SerializeField]
        private List<float> _KeyTime = new List<float>();
        [SerializeField]
        private List<SerializableObject<T>> _KeyValue = new List<SerializableObject<T>>();
        
        // Runtime
        [NonSerialized]
        private Action<int> _SetIndex;
        [NonSerialized]
        private Func<int> _GetIndex;

        private static Comparison<Tuple<float, SerializableObject<T>>> _RowComparer = new Comparison<Tuple<float, SerializableObject<T>>>((a, b) =>
        {
            if (a.Item1 < b.Item1)
                return -1;
            else if (a.Item1 == b.Item1)
                return 0;
            else
                return 1;
        });

        public int Count { get { return _KeyTime.Count; } }
        public bool Sorted { get { return _Sorted; } }
        [SerializeField]
        private bool _Sorted = false;

        public void AddRow(float time, T value)
        {
            _KeyTime.Add(time);
            _KeyValue.Add(new SerializableObject<T>(value));
            _Sorted = false;
        }
        public Tuple<float, T> GetRow(int index)
        {
            if (index < _KeyTime.Count && index >= 0)
            {
                return new Tuple<float, T>(_KeyTime[index], _KeyValue[index].Value);
            }
            else
            {
                return null;
            }
        }
        public bool SetKeyTime(int index, float time)
        {
            if (index < _KeyTime.Count && index >= 0)
            {
                _KeyTime[index] = time;
                _Sorted = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool SetKeyValue(int index, T value)
        {
            if (index < _KeyTime.Count && index >= 0)
            {
                _KeyValue[index].Value = value;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool RemoveAt(int index)
        {
            if (index < _KeyTime.Count && index >= 0)
            {
                _KeyTime.RemoveAt(index);
                _KeyValue.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Sort()
        {
            List<Tuple<float, SerializableObject<T>>> list = new List<Tuple<float, SerializableObject<T>>>();
            for (int i = 0; i < _KeyTime.Count; i++)
            {
                list.Add(new Tuple<float, SerializableObject<T>>(_KeyTime[i], _KeyValue[i]));
            }
            list.Sort(_RowComparer);
            for (int i = 0; i < _KeyTime.Count; i++)
            {
                _KeyTime[i] = list[i].Item1;
                _KeyValue[i] = list[i].Item2;
            }
            _Sorted = true;
        }

        public void BindCache(CacheDic cache)
        {
            _SetIndex = (i) =>
            {
                CacheObj obj = new CacheObj();
                obj.Value_Int = i;
                cache.Overwrite("tt_index", obj);
            };
            _GetIndex = () =>
            {
                return cache.GetValue("tt_index").Value_Int;
            };
            _SetIndex(0);
            if (!_Sorted) Sort();
        }
        public bool TryEvaluate(float time, float right_bound, out T res)
        {
            res = default;
            if (time < 0 || time > right_bound) return false;

            int index = _GetIndex();
            bool flag = false;
            float trigger_time = 0;

            if (index == _KeyTime.Count)
                trigger_time = right_bound + 1f;
            else
                trigger_time = _KeyTime[index];

            while (time >= trigger_time)
            {
                index++;
                if (index == _KeyTime.Count)
                    trigger_time = right_bound + 1f;
                else
                    trigger_time = _KeyTime[index];
                flag = true;
            }
            if (flag)
            {
                _SetIndex(index);
                res = _KeyValue[index - 1].Value;
                return true;
            }

            return false;
        }
    }
}

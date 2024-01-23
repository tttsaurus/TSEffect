using System.Collections.Generic;

namespace TS.TSEffect.Thread.Cache
{
    public class CacheDic
    {
        private int _User = 0;
        private Dictionary<string, CacheObj> _Dic = new Dictionary<string, CacheObj>();
        public void SetUser(int user)
        {
            _User = user;
        }
        public void Overwrite(string key, CacheObj value)
        {
            string k = _User.ToString() + "_" + key;
            if (_Dic.ContainsKey(k))
                _Dic[k] = value;
            else
                _Dic.Add(k, value);
        }
        public bool Remove(string key)
        {
            return _Dic.Remove(_User.ToString() + "_" + key);
        }
        public CacheObj GetValue(string key)
        {
            return _Dic[_User.ToString() + "_" + key];
        }
        public bool TryGetValue(string key, out CacheObj value)
        {
            return _Dic.TryGetValue(_User.ToString() + "_" + key, out value);
        }
        public void Clear()
        {
            _Dic.Clear();
        }
    }
}

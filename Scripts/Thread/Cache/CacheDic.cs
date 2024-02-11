using System.Collections.Generic;

namespace TS.TSEffect.Thread.Cache
{
    public class CacheDict
    {
        private int _User = 0;
        private Dictionary<string, CacheObj> _Dict = new Dictionary<string, CacheObj>();
        public void SetUser(int user)
        {
            _User = user;
        }
        public void Overwrite(string key, CacheObj value)
        {
            string k = _User.ToString() + "_" + key;
            if (_Dict.ContainsKey(k))
                _Dict[k] = value;
            else
                _Dict.Add(k, value);
        }
        public bool Remove(string key)
        {
            return _Dict.Remove(_User.ToString() + "_" + key);
        }
        public CacheObj GetValue(string key)
        {
            return _Dict[_User.ToString() + "_" + key];
        }
        public bool TryGetValue(string key, out CacheObj value)
        {
            return _Dict.TryGetValue(_User.ToString() + "_" + key, out value);
        }
        public void Clear()
        {
            _Dict.Clear();
        }
    }
}

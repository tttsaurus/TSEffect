using UnityEngine;
using TS.TSEffect.Thread.Cache;

namespace TS.TSEffect.Thread
{
    public delegate NormalExeFunc NormalExeFuncBuilder(Component target);
    public delegate TimeExeFunc TimeExeFuncBuilder(Component target);
    public delegate void NormalExeFunc(CacheDict cache);
    public delegate void TimeExeFunc(float time, CacheDict cache);
}

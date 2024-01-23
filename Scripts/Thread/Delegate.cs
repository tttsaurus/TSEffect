using UnityEngine;
using TS.TSEffect.Thread.Cache;

namespace TS.TSEffect.Thread
{
    public delegate NormalExeFunc NormalExeFuncBuilder(Component target);
    public delegate TimeExeFunc TimeExeFuncBuilder(Component target);
    public delegate void NormalExeFunc(CacheDic cache);
    public delegate void TimeExeFunc(float time, CacheDic cache);
}

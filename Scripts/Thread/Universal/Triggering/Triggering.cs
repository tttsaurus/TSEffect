using System;

namespace TS.TSEffect.Thread.Universal
{
    [Serializable]
    public sealed class Triggering<T> : BaseThread
    {
        public TriggerTable<T> Behavior = new TriggerTable<T>();

        public override void Reset()
        {
            base.Reset();
            Behavior = new TriggerTable<T>();
        }
    }
}

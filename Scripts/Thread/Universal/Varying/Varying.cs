using UnityEngine;
using System;

namespace TS.TSEffect.Thread.Universal
{
    [Serializable]
    public sealed class Varying<T> : BaseThread
    {
        public T Target = default;
        public AnimationCurve Behavior = new AnimationCurve();
        public DataVaryingMode Mode = DataVaryingMode.Increment;

        public override void Reset()
        {
            base.Reset();
            Target = default;
            Behavior = new AnimationCurve();
            Mode = DataVaryingMode.Increment;
        }
    }
}

using System.Collections.Generic;

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
}


namespace TS.TSEffect.Thread
{
    public struct ExeFuncBundle
    {
        public NormalExeFunc InitExecute;
        public NormalExeFunc FinalExecute;
        public TimeExeFunc OnExecute;
        public ExeFuncBundle(NormalExeFunc init_exe, NormalExeFunc final_exe, TimeExeFunc on_exe)
        {
            InitExecute = init_exe;
            FinalExecute = final_exe;
            OnExecute = on_exe;
        }
    }
}

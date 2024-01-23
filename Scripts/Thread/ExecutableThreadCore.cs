using System;
using TS.TSEffect.Thread.Universal;

namespace TS.TSEffect.Thread
{
    public class ExecutableThreadCore
    {
        public string Name { get { return _Name; } }
        private string _Name;
        public string TargetType { get { return _TargetType; } }
        private string _TargetType;
        public ThreadType ThreadType { get { return _ThreadType; } }
        private ThreadType _ThreadType;
        public BaseThread Thread { get { return _Thread; } }
        private BaseThread _Thread;

        public NormalExeFuncBuilder InitExecuteBuilder { get { return _InitExecuteBuilder; } }
        private NormalExeFuncBuilder _InitExecuteBuilder;
        public NormalExeFuncBuilder FinalExecuteBuilder { get { return _FinalExecuteBuilder; } }
        private NormalExeFuncBuilder _FinalExecuteBuilder;
        public TimeExeFuncBuilder OnExecuteBuilder { get { return _OnExecuteBuilder; } }
        private TimeExeFuncBuilder _OnExecuteBuilder;

        public ExecutableThreadCore(string name, string target_type, BaseThread thread, NormalExeFuncBuilder init_exe_builder, NormalExeFuncBuilder final_exe_builder, TimeExeFuncBuilder on_exe_builder)
        {
            _Name = name;
            _TargetType = target_type;
            Type t = thread.GetType().GetGenericTypeDefinition();
            if (t == typeof(Varying<>))
            {
                _ThreadType = ThreadType.Varying;
            }
            else if (t == typeof(Triggering<>))
            {
                _ThreadType = ThreadType.Triggering;
            }
            _Thread = thread;
            _InitExecuteBuilder = init_exe_builder;
            _FinalExecuteBuilder = final_exe_builder;
            _OnExecuteBuilder = on_exe_builder;
        }
    }
}

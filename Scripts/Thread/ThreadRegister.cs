using System;

namespace TS.TSEffect.Thread
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ThreadRegister : Attribute
    {
        public string DisplayName;
        public ThreadRegister(string display_name)
        {
            DisplayName = display_name;
        }
    }
}

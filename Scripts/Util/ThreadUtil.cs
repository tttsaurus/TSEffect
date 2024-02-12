using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TS.TSEffect.Template;
using TS.TSEffect.Thread;
using TS.TSEffect.Thread.Universal;

namespace TS.TSEffect.Util
{
    public static class ThreadUtil
    {
        [Serializable]
        public enum ReflectionMode
        {
            Field = 1,
            Property = 2,
            Combined = 3,
        }
        public static void BuildThreadsFromComponent<TThread, TCom>(ReflectionMode r_mode, out ReflectionMode[] member_r_modes, out string[] member_names, out TThread[] threads) where TThread : BaseThread where TCom : Component
        {
            member_r_modes = new ReflectionMode[0];
            member_names = new string[0];
            threads = new TThread[0];

            #region Reflection
            Type target_type = null;
            Type[] args = typeof(TThread).GetGenericArguments();
            if (args.Length == 0)
            {
                return;
            }
            else
            {
                if (typeof(TThread).GetGenericTypeDefinition() == typeof(Varying<>))
                {
                    target_type = args[0];
                }
                else if (typeof(TThread).GetGenericTypeDefinition() == typeof(Triggering<>))
                {
                    target_type = args[0];
                }
                else
                {
                    return;
                }
            }

            FieldInfo[] field_infos = null;
            PropertyInfo[] property_infos = null;

            List<ReflectionMode> _member_r_modes = new List<ReflectionMode>();
            List<string> _member_names = new List<string>();
            List<TThread> _threads = new List<TThread>();

            Type type = typeof(TCom);

            switch (r_mode)
            {
                case ReflectionMode.Field:
                    field_infos = type.GetFields();
                    break;
                case ReflectionMode.Property:
                    property_infos = type.GetProperties();
                    break;
                case ReflectionMode.Combined:
                    field_infos = type.GetFields();
                    property_infos = type.GetProperties();
                    break;
            }
            switch (r_mode)
            {
                case ReflectionMode.Field:
                    for (int i = 0; i < field_infos.Length; i++)
                    {
                        if (field_infos[i].FieldType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Field);
                            _member_names.Add(field_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    break;
                case ReflectionMode.Property:
                    for (int i = 0; i < property_infos.Length; i++)
                    {
                        if (property_infos[i].PropertyType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Property);
                            _member_names.Add(property_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    break;
                case ReflectionMode.Combined:
                    for (int i = 0; i < field_infos.Length; i++)
                    {
                        if (field_infos[i].FieldType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Field);
                            _member_names.Add(field_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    for (int i = 0; i < property_infos.Length; i++)
                    {
                        if (property_infos[i].PropertyType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Property);
                            _member_names.Add(property_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    break;
            }

            member_r_modes = _member_r_modes.ToArray();
            member_names = _member_names.ToArray();
            threads = _threads.ToArray();
            #endregion
        }
        public static bool TryBuildThreadsFromComponent<TThread>(Type com_type, ReflectionMode r_mode, out ReflectionMode[] member_r_modes, out string[] member_names, out TThread[] threads) where TThread : BaseThread
        {
            member_r_modes = new ReflectionMode[0];
            member_names = new string[0];
            threads = new TThread[0];

            #region Reflection
            if (!typeof(Component).IsAssignableFrom(com_type)) return false;
            Type target_type = null;
            Type[] args = typeof(TThread).GetGenericArguments();
            if (args.Length == 0)
            {
                return false;
            }
            else
            {
                if (typeof(TThread).GetGenericTypeDefinition() == typeof(Varying<>))
                {
                    target_type = args[0];
                }
                else if (typeof(TThread).GetGenericTypeDefinition() == typeof(Triggering<>))
                {
                    target_type = args[0];
                }
                else
                {
                    return false;
                }
            }

            FieldInfo[] field_infos = null;
            PropertyInfo[] property_infos = null;

            List<ReflectionMode> _member_r_modes = new List<ReflectionMode>();
            List<string> _member_names = new List<string>();
            List<TThread> _threads = new List<TThread>();

            switch (r_mode)
            {
                case ReflectionMode.Field:
                    field_infos = com_type.GetFields();
                    break;
                case ReflectionMode.Property:
                    property_infos = com_type.GetProperties();
                    break;
                case ReflectionMode.Combined:
                    field_infos = com_type.GetFields();
                    property_infos = com_type.GetProperties();
                    break;
            }
            switch (r_mode)
            {
                case ReflectionMode.Field:
                    for (int i = 0; i < field_infos.Length; i++)
                    {
                        if (field_infos[i].FieldType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Field);
                            _member_names.Add(field_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    break;
                case ReflectionMode.Property:
                    for (int i = 0; i < property_infos.Length; i++)
                    {
                        if (property_infos[i].PropertyType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Property);
                            _member_names.Add(property_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    break;
                case ReflectionMode.Combined:
                    for (int i = 0; i < field_infos.Length; i++)
                    {
                        if (field_infos[i].FieldType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Field);
                            _member_names.Add(field_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    for (int i = 0; i < property_infos.Length; i++)
                    {
                        if (property_infos[i].PropertyType == target_type)
                        {
                            _member_r_modes.Add(ReflectionMode.Property);
                            _member_names.Add(property_infos[i].Name);
                            var t = Activator.CreateInstance(typeof(TThread)) as TThread;
                            t.Reset();
                            _threads.Add(t);
                        }
                    }
                    break;
            }

            member_r_modes = _member_r_modes.ToArray();
            member_names = _member_names.ToArray();
            threads = _threads.ToArray();

            #endregion

            return true;
        }
        public static Dictionary<string, ThreadRegister> GetThreadRegsFromEffect(Type t)
        {
            if (!typeof(TSEffectTemplate).IsAssignableFrom(t)) return new Dictionary<string, ThreadRegister>();

            Dictionary<string, ThreadRegister> dic = new Dictionary<string, ThreadRegister>();
            FieldInfo[] info = t.GetFields();

            for (int i = 0; i < info.Length; i++)
            {
                var thre_reg = info[i].GetCustomAttribute(typeof(ThreadRegister)) as ThreadRegister;
                if (thre_reg != null)
                {
                    dic.Add(info[i].Name, thre_reg);
                }
            }
            return dic;
        }
        public static List<BaseThread> GetThreadsFromEffect(TSEffectTemplate owner, string field)
        {
            var value = owner.GetType().GetField(field).GetValue(owner);

            if (value == null) return new List<BaseThread>();

            Type type = value.GetType();

            List<BaseThread> list = new List<BaseThread>();
            if (typeof(IThread).IsAssignableFrom(type))
            {
                var t = value as IThread;
                list.Add(t.GetThread());
            }
            if (type.IsArray)
            {
                var elem_type = type.GetElementType();
                if (typeof(IThread).IsAssignableFrom(elem_type))
                {
                    Array arr = value as Array;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var t = arr.GetValue(i) as IThread;
                        list.Add(t.GetThread());
                    }
                }
            }
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var gene_type = type.GetGenericArguments();
                    if (gene_type.Length == 1)
                    {
                        if (typeof(IThread).IsAssignableFrom(gene_type[0]))
                        {
                            IList l = value as IList;
                            for (int i = 0; i < l.Count; i++)
                            {
                                var t = l[i] as IThread;
                                list.Add(t.GetThread());
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}

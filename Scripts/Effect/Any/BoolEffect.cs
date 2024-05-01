using TS.TSEffect.Template;
using TS.TSEffect.Thread;
using TS.TSEffect.Thread.Universal;
using TS.TSEffect.Thread.Cache;
using TS.TSEffect.Util;
using TS.TSLib.Util;
using TS.TSLib.Accessor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using TS.TSEffect.Editor;
#endif

namespace TS.TSEffect.Effect
{
    [Serializable, TemplateRegister(true, "Any/Bool", "#CA69Cf", typeof(MonoBehaviour))]
    public sealed class BoolEffect : TSEffectTemplate
    {
        public ThreadUtil.ReflectionMode RMode;
        public ThreadUtil.ReflectionMode[] MemberRModes;
        public string[] BoolNames;

        [ThreadRegister("BoolThread")]
        public Triggering<bool>[] BoolThreads;

        public override void GetExeFuncBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            base.GetExeFuncBuilders(thread_field_name, out init_exe, out final_exe, out on_exe);
            
            #region Bool
            if (thread_field_name == "BoolThreads")
            {
                for (int i = 0; i < BoolThreads.Length; i++)
                {
                    int tmp = i;
                    init_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        return (CacheDict cache) =>
                        {
                            BoolThreads[tmp].Behavior.BindCache(cache);
                            #region Reflection Get
                            switch (MemberRModes[tmp])
                            {
                                case ThreadUtil.ReflectionMode.Field:
                                    {
                                        CacheObj obj = new CacheObj();
                                        obj.Value_Bool = (bool)tar.GetType().GetField(BoolNames[tmp]).GetValue(tar);
                                        cache.Overwrite("initial", obj);
                                    }
                                    break;
                                case ThreadUtil.ReflectionMode.Property:
                                    {
                                        Func<bool> func = null;
                                        if (accessor_cache.Getters.TryGetValue(BoolNames[tmp], out func))
                                        {
                                            CacheObj obj = new CacheObj();
                                            obj.Value_Bool = func();
                                            cache.Overwrite("initial", obj);
                                        }
                                        else
                                        {
                                            CacheObj obj = new CacheObj();
                                            obj.Value_Bool = (bool)tar.GetType().GetProperty(BoolNames[tmp]).GetValue(tar);
                                            cache.Overwrite("initial", obj);
                                        }
                                    }
                                    break;
                            }
                            #endregion
                        };
                    });
                    final_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        return (CacheDict cache) =>
                        {
                            if (BoolThreads[tmp].Resume)
                            {
                                #region Reflection Set
                                switch (MemberRModes[tmp])
                                {
                                    case ThreadUtil.ReflectionMode.Field:
                                        tar.GetType().GetField(BoolNames[tmp]).SetValue(tar, cache.GetValue("initial").Value_Bool);
                                        break;
                                    case ThreadUtil.ReflectionMode.Property:
                                        Action<bool> func = null;
                                        if (accessor_cache.Setters.TryGetValue(BoolNames[tmp], out func))
                                        {
                                            func(cache.GetValue("initial").Value_Bool);
                                        }
                                        else
                                        {
                                            tar.GetType().GetProperty(BoolNames[tmp]).SetValue(tar, cache.GetValue("initial").Value_Bool);
                                        }
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                EvaluateBool(tar, tmp, BoolThreads[tmp].Duration, accessor_cache);
                            }
                        };
                    });
                    on_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        return (float time, CacheDict cache) =>
                        {
                            EvaluateBool(tar, tmp, time, accessor_cache);
                        };
                    });
                }
            }
            #endregion
        }
        public override void OnReset()
        {
            base.OnReset();
            for (int i = 0; i < BoolThreads.Length; i++)
            {
                BoolThreads[i].Reset();
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            RMode = ThreadUtil.ReflectionMode.Field;
            MemberRModes = new ThreadUtil.ReflectionMode[0];
            BoolNames = new string[0];
            BoolThreads = new Triggering<bool>[0];
        }
        public override void OnToggleAll(bool value)
        {
            base.OnToggleAll(value);
            for (int i = 0; i < BoolThreads.Length; i++)
            {
                BoolThreads[i].Enable = value;
            }
        }

        #region GetAccessors
        private AccessorDict<bool> GetAccessors(Component tar)
        {
            AccessorDict<bool> accessor_cache = new AccessorDict<bool>();
            for (int i = 0; i < BoolNames.Length; i++)
            {
                if (MemberRModes[i] == ThreadUtil.ReflectionMode.Property)
                {
                    var info = tar.GetType().GetProperty(BoolNames[i]);
                    if (info != null)
                    {
                        var m_get = info.GetGetMethod();
                        if (m_get != null)
                        {
                            var get_func = DelegateUtil.MethodConverter.CreateFunc(m_get, tar.GetType(), typeof(bool));
                            accessor_cache.Getters.Add(BoolNames[i], () => { return (bool)get_func(tar); });
                        }
                        var m_set = info.GetSetMethod();
                        if (m_set != null)
                        {
                            var set_func = DelegateUtil.MethodConverter.CreateAction(m_set, tar.GetType(), typeof(bool));
                            accessor_cache.Setters.Add(BoolNames[i], (bool v) => { set_func(tar, v); });
                        }
                    }
                }
            }
            return accessor_cache;
        }
        #endregion

        #region Evaluate
        private void EvaluateBool(Component instance, int index, float time, AccessorDict<bool> accessor_cache)
        {
            bool value;
            if (BoolThreads[index].Behavior.TryEvaluate(time, BoolThreads[index].Duration, out value))
            {
                #region Reflection Set
                switch (MemberRModes[index])
                {
                    case ThreadUtil.ReflectionMode.Field:
                        instance.GetType().GetField(BoolNames[index]).SetValue(instance, value);
                        break;
                    case ThreadUtil.ReflectionMode.Property:
                        Action<bool> func = null;
                        if (accessor_cache.Setters.TryGetValue(BoolNames[index], out func))
                        {
                            func(value);
                        }
                        else
                        {
                            instance.GetType().GetProperty(BoolNames[index]).SetValue(instance, value);
                        }
                        break;
                }
                #endregion
            }
        }
        #endregion

        #region GUI
        public override void OnGUI(Action remove)
        {
#if UNITY_EDITOR
            DrawEditorHeaderGUI("Bool Effect", remove);
            DrawEditorBodyGUI();
            DrawEditorExtensionGUI();
            DrawEditorReflectionGUI();
            DrawEditorBottomGUI();
            base.OnGUI(remove);
#endif
        }
        private void DrawEditorReflectionGUI()
        {
#if UNITY_EDITOR
            if (IsFoldoutDisplayed)
            {
                EditorGUILayout.Space(10);
                Rect rect = GUILayoutUtility.GetRect(Screen.width, 18);
                float width = TSEffectGUILayout.GetInputFieldWidth();
                width = width < 232f ? 232f : width;
                width /= 2f;
                Rect rect1 = new Rect((rect.x + rect.width) - (width * 2) - 4, rect.y, width + 4, rect.height);
                Rect rect2 = new Rect((rect.x + rect.width) - width, rect.y, width, rect.height);
                GUI.Label(rect, "Reflection Mode");
                if (rect.width < 330f)
                {
                    rect1.y += 20f;
                    rect2.y += 20f;
                    EditorGUILayout.Space(20);
                }
                RMode = (ThreadUtil.ReflectionMode)EditorGUI.EnumPopup(rect1, RMode);
                if (GUI.Button(rect2, "Refresh " + RMode.ToString()))
                {
                    Type type = null;
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        type = assembly.GetType(TargetType);
                        if (type != null) break;
                    }

                    if (type != null)
                    {
                        ThreadUtil.ReflectionMode[] member_r_modes;
                        string[] member_names;
                        Triggering<bool>[] threads;
                        ThreadUtil.TryBuildThreadsFromComponent(type, RMode, out member_r_modes, out member_names, out threads);

                        bool refresh = false;
                        if (BoolNames.Length != member_names.Length)
                        {
                            refresh = true;
                        }
                        else
                        {
                            for (int i = 0; i < BoolNames.Length; i++)
                            {
                                if (BoolNames[i] != member_names[i])
                                {
                                    refresh = true;
                                    break;
                                }
                            }
                        }
                        if (refresh)
                        {
                            MemberRModes = member_r_modes;
                            BoolNames = member_names;
                            BoolThreads = threads;
                        }
                    }
                    else
                    {
                        MemberRModes = new ThreadUtil.ReflectionMode[0];
                        BoolNames = new string[0];
                        BoolThreads = new Triggering<bool>[0];
                    }
                }
            }
#endif
        }
        private void DrawEditorExtensionGUI()
        {
#if UNITY_EDITOR
            if (IsFoldoutDisplayed)
            {
                MonoBehaviour mono = null;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Type Getter");
                mono = EditorGUILayout.ObjectField("", mono, typeof(MonoBehaviour), true, GUILayout.MaxWidth(TSEffectGUILayout.GetInputFieldWidth())) as MonoBehaviour;
                EditorGUILayout.EndHorizontal();
                if (mono != null)
                {
                    TargetType = mono.GetType().FullName;
                }
                for (int i = 0; i < BoolThreads.Length; i++)
                {
                    TSEffectGUILayout.FullTriggeringThreadField(BoolThreads[i], true, BoolNames[i]);
                }
            }
#endif
        }
        #endregion
    }
}

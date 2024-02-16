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
    [Serializable, TemplateRegister(true, "Any/Float", "#CA69Cf", typeof(MonoBehaviour))]
    public sealed class FloatEffect : TSEffectTemplate
    {
        public ThreadUtil.ReflectionMode RMode;
        public ThreadUtil.ReflectionMode[] MemberRModes;
        public string[] FloatNames;

        [ThreadRegister("FloatThread")]
        public Varying<float>[] FloatThreads;

        public override void GenLogicBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            base.GenLogicBuilders(thread_field_name, out init_exe, out final_exe, out on_exe);

            #region Float
            if (thread_field_name == "FloatThreads")
            {
                for (int i = 0; i < FloatThreads.Length; i++)
                {
                    int tmp = i;

                    init_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        if (FloatThreads[tmp].Enable)
                        {
                            return (CacheDict cache) =>
                            {
                                #region Reflection Get
                                switch (MemberRModes[tmp])
                                {
                                    case ThreadUtil.ReflectionMode.Field:
                                        {
                                            CacheObj obj = new CacheObj();
                                            obj.Value_Float = (float)tar.GetType().GetField(FloatNames[tmp]).GetValue(tar);
                                            cache.Overwrite("initial", obj);
                                            break;
                                        }
                                    case ThreadUtil.ReflectionMode.Property:
                                        {
                                            Func<float> func = null;
                                            if (accessor_cache.Getters.TryGetValue(FloatNames[tmp], out func))
                                            {
                                                CacheObj obj = new CacheObj();
                                                obj.Value_Float = func();
                                                cache.Overwrite("initial", obj);
                                            }
                                            else
                                            {
                                                CacheObj obj = new CacheObj();
                                                obj.Value_Float = (float)tar.GetType().GetProperty(FloatNames[tmp]).GetValue(tar);
                                                cache.Overwrite("initial", obj);
                                            }
                                            break;
                                        }
                                }
                                #endregion
                            };
                        }
                        else
                            return null;
                    });
                    final_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        if (FloatThreads[tmp].Enable)
                        {
                            return (CacheDict cache) =>
                            {
                                if (FloatThreads[tmp].Resume)
                                {
                                    #region Reflection Set
                                    switch (MemberRModes[tmp])
                                    {
                                        case ThreadUtil.ReflectionMode.Field:
                                            tar.GetType().GetField(FloatNames[tmp]).SetValue(tar, cache.GetValue("initial").Value_Float);
                                            break;
                                        case ThreadUtil.ReflectionMode.Property:
                                            Action<float> func = null;
                                            if (accessor_cache.Setters.TryGetValue(FloatNames[tmp], out func))
                                            {
                                                func(cache.GetValue("initial").Value_Float);
                                            }
                                            else
                                            {
                                                tar.GetType().GetProperty(FloatNames[tmp]).SetValue(tar, cache.GetValue("initial").Value_Float);
                                            }
                                            break;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    EvaluateFloat(tar, tmp, 1f, accessor_cache, cache);
                                }
                            };
                        }
                        else
                            return null;
                    });
                    on_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        if (FloatThreads[tmp].Enable)
                        {
                            return (float time, CacheDict cache) =>
                            {
                                EvaluateFloat(tar, tmp, time, accessor_cache, cache);
                            };
                        }
                        else
                            return null;
                    });
                }
            }
            #endregion
        }
        public override void OnReset()
        {
            base.OnReset();
            for (int i = 0; i < FloatThreads.Length; i++)
            {
                FloatThreads[i].Reset();
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            RMode = ThreadUtil.ReflectionMode.Field;
            MemberRModes = new ThreadUtil.ReflectionMode[0];
            FloatNames = new string[0];
            FloatThreads = new Varying<float>[0];
        }
        public override void OnToggleAll(bool value)
        {
            base.OnToggleAll(value);
            for (int i = 0; i < FloatThreads.Length; i++)
            {
                FloatThreads[i].Enable = value;
            }
        }

        #region GetAccessors
        private AccessorDict<float> GetAccessors(Component instance)
        {
            AccessorDict<float> accessor_cache = new AccessorDict<float>();
            for (int i = 0; i < FloatNames.Length; i++)
            {
                if (MemberRModes[i] == ThreadUtil.ReflectionMode.Property)
                {
                    var info = instance.GetType().GetProperty(FloatNames[i]);
                    if (info != null)
                    {
                        var m_get = info.GetGetMethod();
                        if (m_get != null)
                        {
                            var get_func = DelegateUtil.MethodConverter.CreateFunc(m_get, instance.GetType(), typeof(float));
                            accessor_cache.Getters.Add(FloatNames[i], () => { return (float)get_func(instance); });
                        }
                        var m_set = info.GetSetMethod();
                        if (m_set != null)
                        {
                            var set_func = DelegateUtil.MethodConverter.CreateAction(m_set, instance.GetType(), typeof(float));
                            accessor_cache.Setters.Add(FloatNames[i], (float v) => { set_func(instance, v); });
                        }
                    }
                }
            }
            return accessor_cache;
        }
        #endregion

        #region Evaluate
        private void EvaluateFloat(Component instance, int index, float time, AccessorDict<float> accessor_cache, CacheDict cache)
        {
            switch (FloatThreads[index].Mode)
            {
                case DataVaryingMode.Increment:
                    #region Reflection Set
                    {
                        float res = cache.GetValue("initial").Value_Float + FloatThreads[index].Target * FloatThreads[index].Behavior.Evaluate(time);
                        switch (MemberRModes[index])
                        {
                            case ThreadUtil.ReflectionMode.Field:
                                instance.GetType().GetField(FloatNames[index]).SetValue(instance, res);
                                break;
                            case ThreadUtil.ReflectionMode.Property:
                                Action<float> func = null;
                                if (accessor_cache.Setters.TryGetValue(FloatNames[index], out func))
                                {
                                    func(res);
                                }
                                else
                                {
                                    instance.GetType().GetProperty(FloatNames[index]).SetValue(instance, res);
                                }
                                break;
                        }
                    }
                    #endregion
                    break;
                case DataVaryingMode.Target:
                    #region Reflection Set
                    {
                        float res = Mathf.Lerp(cache.GetValue("initial").Value_Float, FloatThreads[index].Target, FloatThreads[index].Behavior.Evaluate(time));
                        switch (MemberRModes[index])
                        {
                            case ThreadUtil.ReflectionMode.Field:
                                instance.GetType().GetField(FloatNames[index]).SetValue(instance, res);
                                break;
                            case ThreadUtil.ReflectionMode.Property:
                                Action<float> func = null;
                                if (accessor_cache.Setters.TryGetValue(FloatNames[index], out func))
                                {
                                    func(res);
                                }
                                else
                                {
                                    instance.GetType().GetProperty(FloatNames[index]).SetValue(instance, res);
                                }
                                break;
                        }
                    }
                    #endregion
                    break;
            }
        }
        #endregion

        #region GUI
        public override void OnGUI(Action remove)
        {
#if UNITY_EDITOR
            DrawEditorHeaderGUI("Float Effect", remove);
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
                float width = 151.5f;
                if (rect.width < 400) width = width - (400 - rect.width) / 2f;
                Rect rect1 = new Rect((rect.x + rect.width) - (width * 2) + 2, rect.y, width, rect.height);
                Rect rect2 = new Rect((rect.x + rect.width) - width + 2, rect.y, width, rect.height);
                GUI.Label(rect, "Reflection Mode");
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
                        Varying<float>[] threads;
                        ThreadUtil.TryBuildThreadsFromComponent(type, RMode, out member_r_modes, out member_names, out threads);
                        
                        bool refresh = false;
                        if (FloatNames.Length != member_names.Length)
                        {
                            refresh = true;
                        }
                        else
                        {
                            for (int i = 0; i < FloatNames.Length; i++)
                            {
                                if (FloatNames[i] != member_names[i])
                                {
                                    refresh = true;
                                    break;
                                }
                            }
                        }
                        if (refresh)
                        {
                            MemberRModes = member_r_modes;
                            FloatNames = member_names;
                            FloatThreads = threads;
                        }
                    }
                    else
                    {
                        MemberRModes = new ThreadUtil.ReflectionMode[0];
                        FloatNames = new string[0];
                        FloatThreads = new Varying<float>[0];
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
                mono = EditorGUILayout.ObjectField("", mono, typeof(MonoBehaviour), true, GUILayout.MaxWidth(300)) as MonoBehaviour;
                EditorGUILayout.EndHorizontal();
                if (mono != null)
                {
                    TargetType = mono.GetType().FullName;
                }
                for (int i = 0; i < FloatThreads.Length; i++)
                {
                    TSEffectGUILayout.FullVaryingThreadField(FloatThreads[i], true, FloatNames[i], () =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(FloatNames[i]);
                        FloatThreads[i].Target = EditorGUILayout.FloatField("", FloatThreads[i].Target, GUILayout.MaxWidth(217));
                        FloatThreads[i].Mode = (DataVaryingMode)EditorGUILayout.EnumPopup(FloatThreads[i].Mode, GUILayout.MaxWidth(80));
                        EditorGUILayout.EndHorizontal();
                    });
                }
            }
#endif
        }
        #endregion
    }
}

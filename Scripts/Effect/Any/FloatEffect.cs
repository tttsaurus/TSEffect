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
        public ThreadUtil.ReflectionMode[] MemberRMode;
        public string[] FloatName;

        [ThreadRegister("FloatThread")]
        public Varying<float>[] FloatThread;

        public override void GenLogicBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            base.GenLogicBuilders(thread_field_name, out init_exe, out final_exe, out on_exe);

            #region Float
            if (thread_field_name == "FloatThread")
            {
                for (int i = 0; i < FloatThread.Length; i++)
                {
                    int tmp = i;

                    init_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        if (FloatThread[tmp].Enable)
                        {
                            return (CacheDict cache) =>
                            {
                                #region Reflection Get
                                switch (MemberRMode[tmp])
                                {
                                    case ThreadUtil.ReflectionMode.Field:
                                        {
                                            CacheObj obj = new CacheObj();
                                            obj.Value_Float = (float)tar.GetType().GetField(FloatName[tmp]).GetValue(tar);
                                            cache.Overwrite("initial", obj);
                                            break;
                                        }
                                    case ThreadUtil.ReflectionMode.Property:
                                        {
                                            Func<float> func = null;
                                            if (accessor_cache.Getters.TryGetValue(FloatName[tmp], out func))
                                            {
                                                CacheObj obj = new CacheObj();
                                                obj.Value_Float = func();
                                                cache.Overwrite("initial", obj);
                                            }
                                            else
                                            {
                                                CacheObj obj = new CacheObj();
                                                obj.Value_Float = (float)tar.GetType().GetProperty(FloatName[tmp]).GetValue(tar);
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
                        if (FloatThread[tmp].Enable)
                        {
                            return (CacheDict cache) =>
                            {
                                if (FloatThread[tmp].Resume)
                                {
                                    #region Reflection Set
                                    switch (MemberRMode[tmp])
                                    {
                                        case ThreadUtil.ReflectionMode.Field:
                                            tar.GetType().GetField(FloatName[tmp]).SetValue(tar, cache.GetValue("initial").Value_Float);
                                            break;
                                        case ThreadUtil.ReflectionMode.Property:
                                            Action<float> func = null;
                                            if (accessor_cache.Setters.TryGetValue(FloatName[tmp], out func))
                                            {
                                                func(cache.GetValue("initial").Value_Float);
                                            }
                                            else
                                            {
                                                tar.GetType().GetProperty(FloatName[tmp]).SetValue(tar, cache.GetValue("initial").Value_Float);
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
                        if (FloatThread[tmp].Enable)
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
            for (int i = 0; i < FloatThread.Length; i++)
            {
                FloatThread[i].Reset();
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            RMode = ThreadUtil.ReflectionMode.Field;
            MemberRMode = new ThreadUtil.ReflectionMode[0];
            FloatName = new string[0];
            FloatThread = new Varying<float>[0];
        }
        public override void OnToggleAll(bool value)
        {
            base.OnToggleAll(value);
            for (int i = 0; i < FloatThread.Length; i++)
            {
                FloatThread[i].Enable = value;
            }
        }

        #region GetAccessors
        private AccessorDict<float> GetAccessors(Component instance)
        {
            AccessorDict<float> accessor_cache = new AccessorDict<float>();
            for (int i = 0; i < FloatName.Length; i++)
            {
                if (MemberRMode[i] == ThreadUtil.ReflectionMode.Property)
                {
                    var info = instance.GetType().GetProperty(FloatName[i]);
                    if (info != null)
                    {
                        var m_get = info.GetGetMethod();
                        if (m_get != null)
                        {
                            var get_func = DelegateUtil.MethodConverter.CreateFunc(m_get, instance.GetType(), typeof(float));
                            accessor_cache.Getters.Add(FloatName[i], () => { return (float)get_func(instance); });
                        }
                        var m_set = info.GetSetMethod();
                        if (m_set != null)
                        {
                            var set_func = DelegateUtil.MethodConverter.CreateAction(m_set, instance.GetType(), typeof(float));
                            accessor_cache.Setters.Add(FloatName[i], (float v) => { set_func(instance, v); });
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
            switch (FloatThread[index].Mode)
            {
                case DataVaryingMode.Increment:
                    #region Reflection Set
                    {
                        float res = cache.GetValue("initial").Value_Float + FloatThread[index].Target * FloatThread[index].Behavior.Evaluate(time);
                        switch (MemberRMode[index])
                        {
                            case ThreadUtil.ReflectionMode.Field:
                                instance.GetType().GetField(FloatName[index]).SetValue(instance, res);
                                break;
                            case ThreadUtil.ReflectionMode.Property:
                                Action<float> func = null;
                                if (accessor_cache.Setters.TryGetValue(FloatName[index], out func))
                                {
                                    func(res);
                                }
                                else
                                {
                                    instance.GetType().GetProperty(FloatName[index]).SetValue(instance, res);
                                }
                                break;
                        }
                    }
                    #endregion
                    break;
                case DataVaryingMode.Target:
                    #region Reflection Set
                    {
                        float res = Mathf.Lerp(cache.GetValue("initial").Value_Float, FloatThread[index].Target, FloatThread[index].Behavior.Evaluate(time));
                        switch (MemberRMode[index])
                        {
                            case ThreadUtil.ReflectionMode.Field:
                                instance.GetType().GetField(FloatName[index]).SetValue(instance, res);
                                break;
                            case ThreadUtil.ReflectionMode.Property:
                                Action<float> func = null;
                                if (accessor_cache.Setters.TryGetValue(FloatName[index], out func))
                                {
                                    func(res);
                                }
                                else
                                {
                                    instance.GetType().GetProperty(FloatName[index]).SetValue(instance, res);
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
                    Type type = Type.GetType(TargetType);
                    if (type != null)
                    {
                        ThreadUtil.ReflectionMode[] member_r_mode = new ThreadUtil.ReflectionMode[0];
                        string[] member_name = new string[0];
                        Varying<float>[] thread = new Varying<float>[0];
                        ThreadUtil.TryBuildThreadsFromComponent(type, RMode, out member_r_mode, out member_name, out thread);
                        
                        bool refresh = false;
                        if (FloatName.Length != member_name.Length)
                        {
                            refresh = true;
                        }
                        else
                        {
                            for (int i = 0; i < FloatName.Length; i++)
                            {
                                if (FloatName[i] != member_name[i])
                                {
                                    refresh = true;
                                    break;
                                }
                            }
                        }
                        if (refresh)
                        {
                            MemberRMode = member_r_mode;
                            FloatName = member_name;
                            FloatThread = thread;
                        }
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
                    _TargetType = mono.GetType().FullName;
                }
                for (int i = 0; i < FloatThread.Length; i++)
                {
                    TSEffectGUILayout.FullVaryingThreadField(FloatThread[i], true, FloatName[i], () =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(FloatName[i]);
                        FloatThread[i].Target = EditorGUILayout.FloatField("", FloatThread[i].Target, GUILayout.MaxWidth(217));
                        FloatThread[i].Mode = (DataVaryingMode)EditorGUILayout.EnumPopup(FloatThread[i].Mode, GUILayout.MaxWidth(80));
                        EditorGUILayout.EndHorizontal();
                    });
                }
            }
#endif
        }
        #endregion
    }
}

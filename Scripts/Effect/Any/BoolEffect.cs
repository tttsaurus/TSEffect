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
    [Serializable, TemplateRegister(true, "Any/Bool", "#CA69Cf", typeof(MonoBehaviour))]
    public sealed class BoolEffect : TSEffectTemplate
    {
        public ThreadUtil.ReflectionMode RMode;
        public ThreadUtil.ReflectionMode[] MemberRMode;
        public string[] BoolName;

        [ThreadRegister("BoolThread")]
        public Triggering<bool>[] BoolThread;

        public override void GenLogicBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            base.GenLogicBuilders(thread_field_name, out init_exe, out final_exe, out on_exe);
            
            #region Bool
            if (thread_field_name == "BoolThread")
            {
                for (int i = 0; i < BoolThread.Length; i++)
                {
                    int tmp = i;
                    init_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        if (BoolThread[tmp].Enable)
                        {
                            return (CacheDic cache) =>
                            {
                                BoolThread[tmp].Behavior.BindCache(cache);
                                #region Reflection Get
                                switch (MemberRMode[tmp])
                                {
                                    case ThreadUtil.ReflectionMode.Field:
                                        {
                                            CacheObj obj = new CacheObj();
                                            obj.Value_Bool = (bool)tar.GetType().GetField(BoolName[tmp]).GetValue(tar);
                                            cache.Overwrite("initial", obj);
                                        }
                                        break;
                                    case ThreadUtil.ReflectionMode.Property:
                                        {
                                            Func<bool> func = null;
                                            if (accessor_cache.Getters.TryGetValue(BoolName[tmp], out func))
                                            {
                                                CacheObj obj = new CacheObj();
                                                obj.Value_Bool = func();
                                                cache.Overwrite("initial", obj);
                                            }
                                            else
                                            {
                                                CacheObj obj = new CacheObj();
                                                obj.Value_Bool = (bool)tar.GetType().GetProperty(BoolName[tmp]).GetValue(tar);
                                                cache.Overwrite("initial", obj);
                                            }
                                        }
                                        break;
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
                        if (BoolThread[tmp].Enable)
                        {
                            return (CacheDic cache) =>
                            {
                                if (BoolThread[tmp].Resume)
                                {
                                    #region Reflection Set
                                    switch (MemberRMode[tmp])
                                    {
                                        case ThreadUtil.ReflectionMode.Field:
                                            tar.GetType().GetField(BoolName[tmp]).SetValue(tar, cache.GetValue("initial").Value_Bool);
                                            break;
                                        case ThreadUtil.ReflectionMode.Property:
                                            Action<bool> func = null;
                                            if (accessor_cache.Setters.TryGetValue(BoolName[tmp], out func))
                                            {
                                                func(cache.GetValue("initial").Value_Bool);
                                            }
                                            else
                                            {
                                                tar.GetType().GetProperty(BoolName[tmp]).SetValue(tar, cache.GetValue("initial").Value_Bool);
                                            }
                                            break;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    EvaluateBool(tar, tmp, BoolThread[tmp].Duration, accessor_cache);
                                }
                            };
                        }
                        else
                            return null;
                    });
                    on_exe.Add((Component tar) =>
                    {
                        var accessor_cache = GetAccessors(tar);
                        if (BoolThread[tmp].Enable)
                        {
                            return (float time, CacheDic cache) =>
                            {
                                EvaluateBool(tar, tmp, time, accessor_cache);
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
            for (int i = 0; i < BoolThread.Length; i++)
            {
                BoolThread[i].Reset();
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            RMode = ThreadUtil.ReflectionMode.Field;
            MemberRMode = new ThreadUtil.ReflectionMode[0];
            BoolName = new string[0];
            BoolThread = new Triggering<bool>[0];
        }
        public override void OnToggleAll(bool value)
        {
            base.OnToggleAll(value);
            for (int i = 0; i < BoolThread.Length; i++)
            {
                BoolThread[i].Enable = value;
            }
        }

        #region GetAccessors
        private AccessorDict<bool> GetAccessors(Component tar)
        {
            AccessorDict<bool> accessor_cache = new AccessorDict<bool>();
            for (int i = 0; i < BoolName.Length; i++)
            {
                if (MemberRMode[i] == ThreadUtil.ReflectionMode.Property)
                {
                    var info = tar.GetType().GetProperty(BoolName[i]);
                    if (info != null)
                    {
                        var m_get = info.GetGetMethod();
                        if (m_get != null)
                        {
                            var get_func = DelegateUtil.MethodConverter.CreateFunc(m_get, tar.GetType(), typeof(bool));
                            accessor_cache.Getters.Add(BoolName[i], () => { return (bool)get_func(tar); });
                        }
                        var m_set = info.GetSetMethod();
                        if (m_set != null)
                        {
                            var set_func = DelegateUtil.MethodConverter.CreateAction(m_set, tar.GetType(), typeof(bool));
                            accessor_cache.Setters.Add(BoolName[i], (bool v) => { set_func(tar, v); });
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
            if (BoolThread[index].Behavior.TryEvaluate(time, BoolThread[index].Duration, out value))
            {
                #region Reflection Set
                switch (MemberRMode[index])
                {
                    case ThreadUtil.ReflectionMode.Field:
                        instance.GetType().GetField(BoolName[index]).SetValue(instance, value);
                        break;
                    case ThreadUtil.ReflectionMode.Property:
                        Action<bool> func = null;
                        if (accessor_cache.Setters.TryGetValue(BoolName[index], out func))
                        {
                            func(value);
                        }
                        else
                        {
                            instance.GetType().GetProperty(BoolName[index]).SetValue(instance, value);
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
                        Triggering<bool>[] thread = new Triggering<bool>[0];
                        ThreadUtil.TryBuildThreadsFromComponent(type, RMode, out member_r_mode, out member_name, out thread);
                        
                        bool refresh = false;
                        if (BoolName.Length != member_name.Length)
                        {
                            refresh = true;
                        }
                        else
                        {
                            for (int i = 0; i < BoolName.Length; i++)
                            {
                                if (BoolName[i] != member_name[i])
                                {
                                    refresh = true;
                                    break;
                                }
                            }
                        }
                        if (refresh)
                        {
                            MemberRMode = member_r_mode;
                            BoolName = member_name;
                            BoolThread = thread;
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
                for (int i = 0; i < BoolThread.Length; i++)
                {
                    TSEffectGUILayout.FullTriggeringThreadField(BoolThread[i], true, BoolName[i]);
                }
            }
#endif
        }
        #endregion
    }
}

using System;
using UnityEngine;
using TS.TSEffect.Thread;
using TS.TSEffect.Thread.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TS.TSEffect.Editor
{
    public class TSEffectGUILayout
    {
        #region ThreadHeaderField
        public static void ThreadHeaderField(string title, BaseThread value)
        {
#if UNITY_EDITOR
            EditorGUILayout.Space(1);
            var area = GUILayoutUtility.GetRect(Screen.width, 20f);
            EditorGUI.LabelField(new Rect(area.x + 33, area.y, area.width - 33, area.height), title);
            value.Foldout = EditorGUI.Foldout(new Rect(area.x + 32, area.y, area.width - 32, area.height), value.Foldout, "");
            value.Enable = EditorGUI.Toggle(new Rect(area.x, area.y, 10, area.height), value.Enable);
#endif
        }
        public static void ThreadHeaderField<T>(string title, UniversalThread<T> value)
        {
#if UNITY_EDITOR
            EditorGUILayout.Space(1);
            var area = GUILayoutUtility.GetRect(Screen.width, 20f);
            EditorGUI.LabelField(new Rect(area.x + 33, area.y, area.width - 33, area.height), title);
            value.Foldout = EditorGUI.Foldout(new Rect(area.x + 32, area.y, area.width - 32, area.height), value.Foldout, "");
            value.Enable = EditorGUI.Toggle(new Rect(area.x, area.y, 10, area.height), value.Enable);
#endif
        }
        #endregion

        #region BaseThreadMainField
        public static void BaseThreadMainField(BaseThread value)
        {
#if UNITY_EDITOR
            EditorGUI.BeginDisabledGroup(!value.Enable);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Priority");
            value.Priority = EditorGUILayout.IntField(value.Priority, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Auto Suspend");
            value.AutoSuspend = EditorGUILayout.Toggle(value.AutoSuspend, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Initial Delay");
            value.InitialDelay = EditorGUILayout.FloatField(value.InitialDelay, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Duration");
            value.Duration = EditorGUILayout.FloatField(value.Duration, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Loop");
            value.Loop = EditorGUILayout.IntField(value.Loop, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Delay Between Loops");
            value.DelayBetweenLoops = EditorGUILayout.FloatField(value.DelayBetweenLoops, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Resume");
            value.Resume = EditorGUILayout.Toggle(value.Resume, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
#endif
        }
        #endregion

        #region FullVaryingThreadField
        public static void FullVaryingThreadField<T>(Varying<T> value, bool header, string title, Action gui)
        {
#if UNITY_EDITOR
            if (header)
            {
                ThreadHeaderField(title, value);
            }
            if (value.Foldout)
            {
                EditorGUI.BeginDisabledGroup(value.Enable == false);

                BaseThreadMainField(value);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Behavior");
                value.Behavior = EditorGUILayout.CurveField(value.Behavior, GUILayout.MaxWidth(300));
                EditorGUILayout.EndHorizontal();
                gui();
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.Space(1);
#endif
        }
        #endregion

        #region FullTriggeringThreadField
        public static void FullTriggeringThreadField<T>(Triggering<T> value, bool header, string title, Func<T, T> value_adjust)
        {
#if UNITY_EDITOR
            if (header)
            {
                ThreadHeaderField(title, value);
            }
            if (value.Foldout)
            {
                EditorGUI.BeginDisabledGroup(value.Enable == false);

                BaseThreadMainField(value);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Behavior");
                if (GUILayout.Button("Edit", GUILayout.MaxWidth(301)))
                {
                    value.Behavior.Foldout = !value.Behavior.Foldout;
                }
                EditorGUILayout.EndHorizontal();
                if (value.Behavior.Foldout)
                {
                    TSEffectGUI.TriggerTableField(GUILayoutUtility.GetRect(Screen.width, 20), value.Behavior, value.Duration, value_adjust);
                }

                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.Space(1);
#endif
        }
        public static void FullTriggeringThreadField<T>(Triggering<T> value, bool header, string title)
        {
#if UNITY_EDITOR
            if (header)
            {
                ThreadHeaderField(title, value);
            }
            if (value.Foldout)
            {
                EditorGUI.BeginDisabledGroup(value.Enable == false);

                BaseThreadMainField(value);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Behavior");
                if (GUILayout.Button("Edit", GUILayout.MaxWidth(301)))
                {
                    value.Behavior.Foldout = !value.Behavior.Foldout;
                }
                EditorGUILayout.EndHorizontal();
                if (value.Behavior.Foldout)
                {
                    TSEffectGUI.TriggerTableField(GUILayoutUtility.GetRect(Screen.width, 20), value.Behavior, value.Duration, (v) => { return v; });
                }

                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.Space(1);
#endif
        }
        #endregion
    }
}

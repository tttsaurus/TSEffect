using UnityEngine;
using TS.TSEffect.Thread.Universal;
using System;
using System.Runtime.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TS.TSEffect.Editor
{
    public static class TSEffectGUI
    {
        #region ParameterField
        public static T ParameterField<T>(Rect rect, T value)
        {
#if UNITY_EDITOR
            Type type = typeof(T);
            if (type == typeof(int))
            {
                object v = value;
                v = EditorGUI.IntField(rect, (int)v);
                value =  (T)v;
            }
            else if (type == typeof(float))
            {
                object v = value;
                v = EditorGUI.FloatField(rect, (float)v);
                value = (T)v;
            }
            else if (type == typeof(bool))
            {
                object v = value;
                v = EditorGUI.Toggle(rect, (bool)v);
                value = (T)v;
            }
            else if (type == typeof(char))
            {
                object v = value;
                string str = ((char)v).ToString();
                str = EditorGUI.TextField(rect, str);
                if (str.Length > 0) v = str[0];
                value = (T)v;
            }
            else if (type == typeof(string))
            {
                object v = value;
                v = EditorGUI.TextField(rect, (string)v);
                value = (T)v;
            }
            else if (type == typeof(Color))
            {
                object v = value;
                v = EditorGUI.ColorField(rect, (Color)v);
                value = (T)v;
            }
            else
            {
                try
                {
                    object v = value;
                    v = EditorGUI.ObjectField(rect, (UnityEngine.Object)v, typeof(T), true);
                    value = (T)v;
                }
                catch (Exception e)
                {
                    EditorGUI.HelpBox(rect, e.Message, MessageType.Error);
                }
            }
#endif
            return value;
        }
        #endregion

        #region TriggerTableField
        public static void TriggerTableField<T>(Rect rect, TriggerTable<T> value, float duration, Func<T, T> value_adjust)
        {
#if UNITY_EDITOR
            rect = new Rect(rect.x, rect.y, rect.width, 20 + value.Count * 24);
            if (value.Count > 0) rect.height += 4;
            EditorGUI.HelpBox(rect, "", MessageType.None);
            EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, 20), "", MessageType.None);
            EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width / 2f, 20), "", MessageType.None);
            EditorGUI.HelpBox(new Rect(rect.x + rect.width / 2f - 1, rect.y, rect.width / 2f + 1, 20), "", MessageType.None);
            GUI.Label(new Rect(rect.x, rect.y, rect.width / 2f, 20), " Time");
            GUI.Label(new Rect(rect.x + rect.width / 2f - 1, rect.y, rect.width / 2f + 1, 20), " Value");
            for (int i = 0; i < value.Count; i++)
            {
                Rect p1 = new Rect(rect.x + 6, rect.y + 20 + i * 24 + 4, rect.width / 2f - 9, 20);
                Rect p2 = new Rect(rect.x + rect.width / 2f + 3, rect.y + 20 + i * 24 + 4, rect.width / 2f - 26, 20);
                Rect p3 = new Rect(p2.x + p2.width, rect.y + 20 + i * 24 + 4, 20, 20);

                var v = value.GetRow(i);

                float time = EditorGUI.Slider(p1, v.Item1, 0f, duration);
                T param = ParameterField(p2, v.Item2);
                param = value_adjust(param);

                if (time != v.Item1)
                {
                    value.SetKeyTime(i, time);
                }
                if (!param.Equals(v.Item2))
                {
                    value.SetKeyValue(i, param);
                }

                EditorGUI.LabelField(p3, EditorGUIUtility.IconContent("TreeEditor.Trash"));
                if (p3.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        value.RemoveAt(i);
                        GUI.changed = true;
                    }
                }
            }
            rect.y += rect.height;
            int sele = -1;
            if (value.Sorted) sele = 2;
            int index = GUI.Toolbar(new Rect(rect.x + rect.width / 3f * 2f - 1, rect.y, rect.width / 3f + 1, 20), sele, new string[] { "+", "-", "Sort" });
            if (index == 0)
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
                {
                    value.AddRow(0f, (T)FormatterServices.GetUninitializedObject(typeof(T)));
                }
                else
                {
                    value.AddRow(0f, (T)Activator.CreateInstance(typeof(T)));
                }
            }
            if (index == 1)
            {
                if (value.Count > 0)
                {
                    value.RemoveAt(value.Count - 1);
                }
            }
            if (index == 2)
            {
                value.Sort();
            }
            EditorGUILayout.Space(rect.height);
#endif
        }
        #endregion
    }
}

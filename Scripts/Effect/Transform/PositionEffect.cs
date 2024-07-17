using UnityEngine;
using TS.TSEffect.Template;
using TS.TSEffect.Thread;
using TS.TSEffect.Thread.Universal;
using TS.TSEffect.Thread.Cache;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using TS.TSEffect.Editor;
#endif

namespace TS.TSEffect.Effect
{
    [Serializable, TemplateRegister(true, "Transform/Position", "#C1E168", typeof(Transform))]
    public sealed class PositionEffect : TSEffectTemplate
    {
        [ThreadRegister("Position")]
        public Varying<Vector3> Position;
        public bool IsWorldPos;
        
        public override void GetExeFuncBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            base.GetExeFuncBuilders(thread_field_name, out init_exe, out final_exe, out on_exe);

            #region Position
            if (thread_field_name == "Position")
            {
                init_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (CacheDict cache) =>
                    {
                        if (IsWorldPos)
                        {
                            CacheObj obj = new CacheObj();
                            obj.Value_UnityEngine_Vector3 = instance.position;
                            cache.Overwrite("initial", obj);
                        }
                        else
                        {
                            CacheObj obj = new CacheObj();
                            obj.Value_UnityEngine_Vector3 = instance.localPosition;
                            cache.Overwrite("initial", obj);
                        }
                    };
                });
                final_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (CacheDict cache) =>
                    {
                        if (Position.RecoverAfterAll)
                        {
                            if (IsWorldPos)
                            {
                                instance.position = cache.GetValue("initial").Value_UnityEngine_Vector3;
                            }
                            else
                            {
                                instance.localPosition = cache.GetValue("initial").Value_UnityEngine_Vector3;
                            }
                        }
                        else
                        {
                            EvaluatePosition(instance, 1f, cache);
                        }
                    };
                });
                on_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (float time, CacheDict cache) =>
                    {
                        EvaluatePosition(instance, time, cache);
                    };
                });
            }
            #endregion
        }
        public override void OnReset()
        {
            base.OnReset();
            Position.Reset();
        }
        protected override void OnInit() 
        {
            base.OnInit();
            Position = new Varying<Vector3>();
            IsWorldPos = true;
        }
        public override void OnToggleAll(bool value)
        {
            base.OnToggleAll(value);
            Position.Enable = value;
        }

        #region Evaluate
        private void EvaluatePosition(Transform instance, float time, CacheDict cache)
        {
            if (IsWorldPos)
            {
                switch (Position.Mode)
                {
                    case DataVaryingMode.Increment:
                        instance.position = cache.GetValue("initial").Value_UnityEngine_Vector3 + Position.Target * Position.Behavior.Evaluate(time);
                        break;
                    case DataVaryingMode.Target:
                        instance.position = Vector3.Lerp(cache.GetValue("initial").Value_UnityEngine_Vector3, Position.Target, Position.Behavior.Evaluate(time));
                        break;
                }
            }
            else
            {
                switch (Position.Mode)
                {
                    case DataVaryingMode.Increment:
                        instance.localPosition = cache.GetValue("initial").Value_UnityEngine_Vector3 + Position.Target * Position.Behavior.Evaluate(time);
                        break;
                    case DataVaryingMode.Target:
                        instance.localPosition = Vector3.Lerp(cache.GetValue("initial").Value_UnityEngine_Vector3, Position.Target, Position.Behavior.Evaluate(time));
                        break;
                }
            }
        }
        #endregion

        #region GUI
        public override void OnGUI(Action remove)
        {
#if UNITY_EDITOR
            DrawEditorHeaderGUI("Position Effect", remove);
            DrawEditorBodyGUI();
            DrawEditorExtensionGUI();
            DrawEditorBottomGUI();
            base.OnGUI(remove);
#endif
        }
        private void DrawEditorExtensionGUI()
        {
#if UNITY_EDITOR
            if (IsFoldoutDisplayed)
            {
                TSEffectGUILayout.FullVaryingThreadField(Position, true, "Position", () =>
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label((IsWorldPos ? "World " : "Local ") + "Position");
                    float width = TSEffectGUILayout.GetInputFieldWidth();
                    if (GUILayout.Button(IsWorldPos ? "W" : "L", GUILayout.MaxWidth(width * 25f / 295f))) IsWorldPos = !IsWorldPos;
                    Position.Target = EditorGUILayout.Vector3Field("", Position.Target, GUILayout.MaxWidth(width * 190f / 295f - 4f));
                    Position.Mode = (DataVaryingMode)EditorGUILayout.EnumPopup(Position.Mode, GUILayout.MaxWidth(width * 80f / 295f - 2f));
                    EditorGUILayout.EndHorizontal();
                });
            }
#endif
        }
        #endregion
    }
}

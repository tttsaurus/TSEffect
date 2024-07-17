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
    [Serializable, TemplateRegister(true, "Transform/Rotation", "#C1E168", typeof(Transform))]
    public sealed class RotationEffect : TSEffectTemplate
    {
        [ThreadRegister("Rotation")]
        public Varying<Vector3> Rotation;
        public bool IsWorldRot;

        public override void GetExeFuncBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            base.GetExeFuncBuilders(thread_field_name, out init_exe, out final_exe, out on_exe);

            #region Rotation
            if (thread_field_name == "Rotation")
            {
                init_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (CacheDict cache) =>
                    {
                        if (IsWorldRot)
                        {
                            CacheObj obj = new CacheObj();
                            obj.Value_UnityEngine_Quaternion = instance.rotation;
                            cache.Overwrite("initial", obj);
                        }
                        else
                        {
                            CacheObj obj = new CacheObj();
                            obj.Value_UnityEngine_Quaternion = instance.localRotation;
                            cache.Overwrite("initial", obj);
                        }
                    };
                });
                final_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (CacheDict cache) =>
                    {
                        if (Rotation.RecoverAfterAll)
                        {
                            if (IsWorldRot)
                            {
                                instance.rotation = cache.GetValue("initial").Value_UnityEngine_Quaternion;
                            }
                            else
                            {
                                instance.localRotation = cache.GetValue("initial").Value_UnityEngine_Quaternion;
                            }
                        }
                        else
                        {
                            EvaluateRotation(instance, 1f, cache);
                        }
                    };
                });
                on_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (float time, CacheDict cache) =>
                    {
                        EvaluateRotation(instance, time, cache);
                    };
                });
            }
            #endregion
        }
        public override void OnReset()
        {
            base.OnReset();
            Rotation.Reset();
        }
        protected override void OnInit()
        {
            base.OnInit();
            Rotation = new Varying<Vector3>();
            IsWorldRot = true;
        }
        public override void OnToggleAll(bool value)
        {
            base.OnToggleAll(value);
            Rotation.Enable = value;
        }

        #region Evaluate
        private void EvaluateRotation(Transform instance, float time, CacheDict cache)
        {
            if (IsWorldRot)
            {
                switch (Rotation.Mode)
                {
                    case DataVaryingMode.Increment:
                        instance.rotation = Quaternion.Euler(cache.GetValue("initial").Value_UnityEngine_Quaternion.eulerAngles + Rotation.Target * Rotation.Behavior.Evaluate(time));
                        break;
                    case DataVaryingMode.Target:
                        instance.rotation = Quaternion.Lerp(cache.GetValue("initial").Value_UnityEngine_Quaternion, Quaternion.Euler(Rotation.Target), Rotation.Behavior.Evaluate(time));
                        break;
                }
            }
            else
            {
                switch (Rotation.Mode)
                {
                    case DataVaryingMode.Increment:
                        instance.localRotation = Quaternion.Euler(cache.GetValue("initial").Value_UnityEngine_Quaternion.eulerAngles + Rotation.Target * Rotation.Behavior.Evaluate(time));
                        break;
                    case DataVaryingMode.Target:
                        instance.localRotation = Quaternion.Lerp(cache.GetValue("initial").Value_UnityEngine_Quaternion, Quaternion.Euler(Rotation.Target), Rotation.Behavior.Evaluate(time));
                        break;
                }
            }
        }
        #endregion

        #region GUI
        public override void OnGUI(Action remove)
        {
#if UNITY_EDITOR
            DrawEditorHeaderGUI("Rotation Effect", remove);
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
                TSEffectGUILayout.FullVaryingThreadField(Rotation, true, "Rotation", () =>
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label((IsWorldRot ? "World " : "Local ") + " Rotation");
                    float width = TSEffectGUILayout.GetInputFieldWidth();
                    if (GUILayout.Button(IsWorldRot ? "W" : "L", GUILayout.MaxWidth(width * 25f / 295f))) IsWorldRot = !IsWorldRot;
                    Rotation.Target = EditorGUILayout.Vector3Field("", Rotation.Target, GUILayout.MaxWidth(width * 190f / 295f - 4f));
                    Rotation.Mode = (DataVaryingMode)EditorGUILayout.EnumPopup(Rotation.Mode, GUILayout.MaxWidth(width * 80f / 295f - 2f));
                    EditorGUILayout.EndHorizontal();
                });
            }
#endif
        }
        #endregion
    }
}

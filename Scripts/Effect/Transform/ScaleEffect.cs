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
    [Serializable, TemplateRegister(true, "Transform/Scale", "#C1E168", typeof(Transform))]
    public sealed class ScaleEffect : TSEffectTemplate
    {
        [ThreadRegister("Scale")]
        public Varying<Vector3> Scale;

        public override void GetExeFuncBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            base.GetExeFuncBuilders(thread_field_name, out init_exe, out final_exe, out on_exe);

            #region Scale
            if (thread_field_name == "Scale")
            {
                init_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (CacheDict cache) =>
                    {
                        CacheObj obj = new CacheObj();
                        obj.Value_UnityEngine_Vector3 = instance.localScale;
                        cache.Overwrite("initial", obj);
                    };
                });
                final_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (CacheDict cache) =>
                    {
                        if (Scale.Resume)
                        {
                            instance.localScale = cache.GetValue("initial").Value_UnityEngine_Vector3;
                        }
                        else
                        {
                            EvaluateScale(instance, 1f, cache);
                        }
                    };
                });
                on_exe.Add((Component tar) =>
                {
                    var instance = tar as Transform;
                    return (float time, CacheDict cache) =>
                    {
                        EvaluateScale(instance, time, cache);
                    };
                });
            }
            #endregion
        }
        public override void OnReset()
        {
            base.OnReset();
            Scale.Reset();
        }
        protected override void OnInit()
        {
            base.OnInit();
            Scale = new Varying<Vector3>();
        }
        public override void OnToggleAll(bool value)
        {
            base.OnToggleAll(value);
            Scale.Enable = value;
        }

        #region Evaluate
        private void EvaluateScale(Transform instance, float time, CacheDict cache)
        {
            switch (Scale.Mode)
            {
                case DataVaryingMode.Increment:
                    instance.localScale = cache.GetValue("initial").Value_UnityEngine_Vector3 + Scale.Target * Scale.Behavior.Evaluate(time);
                    break;
                case DataVaryingMode.Target:
                    instance.localScale = Vector3.Lerp(cache.GetValue("initial").Value_UnityEngine_Vector3, Scale.Target, Scale.Behavior.Evaluate(time));
                    break;
            }
        }
        #endregion

        #region GUI
        public override void OnGUI(Action remove)
        {
#if UNITY_EDITOR
            DrawEditorHeaderGUI("Scale Effect", remove);
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
                TSEffectGUILayout.FullVaryingThreadField(Scale, true, "Scale", () =>
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Local Scale");
                    Scale.Target = EditorGUILayout.Vector3Field("", Scale.Target, GUILayout.MaxWidth(217));
                    Scale.Mode = (DataVaryingMode)EditorGUILayout.EnumPopup(Scale.Mode, GUILayout.MaxWidth(80));
                    EditorGUILayout.EndHorizontal();
                });
            }
#endif
        }
        #endregion
    }
}

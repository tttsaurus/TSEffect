using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TS.TSEffect
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TSEffectExecutor))]
    public class TSEffectExecutorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("Helpbox");
            EditorGUILayout.Space(3);
            foreach (var exe in TSEffect.RuntimeExecutors)
            {
                string status = "Continue";
                Color color = new Color(135f / 255f, 200f / 255f, 111f / 255f);
                if (exe.IsInitDelay || exe.IsLoopDelay)
                {
                    status = "Delay";
                    color = new Color(117f / 255f, 199f / 255f, 169f / 255f);
                }
                if (exe.IsFinished)
                {
                    status = "Finish";
                    color = new Color(199f / 255f, 161f / 255f, 129f / 255f);
                }
                if (exe.IsPaused)
                {
                    status += " (Pause)";
                    color = new Color(color.r, color.g, color.b, 0.7f);
                }
                if (exe.IsSuspended)
                {
                    status = "Suspended";
                    color = new Color(199f / 255f, 161f / 255f, 129f / 255f);
                }

                Rect rect = GUILayoutUtility.GetRect(Screen.width, 25);
                rect.x += 110;
                rect.width -= 120;
                EditorGUI.DrawRect(rect, color);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 2), new Color(0, 0, 0, 0.2f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2), new Color(0, 0, 0, 0.2f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2, 2, 21), new Color(0, 0, 0, 0.2f));
                EditorGUI.DrawRect(new Rect(rect.x + rect.width - 2, rect.y + 2, 2, 21), new Color(0, 0, 0, 0.2f));

                GUIStyle style = new GUIStyle();
                style.normal.textColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
                EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 5, rect.width, 15), exe.ExeThreadCore.Name, style);
                EditorGUI.LabelField(new Rect(rect.x - 105, rect.y + 5, 105, 15), status);

                EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * exe.RelativePerc, rect.height - 4), new Color(0, 0, 0, 0.1f));
                EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y + rect.height - 7, (rect.width - 4) * exe.OverallPerc, 5), new Color(1, 1, 1, 0.3f));
                EditorGUILayout.Space(3);
            }
            EditorGUILayout.EndVertical();
            Repaint();
        }
    }
#endif

    public class TSEffectExecutor : MonoBehaviour
    {
        private void Update()
        {
            foreach (var exe in TSEffect.RuntimeExecutors)
            {
                exe.Update();
            }
            TSEffect.ExecuteCallbacks();
        }
    }
}

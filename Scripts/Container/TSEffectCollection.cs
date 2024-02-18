using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TS.TSEffect.Container
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TSEffectCollection))]
    public class TSEffectCollectionEditor : UnityEditor.Editor
    {
        TSEffectCollection Coll;
        GenericMenu Menu;

        bool IsBuiltinChoiceGUIDisplayed = false;

        void SetToNotBuiltin()
        {
            Coll.IsBuiltin = false;
            Coll.ID = string.Empty;
            EditorUtility.SetDirty(Coll);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnEnable()
        {
            Coll = target as TSEffectCollection;
            TSEffect.LoadTSEffect();
            Menu = new GenericMenu();
            foreach (var pair in TSEffect.Metadata.EffectCache)
            {
                if (pair.Value.TemplateReg.Enable)
                {
                    Menu.AddItem(new GUIContent(pair.Value.TemplateReg.Path), false, () => 
                    { 
                        var s = Coll.Container.AddEffect(pair.Key);
                        s.Effect.InitEditorAnim();
                        s.SerializationTrigger = true;
                        EditorUtility.SetDirty(Coll);
                    });
                }
            }
            for (int i = 0; i < Coll.Container.Count; i++)
            {
                Coll.Container[i].Effect.InitEditorAnim();
            }

            var path = AssetDatabase.GetAssetPath(Coll).Split('/');
            if (path.Length > 3)
            {
                if (path[0] == "Assets" && path[1] == "Resources" && path[2] == "TSEffect")
                {
                    IsBuiltinChoiceGUIDisplayed = true;
                }
            }
            if (!IsBuiltinChoiceGUIDisplayed)
            {
                SetToNotBuiltin();
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            if (IsBuiltinChoiceGUIDisplayed) 
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Is Built-in");
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(300));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle(Coll.IsBuiltin, GUILayout.MaxWidth(15));
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button(Coll.IsBuiltin ? "Turn Off" : "Turn On"))
                {  
                    var path = AssetDatabase.GetAssetPath(Coll).Split('/', '.');
                    if (path.Length - 1 > 3)
                    {
                        if (path[0] == "Assets" && path[1] == "Resources" && path[2] == "TSEffect")
                        {
                            Coll.IsBuiltin = !Coll.IsBuiltin;

                            string r_path = string.Empty;
                            for (int i = 2; i < path.Length - 1; i++)
                            {
                                r_path += path[i];
                                if (i != path.Length - 2)
                                {
                                    r_path += "/";
                                }
                            }

                            if (Coll.IsBuiltin)
                            {
                                if (!TSEffect.Metadata.BuiltinEffectCollectionPaths.Contains(r_path))
                                {
                                    TSEffect.Metadata.BuiltinEffectCollectionPaths.Add(r_path);
                                    EditorUtility.SetDirty(TSEffect.Metadata);
                                }
                            }
                            else
                                TSEffect.Metadata.BuiltinEffectCollectionPaths.Remove(r_path);

                            EditorUtility.SetDirty(Coll);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            SetToNotBuiltin();
                            IsBuiltinChoiceGUIDisplayed = false;
                        }
                    }
                    else
                    {
                        SetToNotBuiltin();
                        IsBuiltinChoiceGUIDisplayed = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!Coll.IsBuiltin);
                GUILayout.Label("Executables ID");
                Coll.ID = EditorGUILayout.TextField(Coll.ID, GUILayout.MaxWidth(300));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);
            for (int i = 0; i < Coll.Container.Count; i++)
            {
                int tmp = i;
                Coll.Container[i].Effect.OnGUI(() =>
                {
                    Coll.Container.RemoveEffect(Coll.Container[tmp]);
                    EditorUtility.SetDirty(Coll);
                });
            }
            EditorGUILayout.Space(10);
            AddButton();

            if (GUI.changed)
            {
                for (int i = 0; i < Coll.Container.Count; i++)
                {
                    if (Coll.Container[i].Effect.IsGUIChanged)
                    {
                        Coll.Container[i].SerializationTrigger = true;
                    }
                }
                EditorUtility.SetDirty(Coll);
            }
            for (int i = 0; i < Coll.Container.Count; i++)
            {
                if (Coll.Container[i].Effect.ForceRepaint)
                {
                    Repaint();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        #region Add
        void AddButton()
        {
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 23);
            float width = 230;
            rect = new Rect((rect.x + rect.width) / 2f - (width / 2f) + 2, rect.y, width, rect.height);
            if (GUI.Button(rect, new GUIContent("Add Effect")))
            {
                Menu.ShowAsContext();
            }
        }
        #endregion
    }
#endif

    [CreateAssetMenu(fileName = "New Collection", menuName = "TSEffect/Collection")]
    public class TSEffectCollection : ScriptableObject
    {
        public bool IsBuiltin = false;
        public string ID = string.Empty;
        public EffectsContainer Container = new EffectsContainer();
    }
}

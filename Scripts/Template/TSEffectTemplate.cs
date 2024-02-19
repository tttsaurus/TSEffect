using System.Collections.Generic;
using System;
using UnityEngine;
using TS.TSEffect.Thread;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace TS.TSEffect.Template
{
    [Serializable]
    public class TSEffectTemplate
    {
#if UNITY_EDITOR
        [NonSerialized]
        private bool _IsAnimSetted;
        [NonSerialized]
        private AnimBool _Foldout;
        public bool IsFoldoutDisplayed { get { return _IsFoldoutDisplayed; } }
        [SerializeField]
        private bool _IsFoldoutDisplayed;
        public bool ForceRepaint { get { return _ForceRepaint; } }
        [NonSerialized]
        private bool _ForceRepaint;
        public bool IsGUIChanged { get { return _IsGUIChanged; } }
        [NonSerialized]
        private bool _IsGUIChanged;
        [NonSerialized]
        private bool _IsGUIChangedReserved;
#endif

        public bool Enable { get { return _Enable; } set { _Enable = value; } }
        [SerializeField]
        private bool _Enable;
        public string TargetType { get { return _TargetType; } protected set { _TargetType = value; } }
        [SerializeField]
        private string _TargetType;

        #region GUI
        public void InitEditorAnim()
        {
#if UNITY_EDITOR
            if (!_IsAnimSetted)
            {
                _Foldout = new AnimBool(_IsFoldoutDisplayed);
                _IsAnimSetted = true;
            }
#endif
        }
        public void DrawEditorHeaderGUI(string title, Action remove)
        {
#if UNITY_EDITOR
            if (_IsAnimSetted)
            {
                var area = GUILayoutUtility.GetRect(Screen.width, 20f);
                area.y -= 1.5f;

                #region Base
                var reg = TSEffect.Metadata.GetEffectReg(GetType());
                Color icon_color = new Color();

                if (_Enable == true)
                {
                    Color color = new Color();
                    if (reg.Exist)
                    {
                        color = reg.TemplateReg.IconColor;
                    }
                    icon_color = new Color(color.r, color.g, color.b, 0.9f);
                }
                else
                {
                    Color color = new Color();
                    if (reg.Exist)
                    {
                        color = reg.TemplateReg.IconColor;
                    }
                    icon_color = new Color(color.r, color.g, color.b, 0.6f);
                }

                EditorGUI.DrawRect(new Rect(area.x - 20, area.y + 2f, 10, area.height - 1.5f), icon_color);
                GUI.Box(new Rect(area.x - 10, area.y + 2f, area.width + 15, area.height - 1.5f), "");
                EditorGUI.HelpBox(new Rect(area.x - 20, area.y, area.width + 25, 1), "", MessageType.None);
                #endregion

                EditorGUI.BeginChangeCheck();

                #region Foldout
                _Foldout.target = EditorGUI.Foldout(new Rect(area.x + 31, area.y + 1, 10, area.height - 1), _Foldout.target, "");
                Rect fold_rect = new Rect(area.x + 31, area.y + 2, area.width - 51, area.height - 2);
                if (fold_rect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        _Foldout.target = !_Foldout.target;
                        GUI.changed = true;
                    }
                }
                #endregion
                
                #region Enable
                _Enable = EditorGUI.Toggle(new Rect(area.x, area.y + 1, 10, area.height - 1), new GUIContent(), _Enable);
                #endregion

                #region Button
                Rect button_rect = new Rect(area.width, area.y + 3, 16, area.height - 4);
                EditorGUI.LabelField(button_rect, EditorGUIUtility.IconContent("_Popup"));
                if (button_rect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Reset"), false, ()=> { OnReset(); _IsGUIChangedReserved = true; });
                        menu.AddItem(new GUIContent("Remove"), false, () => { remove(); });
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Toggle All"), false, () => { OnToggleAll(true); _IsGUIChangedReserved = true; });
                        menu.AddItem(new GUIContent("Toggle None"), false, () => { OnToggleAll(false); _IsGUIChangedReserved = true; });
                        menu.ShowAsContext();
                    }
                }
                #endregion

                EditorGUI.BeginDisabledGroup(!_Enable);

                #region Title
                GUIStyle text_style = new GUIStyle(GUI.skin.label);
                text_style.richText = true;
                EditorGUI.LabelField(new Rect(area.x + 32, area.y + 1f, area.width, area.height - 1), "<b>" + title + "</b>", text_style);
                #endregion

                _IsFoldoutDisplayed = EditorGUILayout.BeginFadeGroup(_Foldout.faded);
            }
#endif
        }
        public void DrawEditorBottomGUI()
        {
#if UNITY_EDITOR
            if (_IsAnimSetted)
            {
                EditorGUILayout.Space(6);

                EditorGUILayout.EndFadeGroup();
                EditorGUI.EndDisabledGroup();

                if (_IsGUIChangedReserved) 
                {
                    _IsGUIChangedReserved = false;
                    GUI.changed = true;
                }
                _IsGUIChanged = EditorGUI.EndChangeCheck();

                var area = GUILayoutUtility.GetRect(Screen.width, 1f);
                EditorGUI.HelpBox(new Rect(area.x - 20, area.y - 0.5f, area.width + 25, area.height), "", MessageType.None);
            }
#endif
        }
        public void DrawEditorBodyGUI()
        {
#if UNITY_EDITOR
            if (_IsAnimSetted)
            {
                if (_IsFoldoutDisplayed)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Target Type");
                    GUILayout.Label(_TargetType, GUILayout.MaxWidth(300));
                    EditorGUILayout.EndHorizontal();
                }
            }
#endif
        }
        #endregion

        #region Virtual
        protected virtual void OnInit()
        {
#if UNITY_EDITOR
            _IsAnimSetted = false;
            _IsFoldoutDisplayed = true;
            _ForceRepaint = false;
            _IsGUIChanged = false;
            _IsGUIChangedReserved = false;
#endif
            _Enable = true;
            _TargetType = string.Empty;
        }
        public virtual void GetExeFuncBuilders(string thread_field_name, out List<NormalExeFuncBuilder> init_exe, out List<NormalExeFuncBuilder> final_exe, out List<TimeExeFuncBuilder> on_exe)
        {
            init_exe = new List<NormalExeFuncBuilder>();
            final_exe = new List<NormalExeFuncBuilder>();
            on_exe = new List<TimeExeFuncBuilder>();
        }
        public virtual void OnReset()
        {
            _Enable = true;
        }
        public virtual void OnToggleAll(bool value)
        {

        }
        public virtual void OnGUI(Action remove)
        {
#if UNITY_EDITOR
            if (_IsAnimSetted)
            {
                _ForceRepaint = _Foldout.isAnimating;
            }
#endif
        }
        #endregion

        // can only be called when TSEffect is loaded
        public void RefreshRegisteredTargetType()
        {
            var reg = TSEffect.Metadata.GetEffectReg(GetType());
            if (reg.Exist)
            {
                _TargetType = reg.TemplateReg.TargetType.FullName;
            }
        }

        public TSEffectTemplate()
        {
            throw new NotSupportedException("You should construct an effect using TS.TSEffect.Util.Effect.EffectUtil.TryInstantiateEffect.");
        }
    }
}

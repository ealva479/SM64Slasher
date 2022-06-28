namespace GameCreator.Quests.UI
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine.Events;
    using GameCreator.Core;

    public class QuestUIUtilities
    {
        public class Section
        {
            private const string ICONS_PATH = "Assets/Plugins/GameCreator/Quests/Icons/UI/";
            private const string KEY_STATE = "quest-section-{0}";

            private const float ANIM_BOOL_SPEED = 3.0f;

            // PROPERTIES: ------------------------------------------------------------------------

            public GUIContent name;
            public AnimBool state;

            // INITIALIZERS: ----------------------------------------------------------------------

            public Section(string name, string icon, UnityAction repaint)
            {
                this.name = new GUIContent(
                    string.Format(" {0}", name),
                    this.GetTexture(icon)
                );

                this.state = new AnimBool(this.GetState());
                this.state.speed = ANIM_BOOL_SPEED;
                this.state.valueChanged.AddListener(repaint);
            }

            // PUBLIC METHODS: --------------------------------------------------------------------

            public void PaintSection()
            {
                GUIStyle buttonStyle = (this.state.target
                    ? CoreGUIStyles.GetToggleButtonNormalOn()
                    : CoreGUIStyles.GetToggleButtonNormalOff()
                );

                if (GUILayout.Button(this.name, buttonStyle))
                {
                    this.state.target = !this.state.target;
                    string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
                    EditorPrefs.SetBool(key, this.state.target);
                }
            }

            // PRIVATE METHODS: -------------------------------------------------------------------

            private bool GetState()
            {
                string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
                return EditorPrefs.GetBool(key, true);
            }

            private Texture2D GetTexture(string icon)
            {
                string path = Path.Combine(ICONS_PATH, icon);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }
    }
}
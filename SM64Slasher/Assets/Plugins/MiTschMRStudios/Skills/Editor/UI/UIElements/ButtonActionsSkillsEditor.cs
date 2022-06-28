namespace MiTschMR.Skills
{
    using GameCreator.Core;
    using UnityEditor;
    using UnityEditor.UI;
    using UnityEngine;
    using UnityEngine.UI;

    [CustomEditor(typeof(ButtonActionsSkills))]
    public class ButtonActionsSkillsEditor : SelectableEditor
    {
        protected ActionsEditor editorActions;
        SerializedProperty spOnClickRight;

        protected override void OnEnable()
        {
            base.OnEnable();

            spOnClickRight = serializedObject.FindProperty("onClickRight");

            SerializedProperty spActions = serializedObject.FindProperty("actions");
            if (spActions.objectReferenceValue != null)
            {
                this.editorActions = Editor.CreateEditor(
                    spActions.objectReferenceValue
                ) as ActionsEditor;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();

            EditorGUILayout.LabelField("Left click");
            if (this.editorActions != null)
            {
                this.editorActions.OnInspectorGUI();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Right click");
            EditorGUILayout.PropertyField(spOnClickRight);
            serializedObject.ApplyModifiedProperties();
        }

        // CREATE: --------------------------------------------------------------------------------

        [MenuItem("GameObject/Game Creator/UI/Button Skill", false, 30)]
        public static void CreateButtonActions()
        {
            GameObject canvas = CreateSceneObject.GetCanvasGameObject();
            GameObject buttonGO = DefaultControls.CreateButton(CreateSceneObject.GetStandardResources());
            buttonGO.transform.SetParent(canvas.transform, false);

            Button button = buttonGO.GetComponent<Button>();
            Graphic targetGraphic = button.targetGraphic;

            DestroyImmediate(button);
            ButtonActions buttonActions = buttonGO.AddComponent<ButtonActions>();
            buttonActions.targetGraphic = targetGraphic;
            Selection.activeGameObject = buttonGO;
        }
    }
}
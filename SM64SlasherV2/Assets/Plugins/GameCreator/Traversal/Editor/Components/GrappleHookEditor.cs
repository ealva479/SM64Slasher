using UnityEditor;

namespace GameCreator.Traversal
{
    [CustomEditor(typeof(GrappleHook), true)]
    public class GrappleHookEditor : ObstacleEditor
    {
        private SerializedProperty anchor;
        private SerializedProperty grappleHookMaterial;
        private SerializedProperty grappleHookTextureMode;
        private SerializedProperty grappleHookWidth;
        private SerializedProperty grappleHookResolution;

        // Throw phase: 
        private SerializedProperty grappleHookThrowDelay;
        private SerializedProperty grappleHookThrowDuration;
        private SerializedProperty grappleHookThrowPeriod;
        private SerializedProperty grappleHookThrowAmplitude;

        // Reel phase:
        private SerializedProperty grappleHookReelDelay;
        private SerializedProperty grappleHookReelDuration;
        private SerializedProperty grappleHookReelPeriod;
        private SerializedProperty grappleHookReelAmplitude;
        
        // INITIALIZE: ----------------------------------------------------------------------------
        
        protected override void OnEnable()
        {
            base.OnEnable();

            this.anchor = this.serializedObject.FindProperty("anchor");
            this.grappleHookMaterial = this.serializedObject.FindProperty("grappleHookMaterial");
            this.grappleHookTextureMode = this.serializedObject.FindProperty("grappleHookTextureMode");
            this.grappleHookWidth = this.serializedObject.FindProperty("grappleHookWidth");
            this.grappleHookResolution = this.serializedObject.FindProperty("grappleHookResolution");
            
            this.grappleHookThrowDelay = this.serializedObject.FindProperty("grappleHookThrowDelay");
            this.grappleHookThrowDuration = this.serializedObject.FindProperty("grappleHookThrowDuration");
            this.grappleHookThrowPeriod = this.serializedObject.FindProperty("grappleHookThrowPeriod");
            this.grappleHookThrowAmplitude = this.serializedObject.FindProperty("grappleHookThrowAmplitude");
                
            this.grappleHookReelDelay = this.serializedObject.FindProperty("grappleHookReelDelay");
            this.grappleHookReelDuration = this.serializedObject.FindProperty("grappleHookReelDuration");
            this.grappleHookReelPeriod = this.serializedObject.FindProperty("grappleHookReelPeriod");
            this.grappleHookReelAmplitude = this.serializedObject.FindProperty("grappleHookReelAmplitude");
        }

        // PAINT INSPECTOR: -----------------------------------------------------------------------
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            this.serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grapple Hook:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.anchor);
            EditorGUILayout.PropertyField(this.grappleHookMaterial);
            EditorGUILayout.PropertyField(this.grappleHookTextureMode);
            EditorGUILayout.PropertyField(this.grappleHookWidth);
            EditorGUILayout.PropertyField(this.grappleHookResolution);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grapple Throw Phase:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.grappleHookThrowDelay);
            EditorGUILayout.PropertyField(this.grappleHookThrowDuration);
            EditorGUILayout.PropertyField(this.grappleHookThrowPeriod);
            EditorGUILayout.PropertyField(this.grappleHookThrowAmplitude);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grapple Reel Phase:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.grappleHookReelDelay);
            EditorGUILayout.PropertyField(this.grappleHookReelDuration);
            EditorGUILayout.PropertyField(this.grappleHookReelPeriod);
            EditorGUILayout.PropertyField(this.grappleHookReelAmplitude);
            
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
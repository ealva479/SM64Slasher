using System;
using System.IO;
using System.Reflection;
using GameCreator.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NJG.Graph
{
    [CustomEditor(typeof(GraphTrigger))]
    public class GraphTriggerInspector : Editor
    {
        private const string MSG_REQUIRE_HAVE_COLLIDER = "This type of Trigger requires a Collider. Select one from below";

        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        private const float DOTTED_LINES_SIZE = 2.0f;
        private const string KEY_IGNITER_INDEX_PREF = "gamecreator-igniters-index2";

        private const string ICONS_PATH = "Assets/Plugins/GameCreator/Extra/Icons/Trigger/{0}";
        private const float ITEMS_TOOLBAR_WIDTH = 25f;

        private const string PROP_OPTION = "option";
        private const string PROP_ACTIONS = "actions";
        private const string PROP_CONDITIONS = "conditions";

        private static readonly Type[] COLLIDER_TYPES = {
            typeof(SphereCollider),
            typeof(BoxCollider),
            typeof(CapsuleCollider),
            typeof(MeshCollider)
        };

        private class IgniterCache
        {
            public GUIContent name;
            public string comment;
            public bool requiresCollider;
            public SerializedObject serializedObject;

            public IgniterCache(Object reference)
            {
                if (reference == null)
                {
                    name = new GUIContent("Undefined");
                    requiresCollider = false;
                    serializedObject = null;
                    return;
                }

                string igniterName = (string)reference.GetType().GetField("NAME", BINDING_FLAGS).GetValue(null);
                string iconPath = (string)reference.GetType().GetField("ICON_PATH", BINDING_FLAGS).GetValue(null);

                if (!string.IsNullOrEmpty(igniterName))
                {
                    string[] igniterNameSplit = igniterName.Split('/');
                    igniterName = igniterNameSplit[igniterNameSplit.Length - 1];
                }

                Texture2D igniterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(iconPath, igniterName + ".png"));
                if (igniterIcon == null) igniterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "Default.png");
                if (igniterIcon == null) igniterIcon = EditorGUIUtility.FindTexture("GameObject Icon");

                name = new GUIContent(" " + igniterName, igniterIcon);
                comment = (string)reference.GetType().GetField("COMMENT", BINDING_FLAGS).GetValue(null);
                requiresCollider = (bool)reference.GetType().GetField("REQUIRES_COLLIDER", BINDING_FLAGS).GetValue(null);
                if(serializedObject == null) serializedObject = new SerializedObject(reference);
            }
        }

        private static string[] IGNITERS_PLATFORM_NAMES = new string[0];

        private static GUIContent GC_ACTIONS;
        private static GUIContent GC_CONDITIONS;
        private static GUIContent GC_SETTINGS;
        private static GUIContent GC_HOTSPOT;

        public static bool HasChanged { get; set; }

        // PROPERTIES: ----------------------------------------------------------------------------

        private Trigger trigger;

        private int ignitersIndex;
        private SerializedProperty spIgnitersKeys;
        private SerializedProperty spIgnitersValues;
        private IgniterCache[] ignitersCache;
        private bool updateIgnitersPlatforms;
        private Rect selectIgniterButtonRect = Rect.zero;

        private SerializedProperty spTrigger;
        private SerializedProperty spTriggerKeyCode;

        private SerializedProperty spItems;
        private EditorSortableList sortableList;

        private bool foldoutAdvancedSettings;
        private SerializedProperty spMinDistance;
        private SerializedProperty spMinDistanceToPlayer;

        // INITIALIZERS: -----------------------------------------------------------------------------------------------

        public void OnEnable()
        {
            if (target == null) return;
            trigger = (Trigger)target;

            SerializedProperty spIgniters = serializedObject.FindProperty("igniters");
            spIgnitersKeys = spIgniters.FindPropertyRelative("keys");
            spIgnitersValues = spIgniters.FindPropertyRelative("values");
            
            if (spIgnitersKeys.arraySize == 0)
            {
                Igniter igniter = trigger.gameObject.AddComponent<IgniterStart>();
                igniter.Setup(trigger);
                igniter.enabled = false;

                spIgnitersKeys.InsertArrayElementAtIndex(0);
                spIgnitersValues.InsertArrayElementAtIndex(0);

                spIgnitersKeys.GetArrayElementAtIndex(0).intValue = Trigger.ALL_PLATFORMS_KEY;
                spIgnitersValues.GetArrayElementAtIndex(0).objectReferenceValue = igniter;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            UpdateIgnitersPlatforms();

            ignitersIndex = EditorPrefs.GetInt(KEY_IGNITER_INDEX_PREF, 0);
            if (ignitersIndex >= spIgnitersKeys.arraySize)
            {
                ignitersIndex = spIgnitersKeys.arraySize - 1;
                EditorPrefs.SetInt(KEY_IGNITER_INDEX_PREF, ignitersIndex);
            }

            spItems = serializedObject.FindProperty("items");
            if(sortableList == null) sortableList = new EditorSortableList();

            spMinDistance = serializedObject.FindProperty("minDistance");
            spMinDistanceToPlayer = serializedObject.FindProperty("minDistanceToPlayer");
        }

        private void OnValidate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                OnEnable();
            }
        }

        // INSPECTOR: --------------------------------------------------------------------------------------------------

        public void UpdateIgniters()
        {
            UpdateIgnitersPlatforms();
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            serializedObject.Update();

            if (updateIgnitersPlatforms)
            {
                UpdateIgnitersPlatforms();
                updateIgnitersPlatforms = false;
            }

            if (GC_ACTIONS == null || GC_CONDITIONS == null || GC_HOTSPOT == null || GC_SETTINGS == null)
            {
                GC_ACTIONS = new GUIContent(
                    AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "actions.png")),
                    "Create an Actions slot"
                );
                GC_CONDITIONS = new GUIContent(
                    AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "conditions.png")),
                    "Create a Conditions slot"
                );
                GC_HOTSPOT = new GUIContent(
                    AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "hotspot.png")),
                    "Add a Hotspot"
                );
                GC_SETTINGS = new GUIContent(
                    AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(ICONS_PATH, "settings.png")),
                    "Open/Close Advanced Settings"
                );
            }

            DoLayoutConfigurationOptions();

            //this.PaintItemsToolbar();
            //this.PaintItems();

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        private void PaintAdvancedSettings()
        {
            EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(spMinDistance);
            EditorGUI.BeginDisabledGroup(!spMinDistance.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(spMinDistanceToPlayer);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel--;
        }

        private void DoLayoutConfigurationOptions()
        {
            int removeIndex = -1;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            int ignIndex = GUILayout.Toolbar(ignitersIndex, IGNITERS_PLATFORM_NAMES);
            if (ignIndex != ignitersIndex)
            {
                ignitersIndex = ignIndex;
                EditorPrefs.SetInt(KEY_IGNITER_INDEX_PREF, ignitersIndex);
            }

            if (GUILayout.Button("+", CoreGUIStyles.GetButtonLeft(), GUILayout.Width(25f)))
            {
                SelectPlatformMenu();
            }

            EditorGUI.BeginDisabledGroup(ignitersIndex == 0);
            if (GUILayout.Button("-", CoreGUIStyles.GetButtonMid(), GUILayout.Width(25f)))
            {
                removeIndex = ignitersIndex;
            }
            EditorGUI.EndDisabledGroup();

            GUIStyle settingStyle = (foldoutAdvancedSettings
                ? CoreGUIStyles.GetToggleButtonRightOn()
                : CoreGUIStyles.GetToggleButtonRightOff()
            );

            if (GUILayout.Button(GC_SETTINGS, settingStyle, GUILayout.Width(25f), GUILayout.Height(18f)))
            {
                foldoutAdvancedSettings = !foldoutAdvancedSettings;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(ignitersCache[ignitersIndex].name, EditorStyles.miniBoldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Change Trigger", GUILayout.Width(SelectTypePanel.WINDOW_WIDTH)))
            {
                SelectTypePanel selectTypePanel = new SelectTypePanel(SelectNewIgniter, "Triggers", typeof(Igniter));
                PopupWindow.Show(selectIgniterButtonRect, selectTypePanel);
            }

            if (Event.current.type == EventType.Repaint)
            {
                selectIgniterButtonRect = GUILayoutUtility.GetLastRect();
            }

            EditorGUILayout.EndHorizontal();

            if (ignitersCache[ignitersIndex].serializedObject != null)
            {
                string comment = ignitersCache[ignitersIndex].comment;
                if (!string.IsNullOrEmpty(comment)) EditorGUILayout.HelpBox(comment, MessageType.Info);

                Igniter.PaintEditor(ignitersCache[ignitersIndex].serializedObject);
            }

            if (ignitersCache[ignitersIndex].requiresCollider)
            {
                Collider collider = trigger.GetComponent<Collider>();
                if (!collider) PaintNoCollider();
                else
                {
                    EditorGUILayout.Space();
                    PaintCollider(collider);
                }
            }

            if (foldoutAdvancedSettings)
            {
                EditorGUILayout.Space();
                PaintAdvancedSettings();
            }

            EditorGUILayout.EndVertical();

            if (removeIndex > 0)
            {
                Object obj = spIgnitersValues.GetArrayElementAtIndex(removeIndex).objectReferenceValue;
                spIgnitersValues.GetArrayElementAtIndex(removeIndex).objectReferenceValue = null;

                spIgnitersKeys.DeleteArrayElementAtIndex(removeIndex);
                spIgnitersValues.DeleteArrayElementAtIndex(removeIndex);

                if (obj != null) DestroyImmediate(obj, true);

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                updateIgnitersPlatforms = true;
                if (ignitersIndex >= spIgnitersKeys.arraySize)
                    ignitersIndex = spIgnitersKeys.arraySize - 1;

                HasChanged = true;
            }
        }

        private void PaintCollider(Collider collider )
        {
            var so = new SerializedObject(collider);
            so.Update();
            SerializedProperty iterator = so.GetIterator();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(GraphEditor.Controller && !GraphEditor.Controller.overrideColliderValues);
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if ("m_Script" == iterator.propertyPath) continue;
                EditorGUILayout.PropertyField(iterator, true);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            if (GraphEditor.Controller && !GraphEditor.Controller.overrideColliderValues)
            {
                EditorGUILayout.HelpBox("Override Collider is turned off. Collider values can only be changed in the controller.", MessageType.Warning);
            }
            if (GUILayout.Button("Remove Collider"))
            {
                DestroyImmediate(collider, true);
                if(GraphEditor.Controller) GraphEditor.Controller.SyncCollider(false);
                return;
            }

            if (EditorGUI.EndChangeCheck() && GraphEditor.Controller)
            {
                var so2 = new SerializedObject(GraphEditor.Controller);
                so2.Update();
                GraphEditor.Controller.SyncCollider(false);
                so2.ApplyModifiedProperties();
            }

            so.ApplyModifiedProperties();
        }

        private void SelectPlatformCallback(object data)
        {
            if (trigger.igniters.ContainsKey((int)data)) return;

            int index = spIgnitersKeys.arraySize;
            spIgnitersKeys.InsertArrayElementAtIndex(index);
            spIgnitersValues.InsertArrayElementAtIndex(index);

            spIgnitersKeys.GetArrayElementAtIndex(index).intValue = (int)data;

            Igniter igniter = trigger.gameObject.AddComponent<IgniterStart>();
            igniter.Setup(trigger);
            igniter.enabled = false;

            spIgnitersValues.GetArrayElementAtIndex(index).objectReferenceValue = igniter;

            ignitersIndex = index;
            EditorPrefs.SetInt(KEY_IGNITER_INDEX_PREF, ignitersIndex);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            updateIgnitersPlatforms = true;

            HasChanged = true;
        }

        private void SelectPlatformMenu()
        {
            GenericMenu menu = new GenericMenu();

            foreach (Trigger.Platforms platform in Enum.GetValues(typeof(Trigger.Platforms)))
            {
                bool disabled = trigger.igniters.ContainsKey((int)platform);
                menu.AddItem(new GUIContent(platform.ToString()), disabled, SelectPlatformCallback, (int)platform);
            }

            menu.ShowAsContext();
        }

        private void PaintItemsToolbar()
        {
            Rect rectItem = GUILayoutUtility.GetRect(
                GUIContent.none, CoreGUIStyles.GetToggleButtonOff()
            );

            Rect rectItem1 = new Rect(
                rectItem.x,
                rectItem.y,
                ITEMS_TOOLBAR_WIDTH,
                rectItem.height
            );
            Rect rectItem2 = new Rect(
                rectItem1.x + rectItem1.width,
                rectItem1.y,
                rectItem1.width,
                rectItem1.height
            );
            Rect rectItem3 = new Rect(
                rectItem2.x + rectItem2.width,
                rectItem2.y,
                rectItem2.width,
                rectItem2.height
            );
            Rect rectItemH = new Rect(
                //rectItem.x + (rectItem.width - ITEMS_TOOLBAR_WIDTH),
                rectItem3.x + rectItem3.width + 5f,
                rectItem2.y,
                ITEMS_TOOLBAR_WIDTH,
                rectItem2.height
            );

            if (GUI.Button(rectItem1, GC_ACTIONS, CoreGUIStyles.GetButtonLeft()))
            {
                int index = spItems.arraySize;
                spItems.InsertArrayElementAtIndex(index);

                SerializedProperty spItem = spItems.GetArrayElementAtIndex(index);
                spItem.FindPropertyRelative(PROP_OPTION).intValue = (int)Trigger.ItemOpts.Actions;
                spItem.FindPropertyRelative(PROP_ACTIONS).objectReferenceValue = CreateSubObject<Actions>();
                spItem.FindPropertyRelative(PROP_CONDITIONS).objectReferenceValue = null;
            }

            if (GUI.Button(rectItem2, GC_CONDITIONS, CoreGUIStyles.GetButtonMid()))
            {
                int index = spItems.arraySize;
                spItems.InsertArrayElementAtIndex(index);

                SerializedProperty spItem = spItems.GetArrayElementAtIndex(index);
                spItem.FindPropertyRelative(PROP_OPTION).intValue = (int)Trigger.ItemOpts.Conditions;
                spItem.FindPropertyRelative(PROP_ACTIONS).objectReferenceValue = null;
                spItem.FindPropertyRelative(PROP_CONDITIONS).objectReferenceValue = CreateSubObject<Conditions>();
            }

            if (GUI.Button(rectItem3, "+", CoreGUIStyles.GetButtonRight()))
            {
                int index = spItems.arraySize;
                spItems.InsertArrayElementAtIndex(index);

                SerializedProperty spItem = spItems.GetArrayElementAtIndex(index);
                spItem.FindPropertyRelative(PROP_OPTION).intValue = (int)Trigger.ItemOpts.Actions;
                spItem.FindPropertyRelative(PROP_ACTIONS).objectReferenceValue = null;
                spItem.FindPropertyRelative(PROP_CONDITIONS).objectReferenceValue = null;
            }

            EditorGUI.BeginDisabledGroup(trigger.gameObject.GetComponent<Hotspot>() != null);
            if (GUI.Button(rectItemH, GC_HOTSPOT))
            {
                Undo.AddComponent<Hotspot>(trigger.gameObject);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void PaintItems()
        {
            int itemsCount = spItems.arraySize;
            int removeIndex = -1;
            bool forceRepaint = false;

            GUIContent gcDelete = ClausesUtilities.Get(ClausesUtilities.Icon.Delete);

            for (int i = 0; i < itemsCount; ++i)
            {
                SerializedProperty spItem = spItems.GetArrayElementAtIndex(i);
                SerializedProperty spIOption = spItem.FindPropertyRelative(PROP_OPTION);
                SerializedProperty spIActions = spItem.FindPropertyRelative(PROP_ACTIONS);
                SerializedProperty spIConditions = spItem.FindPropertyRelative(PROP_CONDITIONS);

                Rect rectItem = GUILayoutUtility.GetRect(GUIContent.none, CoreGUIStyles.GetToggleButtonNormalOff());

                Rect rectHandle = new Rect(
                    rectItem.x,
                    rectItem.y,
                    25f,
                    rectItem.height
                );

                Rect rectToggle = new Rect(
                    rectHandle.x + rectHandle.width,
                    rectHandle.y,
                    25f,
                    rectHandle.height
                );

                Rect rectDelete = new Rect(
                    rectItem.x + (rectItem.width - 25f),
                    rectToggle.y,
                    25f,
                    rectToggle.height
                );

                Rect rectCont = new Rect(
                    rectToggle.x + rectToggle.width,
                    rectToggle.y,
                    rectItem.width - (rectHandle.width + rectToggle.width + rectDelete.width),
                    rectToggle.height
                );

                GUI.Label(rectHandle, "=", CoreGUIStyles.GetButtonLeft());
                bool forceSortRepaint = sortableList.CaptureSortEvents(rectHandle, i);
                forceRepaint = forceSortRepaint || forceRepaint;

                EditorGUIUtility.AddCursorRect(rectHandle, MouseCursor.Pan);

                GUIContent gcToggle = null;
                if (spIOption.intValue == (int)Trigger.ItemOpts.Actions) gcToggle = GC_ACTIONS;
                if (spIOption.intValue == (int)Trigger.ItemOpts.Conditions) gcToggle = GC_CONDITIONS;

                if (GUI.Button(rectToggle, gcToggle, CoreGUIStyles.GetButtonMid()))
                {
                    switch (spIOption.intValue)
                    {
                        case (int)Trigger.ItemOpts.Actions:
                            spIOption.intValue = (int)Trigger.ItemOpts.Conditions;
                            break;

                        case (int)Trigger.ItemOpts.Conditions:
                            spIOption.intValue = (int)Trigger.ItemOpts.Actions;
                            break;
                    }
                }

                GUI.Label(rectCont, string.Empty, CoreGUIStyles.GetButtonMid());
                Rect rectField = new Rect(
                    rectCont.x + 2f,
                    rectCont.y + (rectCont.height / 2f - EditorGUIUtility.singleLineHeight / 2f),
                    rectCont.width - 7f,
                    EditorGUIUtility.singleLineHeight
                );

                switch (spIOption.intValue)
                {
                    case (int)Trigger.ItemOpts.Actions:
                        EditorGUI.PropertyField(rectField, spIActions, GUIContent.none, true);
                        break;

                    case (int)Trigger.ItemOpts.Conditions:
                        EditorGUI.PropertyField(rectField, spIConditions, GUIContent.none, true);
                        break;
                }


                if (GUI.Button(rectDelete, gcDelete, CoreGUIStyles.GetButtonRight()))
                {
                    removeIndex = i;
                }

                sortableList.PaintDropPoints(rectItem, i, itemsCount);
            }

            if (removeIndex != -1 && removeIndex < spItems.arraySize)
            {
                SerializedProperty spItem = spItems.GetArrayElementAtIndex(removeIndex);
                SerializedProperty spIOption = spItem.FindPropertyRelative(PROP_OPTION);
                SerializedProperty spIActions = spItem.FindPropertyRelative(PROP_ACTIONS);
                SerializedProperty spIConditions = spItem.FindPropertyRelative(PROP_CONDITIONS);
                Object @object = null;
                switch (spIOption.intValue)
                {
                    case (int)Trigger.ItemOpts.Actions: @object = spIActions.objectReferenceValue; break;
                    case (int)Trigger.ItemOpts.Conditions: @object = spIConditions.objectReferenceValue; break;
                }

                spItems.DeleteArrayElementAtIndex(removeIndex);
            }

            EditorSortableList.SwapIndexes swapIndexes = sortableList.GetSortIndexes();
            if (swapIndexes != null)
            {
                spItems.MoveArrayElement(swapIndexes.src, swapIndexes.dst);
            }

            if (forceRepaint) Repaint();
        }

        private T CreateSubObject<T>() where T : MonoBehaviour
        {
            if (PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.Regular || PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.Variant)
            {
                return CreatePrefabObject.AddGameObjectToPrefab<T>(
                    PrefabUtility.GetOutermostPrefabInstanceRoot(trigger.gameObject),
                    typeof(T).Name
                );
            }

            GameObject asset = CreateSceneObject.Create(typeof(T).Name, false);
            return asset.AddComponent<T>();
        }

        private void PaintNoCollider()
        {
            EditorGUILayout.HelpBox(MSG_REQUIRE_HAVE_COLLIDER, MessageType.Error);

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < COLLIDER_TYPES.Length; ++i)
            {
                GUIStyle style = CoreGUIStyles.GetButtonMid();
                if (i == 0) style = CoreGUIStyles.GetButtonLeft();
                else if (i >= COLLIDER_TYPES.Length - 1) style = CoreGUIStyles.GetButtonRight();

                if (GUILayout.Button(COLLIDER_TYPES[i].Name, style))
                {
                    Undo.AddComponent(trigger.gameObject, COLLIDER_TYPES[i]);
                    if(GraphEditor.Controller) GraphEditor.Controller.SyncCollider(false);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        // SCENE METHODS: -------------------------------------------------------------------------

        private void OnSceneGUI()
        {
            for (int i = 0; i < trigger.items.Count; ++i)
            {
                if (trigger.items[i].option == Trigger.ItemOpts.Actions &&
                    trigger.items[i].actions != null)
                {
                    PaintLine(
                        trigger.transform,
                        trigger.items[i].actions.transform,
                        Color.cyan
                    );
                }
                else if (trigger.items[i].option == Trigger.ItemOpts.Conditions &&
                         trigger.items[i].conditions != null)
                {
                    PaintLine(
                        trigger.transform,
                        trigger.items[i].conditions.transform,
                        Color.green
                    );
                }
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Rect GetCenteredRect(Rect rect, float height)
        {
            return new Rect(
                rect.x,
                rect.y + (rect.height - height) / 2.0f,
                rect.width,
                height
            );
        }

        private void UpdateIgnitersPlatforms()
        {
            int numKeys = spIgnitersKeys.arraySize;

            ignitersCache = new IgniterCache[numKeys];
            IGNITERS_PLATFORM_NAMES = new string[numKeys];

            for (int i = 0; i < numKeys; ++i)
            {
                if (i == 0) IGNITERS_PLATFORM_NAMES[0] = "Any Platform";
                else
                {
                    int key = spIgnitersKeys.GetArrayElementAtIndex(i).intValue;
                    IGNITERS_PLATFORM_NAMES[i] = ((Trigger.Platforms)key).ToString();
                }

                Object reference = spIgnitersValues.GetArrayElementAtIndex(i).objectReferenceValue;
                ignitersCache[i] = new IgniterCache(reference);
            }

            HasChanged = true;
        }

        private void SelectNewIgniter(Type igniterType)
        {
            SerializedProperty property = spIgnitersValues.GetArrayElementAtIndex(ignitersIndex);
            if (property.objectReferenceValue != null)
            {
                DestroyImmediate(property.objectReferenceValue, true);
                property.objectReferenceValue = null;
            }

            Igniter igniter = (Igniter)trigger.gameObject.AddComponent(igniterType);
            igniter.Setup(trigger);
            igniter.enabled = false;

            property.objectReferenceValue = igniter;
            ignitersCache[ignitersIndex] = new IgniterCache(igniter);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            HasChanged = true;
        }

        private void PaintLine(Transform transform1, Transform transform2, Color color)
        {
            Handles.color = color;
            Handles.DrawDottedLine(
                transform1.position,
                transform2.position,
                DOTTED_LINES_SIZE
            );
        }
    }
}
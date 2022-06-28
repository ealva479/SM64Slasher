namespace MiTschMR.Skills
{
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	public class SkillHolderSerializedPropertyDrawerWindow : PopupWindowContent
	{
		protected const string INPUTTEXT_NAME = "gamecreator-skillholder-input";
		protected const float WIN_HEIGHT = 300f;
		protected static DatabaseSkills DATABASE_SKILLS;

		protected Rect windowRect = Rect.zero;
		protected bool inputfieldFocus = true;
		protected Vector2 scroll = Vector2.zero;
		protected int skillsIndex = -1;

		protected string searchText = "";
		protected List<int> suggestions = new List<int>();

		protected GUIStyle inputBGStyle;
		protected GUIStyle suggestionHeaderStyle;
		protected GUIStyle suggestionSkillStyle;
		protected GUIStyle searchFieldStyle;
		protected GUIStyle searchCloseOnStyle;
		protected GUIStyle searchCloseOffStyle;

		protected SerializedProperty property;

		protected bool keyPressedAny = false;
		protected bool keyPressedUp = false;
		protected bool keyPressedDown = false;
		protected bool keyPressedEnter = false;
		protected bool keyFlagVerticalMoved = false;
		protected Rect skillSelectedRect = Rect.zero;

		// PUBLIC METHODS: ---------------------------------------------------------------------------------------------

		public SkillHolderSerializedPropertyDrawerWindow(Rect activatorRect, SerializedProperty property)
		{
			this.windowRect = new Rect(
				activatorRect.x,
				activatorRect.y + activatorRect.height,
				activatorRect.width,
				WIN_HEIGHT
			);

			this.inputfieldFocus = true;
			this.scroll = Vector2.zero;
			this.property = property;

			if (DATABASE_SKILLS == null) DATABASE_SKILLS = DatabaseSkills.LoadDatabase<DatabaseSkills>();
		}

		public override Vector2 GetWindowSize() => new Vector2(this.windowRect.width, WIN_HEIGHT);

		public override void OnOpen()
		{
			this.inputBGStyle = new GUIStyle(GUI.skin.FindStyle("TabWindowBackground"));
            this.suggestionHeaderStyle = new GUIStyle(GUI.skin.FindStyle("IN BigTitle")) { margin = new RectOffset(0, 0, 0, 0) };
            this.suggestionSkillStyle = new GUIStyle(GUI.skin.FindStyle("MenuItem"));
			this.searchFieldStyle = new GUIStyle(GUI.skin.FindStyle("SearchTextField"));
			this.searchCloseOnStyle = new GUIStyle(GUI.skin.FindStyle("SearchCancelButton"));
			this.searchCloseOffStyle = new GUIStyle(GUI.skin.FindStyle("SearchCancelButtonEmpty"));

			this.inputfieldFocus = true;

			this.searchText = "";
			this.suggestions = DATABASE_SKILLS.GetSkillSuggestions(this.searchText);
		}

		// GUI METHODS: ------------------------------------------------------------------------------------------------

		public override void OnGUI(Rect windowRect)
		{
			if (this.property == null) { this.editorWindow.Close(); return; }
			this.property.serializedObject.Update();

			this.HandleKeyboardInput();

			string modSearchText = this.searchText;
			this.PaintInputfield(ref modSearchText);
			this.PaintSuggestions(ref modSearchText);

			this.searchText = modSearchText;

			this.property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

			if (this.keyPressedEnter)
			{
				this.editorWindow.Close();
				Event.current.Use();
			}

			bool repaintEvent = false;
			repaintEvent = repaintEvent || Event.current.type == EventType.MouseMove;
			repaintEvent = repaintEvent || Event.current.type == EventType.MouseDown;
			repaintEvent = repaintEvent || this.keyPressedAny;
			if (repaintEvent) this.editorWindow.Repaint();
		}

		// PRIVATE METHODS: --------------------------------------------------------------------------------------------

		protected virtual void HandleKeyboardInput()
		{
			this.keyPressedAny = false;
			this.keyPressedUp = false;
			this.keyPressedDown = false;
			this.keyPressedEnter = false;

			if (Event.current.type != EventType.KeyDown) return;

			this.keyPressedAny = true;
			this.keyPressedUp = (Event.current.keyCode == KeyCode.UpArrow);
			this.keyPressedDown = (Event.current.keyCode == KeyCode.DownArrow);

			this.keyPressedEnter = (
				Event.current.keyCode == KeyCode.KeypadEnter ||
				Event.current.keyCode == KeyCode.Return
			);

			this.keyFlagVerticalMoved = (
				this.keyPressedUp ||
				this.keyPressedDown
			);
		}

		protected virtual void PaintInputfield(ref string modifiedText)
		{
			EditorGUILayout.BeginHorizontal(this.inputBGStyle);

			GUI.SetNextControlName(INPUTTEXT_NAME);
			modifiedText = EditorGUILayout.TextField(GUIContent.none, modifiedText, this.searchFieldStyle);


			GUIStyle style = (string.IsNullOrEmpty(this.searchText)
				? this.searchCloseOffStyle
				: this.searchCloseOnStyle
			);

			if (this.inputfieldFocus)
			{
				EditorGUI.FocusTextInControl(INPUTTEXT_NAME);
				this.inputfieldFocus = false;
			}

			if (GUILayout.Button("", style))
			{
				modifiedText = "";
				GUIUtility.keyboardControl = 0;
				EditorGUIUtility.keyboardControl = 0;
				this.inputfieldFocus = true;
			}

			EditorGUILayout.EndHorizontal();
		}

		protected virtual void PaintSuggestions(ref string modifiedText)
		{
			EditorGUILayout.BeginHorizontal(this.suggestionHeaderStyle);
			EditorGUILayout.LabelField("Suggestions", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();

			this.scroll = EditorGUILayout.BeginScrollView(this.scroll);
			if (modifiedText != this.searchText) this.suggestions = DATABASE_SKILLS.GetSkillSuggestions(modifiedText);

			int suggestionCount = this.suggestions.Count;

			if (suggestionCount > 0)
			{
				for (int i = 0; i < suggestionCount; ++i)
				{
					SkillRelyOn skill = DATABASE_SKILLS.GetSkillListSkillByIndex(this.suggestions[i]).CopySkillToSkillRelyOn();
					string skillName = (string.IsNullOrEmpty(skill.skillName.content) ? "No-name" : skill.skillName.content);
					GUIContent skillContent = new GUIContent(skillName);

					Rect skillRect = GUILayoutUtility.GetRect(skillContent, this.suggestionSkillStyle);
					bool skillHasFocus = (i == this.skillsIndex);
					bool mouseEnter = skillHasFocus && Event.current.type == EventType.MouseDown;

					if (Event.current.type == EventType.Repaint)
					{
						this.suggestionSkillStyle.Draw(
							skillRect,
							skillContent,
							skillHasFocus,
							skillHasFocus,
							false,
							false
						);
					}

					if (this.skillsIndex == i) this.skillSelectedRect = skillRect;

					if (skillHasFocus)
					{
						if (mouseEnter || this.keyPressedEnter)
						{
							if (this.keyPressedEnter) Event.current.Use();
							modifiedText = skillName;
							SerializedProperty spSkill = this.property.FindPropertyRelative(SkillHolderPropertyDrawer.PROP_SKILL);
							if (spSkill.objectReferenceValue == null)
                            {
								skill = Skill.CreateSkillRelyOnInstance(skill);
								spSkill.objectReferenceValue = skill;
							}
                            else
                            {
								SkillRelyOn skillReturned = (SkillRelyOn)spSkill.objectReferenceValue;
								skillReturned = Skill.UpdateSkillRelyOnInstance(skillReturned, skill);
								EditorUtility.SetDirty(skillReturned);
								spSkill.objectReferenceValue = skillReturned;
							}
							
							this.property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
							this.property.serializedObject.Update();

							this.editorWindow.Close();
						}
					}

					if (Event.current.type == EventType.MouseMove &&
						GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
					{
						this.skillsIndex = i;
					}
				}

				if (this.keyPressedDown && this.skillsIndex < suggestionCount - 1)
				{
					this.skillsIndex++;
					Event.current.Use();
				}
				else if (this.keyPressedUp && this.skillsIndex > 0)
				{
					this.skillsIndex--;
					Event.current.Use();
				}
			}

			EditorGUILayout.EndScrollView();
			float scrollHeight = GUILayoutUtility.GetLastRect().height;

			if (Event.current.type == EventType.Repaint && this.keyFlagVerticalMoved)
			{
				this.keyFlagVerticalMoved = false;
				if (this.skillSelectedRect != Rect.zero)
				{
					bool isUpperLimit = this.scroll.y > this.skillSelectedRect.y;
					bool isLowerLimit = (this.scroll.y + scrollHeight <
						this.skillSelectedRect.position.y + this.skillSelectedRect.size.y
					);

					if (isUpperLimit)
					{
						this.scroll = Vector2.up * (this.skillSelectedRect.position.y);
						this.editorWindow.Repaint();
					}
					else if (isLowerLimit)
					{
						float positionY = this.skillSelectedRect.y + this.skillSelectedRect.height - scrollHeight;
						this.scroll = Vector2.up * positionY;
						this.editorWindow.Repaint();
					}
				}
			}
		}
	}
}
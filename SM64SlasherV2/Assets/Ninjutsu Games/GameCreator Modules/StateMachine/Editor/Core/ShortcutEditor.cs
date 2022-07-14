using UnityEngine;
using UnityEditor;

namespace NJG.Graph
{
    [System.Serializable]
    public class ShortcutEditor
    {
        public enum FirstKey
        {
            None,
            Control,
            Alt,
            Shift,
            Command,
        }

        private string KEY_CENTER_VIEW = "centerView";
        private string KEY_SELECT_ALL = "selectAll";
        private string KEY_COPY = "copy";
        private string KEY_PASTE = "paste";
        private string KEY_DUPLICATE = "duplicate";

        // private bool IsOverBlackboard => GraphEditor.Instance.windowBlackboard.windowRect.Contains(Event.current.mousePosition);

        public void HandleKeyEvents()
        {
            if (!GraphEditor.Instance)
            {
                return;
            }
            Event ev = Event.current;
            
            if (ev.rawType == EventType.ValidateCommand)
            {
                if(ev.commandName == "SelectAll")
                {
                    // Debug.Log("Select All");
                    GraphEditor.Instance.ToggleSelection();
                    ev.Use();
                    return;
                }
            }
            else if (ev.rawType == EventType.KeyDown 
                     && !ev.command && !ev.control && !ev.alt
                     // && !GraphEditor.Instance.windowBlackboard.IsOpen()
                     // && !GraphEditor.Instance.windowBlackboard.windowRect.Contains(GraphEditor.Instance.mousePosition)
                     && ev.keyCode == KeyCode.A && !GraphEditor.Instance.MouseOverNode() && GraphEditor.SelectionCount == 0)
            {
                // Debug.LogWarningFormat("Add action: {0} commandName: {1} command: {2} control: {3}, alt: {4}", 
                //     ev.rawType, ev.commandName, ev.command, ev.control, ev.alt);
                ActionsState state = GraphUtility.AddNode<ActionsState>(GraphEditor.Instance.mousePosition, GraphEditor.Active);
                GraphUtility.UpdateNodeColor(state);
                ev.Use();
                // return;
            }
            else if (ev.rawType == EventType.KeyDown 
                                       && !ev.command && !ev.control && !ev.alt
                                       // && !GraphEditor.Instance.windowBlackboard.IsOpen()
                                       // && !GraphEditor.Instance.windowBlackboard.windowRect.Contains(GraphEditor.Instance.mousePosition)
                                       && ev.keyCode == KeyCode.T)
            {
                Node node = GraphEditor.SelectedNodes.Count > 0 ? GraphEditor.SelectedNodes[0] : GraphEditor.Instance.MouseOverNode();
                if (!node && !GraphEditor.Instance.MouseOverNode())
                {
                    // Debug.LogWarningFormat("T Over blackboard: {0}", GraphEditor.Instance.windowBlackboard.windowRect.Contains(GraphEditor.Instance.mousePosition));

                    TriggerState state = GraphUtility.AddNode<TriggerState>(GraphEditor.Instance.mousePosition, GraphEditor.Active);
                    GraphUtility.UpdateNodeColor(state);
                    // ev.Use();
                    return;
                }
                if (GraphEditor.Instance.IsValidNode(node))
                {
                    GraphEditor.Instance.fromNode = node;
                }                
            }
            
            /*if (ev != null && ev.rawType == EventType.KeyDown && ev.keyCode == KeyCode.T && !GraphEditor.Instance.windowBlackboard.IsOpen())
            {
                Node node = GraphEditor.SelectedNodes.Count > 0 ? GraphEditor.SelectedNodes[0] : GraphEditor.Instance.MouseOverNode();
                if (!node)
                {
                    return;
                }
                if (GraphEditor.Instance.IsValidNode(node))
                {
                    GraphEditor.Instance.fromNode = node;
                }                
            }*/

            
            if (ev != null && ev.rawType == EventType.KeyDown && ev.keyCode == KeyCode.Escape && GraphEditor.Instance.fromNode)
            {
                GraphEditor.Instance.fromNode = null;
            }
            
            switch (ev.type)
            {
                case EventType.KeyUp:
                    DoEvents(ev, false);
                    break;
                case EventType.MouseUp:
                    DoEvents(ev, true);
                    break;
            }
        }

        private void DoEvents(Event ev, bool isMouse)
        {
            /*if (ev != null && ev.rawType == EventType.ValidateCommand && ev.commandName == "SelectAll")
            {
                GraphEditor.Instance.ToggleSelection();
                ev.Use();
            }*/
            

            if (Validate(KEY_CENTER_VIEW, KeyCode.Tab, isMouse))
            {
                GraphEditor.Instance.CenterView();
            }

            if (Validate(KEY_SELECT_ALL, KeyCode.A, isMouse))
            {
                // GraphEditor.Instance.ToggleSelection();
                ev.Use();
            }
            /*else if (ev.keyCode == KeyCode.A && !GraphEditor.Instance.MouseOverNode())
            {
                Debug.LogWarningFormat("ev.rawType: {0} commandName: {1}", ev.rawType, ev.commandName);
                ActionsState state = GraphUtility.AddNode<ActionsState>(GraphEditor.Instance.mousePosition, GraphEditor.Active);
                GraphUtility.UpdateNodeColor(state);
                // shouldUpdate = true;
                ev.Use();
            }*/

            if (Validate(KEY_COPY, KeyCode.C, isMouse))
            {
                //Debug.LogWarningFormat("Copy SelectedNodes.Count "+GraphEditor.SelectedNodes.Count);
                if (GraphEditor.SelectedNodes.Count == 0) return;
                Pasteboard.Copy(GraphEditor.SelectedNodes, GraphEditor.Reactions);
                ev.Use();
            }
            if (Validate(KEY_PASTE, KeyCode.V, isMouse) && Pasteboard.CanPaste())
            {
                //Debug.LogWarningFormat("Paste SelectedNodes.Count "+GraphEditor.SelectedNodes.Count);

                Pasteboard.Paste(GraphEditor.Instance.mousePosition, GraphEditor.Active, GraphEditor.Reactions, false);
                ev.Use();
                GraphEditor.RepaintAll();
            }
            if (Validate(KEY_DUPLICATE, KeyCode.D, isMouse))
            {
                //Debug.LogWarningFormat("Duplicate SelectedNodes.Count "+GraphEditor.SelectedNodes.Count);
                if (GraphEditor.SelectedNodes.Count == 0) return;
                Pasteboard.Copy(GraphEditor.SelectedNodes, GraphEditor.Reactions);
                Pasteboard.Paste(GraphEditor.Instance.mousePosition, GraphEditor.Active, GraphEditor.Reactions, false);
                ev.Use();
                GraphEditor.RepaintAll();
            }
        }

        private bool Validate(string key, KeyCode defaultKey, bool isMouse)
        {
            return ControlPressed(key) && KeyPressed(key, defaultKey, isMouse);

        }

        private bool KeyPressed(string key, KeyCode defaultKey, bool isMouse)
        {
            return (Event.current.keyCode == (KeyCode)EditorPrefs.GetInt(key + "2", (int)defaultKey)) || isMouse && (KeyCode)EditorPrefs.GetInt(key + "2", (int)defaultKey) == KeyCode.Mouse0;
        }

        private bool ControlPressed(string key)
        {
            FirstKey firstKey = (FirstKey)EditorPrefs.GetInt(key + "1", (int)FirstKey.None);
            switch (firstKey)
            {
                case FirstKey.Alt:
                    return Event.current.alt;
                case FirstKey.Control:
                    return Event.current.control;
                case FirstKey.Shift:
                    return Event.current.shift;
                case FirstKey.Command:
                    return Event.current.command;
            }
            return true;
        }

    }
}
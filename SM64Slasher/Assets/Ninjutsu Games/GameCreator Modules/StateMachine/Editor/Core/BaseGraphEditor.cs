using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Rendering;

namespace NJG.Graph
{
    public class BaseGraphEditor : EditorWindow
    {
        private const float MaxCanvasSize = 50000f;
        private const float GridMinorSize = 12f;
        private const float GridMajorSize = 120f;
        public static Vector2 Center => new Vector2(MaxCanvasSize * 0.5f, MaxCanvasSize * 0.5f);

        [SerializeField]
        protected Vector2 scrollPosition;
        [SerializeField]
        protected Rect canvasSize;
        [SerializeField]
        protected float scale = 1.0f;

        private float minScale = 0.1f;
        private float maxScale = 1.0f;
        private float zoomSpeed = 50f;
        private Vector2 snapping = new Vector2(5f, 5f);
        protected Rect ScaledCanvasSize => new Rect(canvasSize.x * (maxScale / scale), canvasSize.y * (maxScale / scale), canvasSize.width * (maxScale / scale), canvasSize.height * (maxScale / scale));

        [SerializeField]
        private Rect worldViewRect;
        [SerializeField]
        private Vector2 offset;
        public Vector2 mousePosition;
        public Event currentEvent;
        protected Rect scrollView;
        private Material material;
        protected float debugProgress;
        protected List<Transition> transitionsProgress = new List<Transition>();
        protected List<Node> nodesProgress = new List<Node>();

        protected virtual void OnEnable()
        {
            scrollView = new Rect(0, 0, MaxCanvasSize, MaxCanvasSize);
            UpdateScrollPosition(Center);
        }

        protected Vector2 SnapToGrid(Vector2 input)
        {
            Vector2 pt = input;
            //if (pt.x % snapping.x < snapping.x / 2)
            //{
                pt.x = pt.x - pt.x % snapping.x;
            //}
            //else
            {
            //    pt.x = pt.x + (snapping.x - pt.x % snapping.x);
            }

            //if (pt.y % snapping.y < snapping.y / 2)
            {
                pt.y = pt.y - pt.y % snapping.y;
            }
            //else
            {
                //pt.y = pt.y + (snapping.y - pt.y % snapping.y);
            }
            // curPosition.x = (float)(Mathf.Round(curPosition.x) * gridSize);

            //float x = snapping.x * (int)Mathf.Round((float)input.x / snapping.x);
            //float y = snapping.y * (int)Mathf.Round((float)input.y / snapping.y);
            return pt;
            //return new Vector2(x, y);
            //return new Vector2(snapping.x * Mathf.Round(input.x / snapping.x), snapping.y * Mathf.Round(input.y / snapping.y));
        }

        protected void Begin()
        {
            currentEvent = Event.current;
            canvasSize = GetCanvasSize();

            DragCanvas();

            if (currentEvent.type == EventType.ScrollWheel)
            {
                var offset = (ScaledCanvasSize.size - canvasSize.size) * 0.5f;
                scale -= currentEvent.delta.y / zoomSpeed;
                scale = Mathf.Clamp(scale, minScale, maxScale);

                //var perc = 1 - (scale / maxScale);
                //var mouse = WorldToScreen(mousePosition);
                var center = (ScaledCanvasSize.size - canvasSize.size);
                //var mouseOffset = (worldViewRect.center - mouse);
                var scrollPos = (scrollPosition - (center) * 0.5f + offset);
                //var scrollPos2 = (scrollPosition - (center) * 0.5f + ScreenToWorld(currentEvent.mousePosition));

                //Debug.Log("Scroll pos: " + scrollPosition + " / center: " + center+" - "+((center) * 0.5f) + " / offset: " + offset+" / mouse: "+ mouse+ " / scale: "+ scale);

                // Debug.Log("currentEvent center " + center + " / mouse "+ ScreenToWorld(currentEvent.mousePosition)+ " / "+WorldToScreen(currentEvent.mousePosition) + " / scrollPos" + scrollPos+ 
                //     " / mouseOffset "+ mouseOffset+ " / scrollPos2 "+ scrollPos2+ " / Perc "+perc+ " / worldViewRect "+ worldViewRect.position);

                //offset += currentEvent.delta;
                UpdateScrollPosition(scrollPos);    
                //UpdateScrollPosition((scrollPosition - (scaledCanvasSize.size - canvasSize.size) * 0.5f + offset)); // + (currentEvent.delta * (maxScale / scale))
                Event.current.Use();
            }

            if (currentEvent.type == EventType.Repaint)
            {
                Styles.graphBackground.Draw(ScaledCanvasSize, false, false, false, false);
                DrawGrid();
            }

            Vector2 curScroll = GUI.BeginScrollView(ScaledCanvasSize, scrollPosition, scrollView, GUIStyle.none, GUIStyle.none);

            UpdateScrollPosition(curScroll);
            mousePosition = Event.current.mousePosition;
        }

        public Vector2 WindowToGridPosition(Vector2 windowPosition)
        {
            //new Rect(-(center.x - origNode.position.x) + position.x, -(center.y - origNode.position.y) + position.y, GraphStyles.StateWidth, GraphStyles.StateHeight)
            return (windowPosition - (worldViewRect.size * 0.5f) - (offset / scale)) * scale;
        }

        public Vector2 GridToWindowPosition(Vector2 gridPosition)
        {
            return (worldViewRect.size * 0.5f) + (offset / scale) + (gridPosition / scale);
        }

        /// <summary>
        /// Converts world coordinates (in the graph view) into Screen coordinates (relative to the editor window)
        /// </summary>
        /// <param name="worldCoord">The world cooridnates of the graph view</param>
        /// <returns>The screen cooridnates relative to the editor window</returns>
        public Vector2 WorldToScreen(Vector2 worldCoord)
        {
            //return worldCoord - position;
            return (worldCoord - Center) / scale;
        }

        /// <summary>
        /// Converts the Screen coordinates (of the editor window) into the graph's world coordinate
        /// </summary>
        /// <param name="screenCoord"></param>
        /// <returns>The world coordinates in the graph view</returns>
        public Vector2 ScreenToWorld(Vector2 screenCoord)
        {
            //return screenCoord + position;
            return (screenCoord * scale);
        }

        protected void End()
        {
            GUI.EndScrollView();
        }

        protected virtual void OnGUI()
        {

        }

        protected virtual void CanvasContextMenu()
        {

        }

        protected virtual Rect GetCanvasSize()
        {
            return new Rect(0, 0, position.width, position.height - 20);
        }

        protected void UpdateScrollPosition(Vector2 position)
        {
            offset = offset + (scrollPosition - position);
            scrollPosition = position;
            worldViewRect = new Rect(ScaledCanvasSize);
            worldViewRect.y += scrollPosition.y;
            worldViewRect.x += scrollPosition.x;
        }

        private void DragCanvas()
        {
            if (currentEvent.alt || currentEvent.button == 2)
            {
                int controlID = GUIUtility.GetControlID(FocusType.Keyboard);

                switch (currentEvent.rawType)
                {
                    case EventType.MouseDown:
                        GUIUtility.hotControl = controlID;
                        currentEvent.Use();
                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlID)
                        {
                            GUIUtility.hotControl = 0;
                            currentEvent.Use();
                        }

                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlID)
                        {
                            UpdateScrollPosition(scrollPosition - currentEvent.delta);
                            currentEvent.Use();
                        }

                        break;
                }
            }
        }

        private void DrawGrid()
        {
            //HandleUtility2.ApplyWireMaterial();
            GL.PushMatrix();
            GL.Begin(1);
            Color minor = GraphStyles.gridMinorColor;
            Color major = GraphStyles.gridMajorColor;
            if (EditorGUIUtility.isProSkin)
            {
                minor.a = Mathf.Lerp(0.0f - minScale, 0.18f, (scale / maxScale));
                major.a = Mathf.Lerp(0.16f, 0.28f, (scale / maxScale));
            }
            else
            {
                minor.a = Mathf.Lerp(0.0f - minScale, 0.1f, (scale / maxScale));
                major.a = Mathf.Lerp(0.07f, 0.15f, (scale / maxScale));
            }

            DrawGridLines(ScaledCanvasSize, GridMinorSize, offset, minor);
            DrawGridLines(ScaledCanvasSize, GridMajorSize, offset, major);
            GL.End();
            GL.PopMatrix();
        }

        private void DrawGridLines(Rect rect, float gridSize, Vector2 offset, Color gridColor)
        {
            GL.Color(gridColor);
            //Handles.color = gridColor;
            for (float i = rect.x + (offset.x < 0f ? gridSize : 0f) + offset.x % gridSize; i < rect.x + rect.width; i = i + gridSize)
            {
                DrawLine(new Vector2(i, rect.y), new Vector2(i, rect.y + rect.height));
            }
            for (float j = rect.y + (offset.y < 0f ? gridSize : 0f) + offset.y % gridSize; j < rect.y + rect.height; j = j + gridSize)
            {
                DrawLine(new Vector2(rect.x, j), new Vector2(rect.x + rect.width, j));
            }
        }

        private void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);

            /*Texture2D tex = (Texture2D)UnityEditor.Graphs.Styles.connectionTexture.image;
            tex.filterMode = FilterMode.Trilinear;
            
            Handles.DrawAAPolyLine(tex, 1.0f, new Vector3[] { p1, p2 });*/
        }

        protected void DrawTransition(Vector3 position)
        {
            //Handles.draw
        }

        protected void DrawConnection(Vector3 start, Vector3 end, Color color, int arrows, bool offset, Transition transition)
        {
            if (currentEvent.type != EventType.Repaint)
            {
                return;
            }

            Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
            if (offset)
            {
                start = start + cross * 6;
                end = end + cross * 6;
            }

            Texture2D tex = (Texture2D)Styles.connectionTexture.image;
            tex.filterMode = FilterMode.Trilinear;

            Handles.color = color;
            Handles.DrawAAPolyLine(tex, Mathf.Lerp(28f, 11f, (scale / maxScale)), start, end);

            if (transition && Application.isPlaying && transition.entered)
            {
                if (!transitionsProgress.Contains(transition))
                {
                    transition.progress = 0;
                    transitionsProgress.Add(transition);
                }
                Vector3 start2 = Vector3.Lerp(start, end, transition.progress / 100);

                Color c = Color.cyan;
                c.a = 0.5f;
                Handles.color = c;

                Handles.DrawAAPolyLine(Mathf.Lerp(46f, 15f, (scale / maxScale)), start2, end);
            }

            Vector3 vector3 = end - start;
            Vector3 vector31 = vector3.normalized;
            Vector3 vector32 = (vector3 * 0.5f) + start;
            vector32 = vector32 - (cross * 0.5f);
            Vector3 vector33 = vector32 + vector31;

            for (int i = 0; i < arrows; i++)
            {
                Vector3 center = vector33 + vector31 * 10.0f * i + vector31 * 5.0f - vector31 * arrows * 5.0f;
                //Handles.ArrowHandleCap(0, start, Quaternion.Euler(vector31), 60, currentEvent.type);
                Rect rect = new Rect(worldViewRect);
                rect.y -= canvasSize.y - 24;
                GUI.Label(rect, new GUIContent(Styles.nodeAddButton.normal.background));
                DrawArrow(transition && transition.mute ? Color.red : color, cross, vector31, center, Mathf.Lerp(14f, 6f, (scale / maxScale)));
            }
        }

        private void DrawArrow(Color color, Vector3 cross, Vector3 direction, Vector3 center, float size)
        {
            Rect rect = new Rect(worldViewRect);
            rect.y -= canvasSize.y - size;
            if (!rect.Contains(center))
            {
                return;
            }
            Vector3[] vector3Array = {
                center + (direction * size),
                (center - (direction * size)) + (cross * size),
                (center - (direction * size)) - (cross * size),
                center + (direction * size)
            };

            /*Color color1 = color;
            color1.r *= 0.8f;
            color1.g *= 0.8f;
            color1.b *= 0.8f;*/
             
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vector3Array);
        }
    }

    static class HandleUtility2
    {
        static Material s_HandleWireMaterial;
        static Material s_HandleWireMaterial2D;

        internal static void ApplyWireMaterial(CompareFunction zTest = CompareFunction.Always)
        {
            Material handleWireMaterial = HandleUtility2.handleWireMaterial;
            handleWireMaterial.SetInt("_HandleZTest", (int)zTest);
            handleWireMaterial.SetPass(0);
        }

        private static Material handleWireMaterial
        {
            get
            {
                InitHandleMaterials();
                return (!Camera.current) ? s_HandleWireMaterial2D : s_HandleWireMaterial;
            }
        }

        private static void InitHandleMaterials()
        {
            if (!s_HandleWireMaterial)
            {
                s_HandleWireMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
                s_HandleWireMaterial2D = (Material)EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");
            }
        }
    }
}
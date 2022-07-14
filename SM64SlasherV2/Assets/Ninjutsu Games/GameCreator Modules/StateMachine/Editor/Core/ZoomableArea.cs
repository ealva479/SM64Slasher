using System;
using UnityEngine;

namespace NJG
{
    public class ZoomableArea
    {
        private static Matrix4x4 prevGuiMatrix;
        private static float kEditorWindowTabHeight;
        private static float kEditorWindowTabWith;

        static ZoomableArea()
        {
            kEditorWindowTabHeight = 22f;
            kEditorWindowTabWith = 0;
        }

        public static void Begin(Rect screenCoordsArea, float zoomScale, bool docked)
        {
            GUI.EndGroup();
            kEditorWindowTabHeight = (docked ? 19f : 22f);
            kEditorWindowTabWith = (docked ? 5 : 0);
            Rect rect = screenCoordsArea.ScaleSizeBy(1f / zoomScale, screenCoordsArea.TopLeft());
            rect.y = rect.y + kEditorWindowTabHeight;
            GUI.BeginGroup(rect);
            prevGuiMatrix = GUI.matrix;
            Matrix4x4 matrix4x4 = Matrix4x4.TRS(rect.TopLeft(), Quaternion.identity, Vector3.one);
            Vector3 vector3 = Vector3.one;
            float single = zoomScale;
            float single1 = single;
            vector3.y = single;
            vector3.x = single1;
            Matrix4x4 matrix4x41 = Matrix4x4.Scale(vector3);
            GUI.matrix = ((matrix4x4 * matrix4x41) * matrix4x4.inverse) * GUI.matrix;
        }

        public static void End()
        {
            GUI.matrix = prevGuiMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(kEditorWindowTabWith, kEditorWindowTabHeight, (float)Screen.width, (float)Screen.height));
        }
    }
}
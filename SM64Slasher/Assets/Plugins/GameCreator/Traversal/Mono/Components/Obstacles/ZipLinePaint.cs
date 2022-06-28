using System;
using UnityEngine;

namespace GameCreator.Traversal
{
    public class ZipLinePaint : MonoBehaviour
    {
        public Transform pointA;
        public Transform pointB;

        [Space] 
        public float lineWidth = 0.02f;
        public Material lineMaterial;

        private LineRenderer line;
        
        private void Awake()
        {
            this.line = gameObject.GetComponent<LineRenderer>();
            if (this.line == null)
            {
                this.line = gameObject.AddComponent<LineRenderer>();
                
                this.line.startWidth = this.lineWidth;
                this.line.endWidth = this.lineWidth;
            
                this.line.material = this.lineMaterial;
                this.line.numCapVertices = 5;
            }

            this.line.positionCount = 2;

            this.UpdatePosition();
        }

        private void Update()
        {
            this.UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (this.pointA == null) return;
            if (this.pointB == null) return;
            
            this.line.SetPosition(0, this.pointA.position);
            this.line.SetPosition(1, this.pointB.position);
        }

        private void OnDrawGizmos()
        {
            if (this.pointA == null) return;
            if (this.pointB == null) return;
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(this.pointA.position, this.pointB.position);
        }
    }
}
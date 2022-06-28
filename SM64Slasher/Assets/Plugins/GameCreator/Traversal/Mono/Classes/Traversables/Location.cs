namespace GameCreator.Traversal
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Location
    {
        public enum Type
        {
            Position,
            Transform
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        [SerializeField] private Type type = Type.Position;
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private Transform transform = null;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector3 Get(Transform transform)
        {
            if (this.type == Type.Transform) return this.transform == null 
                ? transform.position 
                : this.transform.position;
            
            return transform.TransformPoint(this.position);
        }
    }   
}

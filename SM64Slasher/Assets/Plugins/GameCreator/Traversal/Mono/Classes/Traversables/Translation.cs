namespace GameCreator.Traversal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class Translation : ICloneable
    {
        private enum Mode
        {
            BySpeed = 0,
            ByDuration = 1
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        [SerializeField] private Mode mode = Mode.ByDuration;
        [SerializeField] private float speed = 4f;
        [SerializeField] private float duration = 1f;
        
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public Translation()
        { }

        public static Translation BySpeed(float speed)
        {
            return new Translation
            {
                mode = Mode.BySpeed,
                speed = speed
            };
        }
        
        public static Translation ByDuration(float duration)
        {
            return new Translation
            {
                mode = Mode.ByDuration,
                duration = duration
            };
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float Get(Vector3 origin, Vector3 target)
        {
            float distance = Vector3.Distance(origin, target);
            Debug.DrawLine(origin, target, Color.cyan, 10f);
            
            switch (this.mode)
            {
                case Mode.BySpeed: 
                    return distance / this.speed;
                
                case Mode.ByDuration: 
                    return distance <= 0.1f ? 0f : this.duration;
                
                default: return 0;
            }
        }

        public object Clone()
        {
            return new Translation
            {
                mode = this.mode,
                duration = this.duration,
                speed = this.speed
            };
        }
    }   
}

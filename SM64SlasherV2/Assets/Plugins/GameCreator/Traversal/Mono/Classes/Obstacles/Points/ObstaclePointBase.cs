namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    public abstract class ObstaclePointBase
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        [Range(0f, 360f)] 
        public float angle;
        
        public TraversableComponent commuteTo;
            
        public Translation transition = Translation.ByDuration(0.25f);
        public Easing.EaseType easing = Easing.EaseType.Linear;
        public float restDuration = 0f;
    }     
}

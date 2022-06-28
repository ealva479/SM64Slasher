namespace GameCreator.Traversal
{
    using System;
    using UnityEngine;

    [Serializable]
    public class ObstaclePoint : ObstaclePointBase
    {
        public Location location = new Location();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public ObstacleStep Convert(TraversalCharacter traverser, Obstacle obstacle)
        {
            Quaternion rotation = obstacle.transform.rotation * Quaternion.Euler(0f, this.angle, 0f);
            Quaternion handlesRotation =
                rotation * Quaternion.Inverse(traverser.transform.rotation);
            
            Vector3 position = this.location.Get(obstacle.transform) + (traverser != null
                ? handlesRotation * traverser.GetHandlesOffset(obstacle)
                : Vector3.zero
            );
            
            ObstacleStep step = new ObstacleStep
            {
                position = position,
                angle = rotation.eulerAngles.y,
                commuteTo = this.commuteTo,
                transition =  this.transition.Clone() as Translation,
                easing = this.easing,
                restDuration = this.restDuration
            };

            return step;
        }

        public Vector3 GetPosition(Transform transform)
        {
            return this.location.Get(transform);
        }
        
        // public static ObstacleStep Convert(ObstaclePoint source, Obstacle obstacle)
        // {
        //     Quaternion rotation = obstacle.transform.rotation * Quaternion.Euler(0f, source.angle, 0f);
        //     Vector3 position = source.location.Get(obstacle)
        //     
        //     ObstacleStep step = new ObstacleStep
        //     {
        //         position = position,
        //         angle = rotation.eulerAngles.y,
        //         commuteTo = source.commuteTo,
        //         transition =  source.transition.Clone() as Translation,
        //         easing = source.easing,
        //         restDuration = source.restDuration
        //     };
        //
        //     return step;
        // }
    }   
}

using UnityEngine;

namespace GameCreator.Traversal
{
    public class ObstacleStep : ObstaclePointBase
    {
        public Vector3 position;

        public ObstacleStep()
        { }
        
        public ObstacleStep(Vector3 point)
        {
            this.position = point;
        }
    }
}
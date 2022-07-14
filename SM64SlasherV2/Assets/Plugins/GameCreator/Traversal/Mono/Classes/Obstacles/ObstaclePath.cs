namespace GameCreator.Traversal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [Serializable]
    public class ObstaclePath
    {
        public enum Path
        {
            ToFirstPoint,
            ToLastPoint,
            ToClosestEdge,
            ToFarthestEdge,
            FromFirstToLastPoint,
            FromLastToFirstPoint,
            FromClosestToFarthestEdge,
            FromFarthestToClosestEdge
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        [SerializeField] private Path mode = Path.FromClosestToFarthestEdge;
        [SerializeField] private ObstaclePoint[] path = { new ObstaclePoint() };

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public ObstacleStep[] GetPathSteps(TraversalCharacter traverser, Obstacle obstacle)
        {
            Vector3 characterPosition = traverser.transform.position;

            ObstacleStep pointFirst = this.path.Length > 0
                ? this.path[0].Convert(traverser, obstacle)
                : new ObstacleStep();
            
            ObstacleStep pointLast = this.path.Length > 0 
                ? this.path[this.path.Length - 1].Convert(traverser, obstacle) 
                : new ObstacleStep();

            float distanceToFirstPoint = Vector3.Distance(characterPosition, pointFirst.position);
            float distanceToLastPoint = Vector3.Distance(characterPosition, pointLast.position);

            List<ObstacleStep> pathPoints = new List<ObstacleStep>
            {
                new ObstacleStep(characterPosition)
            };

            switch (this.mode)
            {
                case Path.ToFirstPoint:
                    pathPoints.Add(pointFirst);
                    break;
                
                case Path.ToLastPoint:
                    pathPoints.Add(pointLast);
                    break;
                
                case Path.ToClosestEdge:
                    pathPoints.Add(distanceToFirstPoint < distanceToLastPoint
                        ? pointFirst
                        : pointLast
                    );
                    break;
                
                case Path.ToFarthestEdge:
                    pathPoints.Add(distanceToFirstPoint > distanceToLastPoint
                        ? pointFirst
                        : pointLast
                    );
                    break;
                
                case Path.FromFirstToLastPoint:
                    pathPoints = this.FillPointsForward(pathPoints, traverser, obstacle);
                    break;
                
                case Path.FromLastToFirstPoint:
                    pathPoints = this.FillPointsReverse(pathPoints, traverser, obstacle);
                    break;
                
                case Path.FromClosestToFarthestEdge:
                    pathPoints = distanceToFirstPoint < distanceToLastPoint
                        ? this.FillPointsForward(pathPoints, traverser, obstacle)
                        : this.FillPointsReverse(pathPoints, traverser, obstacle);
                    break;
                
                case Path.FromFarthestToClosestEdge:
                    pathPoints = distanceToFirstPoint > distanceToLastPoint
                        ? this.FillPointsForward(pathPoints, traverser, obstacle)
                        : this.FillPointsReverse(pathPoints, traverser, obstacle);
                    break;
            }

            return pathPoints.ToArray();
        }

        public float GetDuration(ObstacleStep[] steps)
        {
            float duration = 0f;
            for (int i = 1; i < steps.Length; i++)
            {
                duration += steps[i].transition.Get(
                    steps[i - 1].position,
                    steps[i - 0].position
                );

                duration += steps[i].restDuration;
            }

            return duration;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private List<ObstacleStep> FillPointsForward(List<ObstacleStep> pathPoints,
            TraversalCharacter traverser,  Obstacle obstacle)
        {
            for (int i = 0; i < this.path.Length; ++i)
            {
                ObstacleStep point = this.path[i].Convert(traverser, obstacle);
                pathPoints.Add(point);
            }

            return pathPoints;
        }
        
        private List<ObstacleStep> FillPointsReverse(List<ObstacleStep> pathPoints,
            TraversalCharacter traverser,  Obstacle obstacle)
        {
            for (int i = this.path.Length - 1; i >= 0; --i)
            {
                ObstacleStep point = this.path[i].Convert(traverser, obstacle);
                pathPoints.Add(point);
            }
            
            return pathPoints;
        }
        
        // GIZMOS: --------------------------------------------------------------------------------

        public void OnDrawGizmos(Transform transform)
        {
            for (int i = 0; i < this.path.Length; ++i)
            {
                Vector3 currPoint = this.path[i].GetPosition(transform);
                
                if (i == 0)
                {
                    Gizmos.color = Color.blue;
                }
                else
                {
                    Vector3 prevPoint = this.path[i - 1].GetPosition(transform);
                    
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(currPoint, prevPoint);

                    if (i == this.path.Length - 1) Gizmos.color = Color.red;
                }
                
                Gizmos.DrawCube(currPoint, Vector3.one * 0.1f);
            }
        }
    }
}

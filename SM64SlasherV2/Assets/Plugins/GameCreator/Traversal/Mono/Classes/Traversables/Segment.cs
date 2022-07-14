using UnityEngine;

namespace GameCreator.Traversal
{
    public struct Segment
    {
        public Vector3 pointA;
        public Vector3 pointB;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public float Length => Vector3.Distance(this.pointA, this.pointB);
        
        public Vector3 Direction => (this.pointB - this.pointA).normalized;
        
        // PUBLIC STATIC METHODS: -----------------------------------------------------------------
        
        public static Vector3 ClampPoint(Vector3 point, Segment segment)
        {
            return ClampProjection(ProjectPoint(point, segment), segment);
        }
        
        public static Vector3 ProjectPoint(Vector3 point, Segment segment)
        {
            return segment.pointA + Vector3.Project(
                point - segment.pointA, 
                segment.Direction
            );
        }
        
        public static Vector3 ClampProjection(Vector3 point, Segment segment)
        {
            var toStart = (point - segment.pointA).sqrMagnitude;
            var toEnd = (point - segment.pointB).sqrMagnitude;
            float segmentMagnitude = (segment.pointA - segment.pointB).sqrMagnitude;
            if (toStart > segmentMagnitude || toEnd > segmentMagnitude) return toStart > toEnd 
                ? segment.pointB 
                : segment.pointA;
            
            return point;
        }

        public static bool IsSegmentNearSegment(Segment segment1, Segment segment2, float maxDistance)
        {
            float partitions = segment1.Length / (maxDistance * 0.25f);
            bool firstIteration = true;
            
            for (float i = 0f; i <= partitions || firstIteration; i += 1f)
            {
                float t = partitions <= 0.01f ? 0f : i / partitions;
                Vector3 point1 = Vector3.Lerp(segment1.pointA, segment1.pointB, t);
                Vector3 point2 = NearestSegmentPoint(segment2.pointA, segment2.pointB, point1);

                float distance = Vector3.Distance(point1, point2);

                if (distance <= maxDistance) return true;
                firstIteration = false;
            }

            return false;
        }
        
        public static Vector3 NearestSegmentPoint(Vector3 pointA, Vector3 pointB, Vector3 point)
        {
            var direction = pointB - pointA;
            
            var length = direction.magnitude;
            if (length < 0.01f) return pointA;
            
            direction.Normalize();
   
            var directionA = point - pointA;
            var dotProductA = Vector3.Dot(directionA, direction);
            
            dotProductA = Mathf.Clamp(dotProductA, 0f, length);
            return pointA + direction * dotProductA;
        }
        
        public static Vector3 GetClosestClimbablePoint(Vector3 pointP, Climbable climbable)
        {
            Vector3 climablePointA = climbable.path.GetPointA(climbable);
            Vector3 climablePointB = climbable.path.GetPointB(climbable);

            return NearestSegmentPoint(climablePointA, climablePointB, pointP);
        }
    }
}
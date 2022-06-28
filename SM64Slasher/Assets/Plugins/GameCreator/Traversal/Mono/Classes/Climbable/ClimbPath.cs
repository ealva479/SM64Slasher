namespace GameCreator.Traversal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class ClimbPath
    {
        // PRIVATE CLASSES: -----------------------------------------------------------------------
        
        [Serializable]
        public class Point
        {
            public Vector3 position;
            public float angle;
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------

        [SerializeField] private Point pointA = new Point
        {
            position = Vector3.right * (-1),
            angle = 0f
        };
        
        [SerializeField] private Point pointB = new Point
        {
            position = Vector3.right * (+1),
            angle = 0f
        };

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector3 GetPointA(Climbable climbable)
        {
            if (climbable == null) return default;
            return climbable.transform.TransformPoint(this.pointA.position);
        }
        
        public Vector3 GetPointB(Climbable climbable)
        {
            if (climbable == null) return default;
            return climbable.transform.TransformPoint(this.pointB.position);
        }
        
        public Vector3 GetDirectionA(Climbable climbable)
        {
            if (climbable == null) return default;
            Quaternion rotationA = Quaternion.LookRotation(
                climbable.transform.TransformDirection(Vector3.forward)
            ) * Quaternion.Euler(0, this.pointA.angle, 0f);
            
            return rotationA * Vector3.forward;
        }
        
        public Vector3 GetDirectionB(Climbable climbable)
        {
            if (climbable == null) return default;
            Quaternion rotationB = Quaternion.LookRotation(
                climbable.transform.TransformDirection(Vector3.forward)
            ) * Quaternion.Euler(0, this.pointB.angle, 0f);
            
            return rotationB * Vector3.forward;
        }
        
        public Vector3 GetStartingPoint(TraversalCharacter traverser, 
            Climbable previousClimbable, Climbable nextClimbable)
        {
            Vector3 positionA = this.GetPointA(nextClimbable);
            Vector3 positionB = this.GetPointB(nextClimbable);

            Vector3 positionC = Vector3.zero;
            if (traverser != null)
            {
                positionC = traverser.transform.position - (previousClimbable != null
                    ? traverser.GetHandlesOffset(previousClimbable)
                    : Vector3.zero
                );
            }

            Vector3 direction = positionB - positionA;
            float magnitudeMax = direction.magnitude;
            direction.Normalize();
            
            Vector3 lhs = positionC - positionA;
            float dotP = Vector3.Dot(lhs, direction);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            
            Vector3 startingPoint = positionA + direction * dotP;

            return traverser != null
                ? startingPoint + traverser.GetHandlesOffset(nextClimbable)
                : startingPoint;
        }

        public Vector3 GetPathDirection(Climbable climbable)
        {
            Vector3 positionA = this.GetPointA(climbable);
            Vector3 positionB = this.GetPointB(climbable);

            return (positionB - positionA).normalized;
        }

        public Quaternion GetRotation(Vector3 source, Climbable climbable)
        {
            Vector3 positionA = this.GetPointA(climbable);
            Vector3 positionB = this.GetPointB(climbable);

            float t = 0.5f;
            if (Vector3.Distance(positionB, positionA) > 0.01f)
            {
                float magnitudeRelative = (source - positionA).magnitude;
                float magnitudePath = (positionB - positionA).magnitude;
                t = Mathf.Clamp01(magnitudeRelative / magnitudePath);   
            }

            Quaternion localRotationA = Quaternion.Euler(0f, this.pointA.angle, 0f);
            Quaternion localRotationB = Quaternion.Euler(0f, this.pointB.angle, 0f);
            
            Quaternion rotationA = climbable.transform.rotation * localRotationA;
            Quaternion rotationB = climbable.transform.rotation * localRotationB;

            return Quaternion.Lerp(rotationA, rotationB, t);
        }

        // GIZMOS: --------------------------------------------------------------------------------

        public void OnDrawGizmos(Climbable climbable)
        {
            Gizmos.color = Color.yellow;

            Vector3 positionA = climbable.transform.TransformPoint(this.pointA.position);
            Vector3 positionB = climbable.transform.TransformPoint(this.pointB.position);
            
            Gizmos.DrawLine(positionA, positionB);
            
            Gizmos.DrawCube(positionA, Vector3.one * 0.1f);
            Gizmos.DrawCube(positionB, Vector3.one * 0.1f);
        }
    }   
    
}

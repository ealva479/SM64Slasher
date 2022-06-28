using GameCreator.Core;

namespace GameCreator.Traversal
{
    using UnityEngine;

    public class RopeSimulator
    {
        private readonly bool affectZ;
        
        private float periodX;
        private float periodY;
        private float periodZ;
        
        private float amplitudeX;
        private float amplitudeY;
        private float amplitudeZ;

        public RopeSimulator(bool affectZ)
        {
            this.affectZ = affectZ;
        }

        public void Regenerate(Vector2 period, Vector2 amplitude)
        {
            this.periodX = Random.Range(period.x, period.y);
            this.periodY = Random.Range(period.x, period.y);
            this.periodZ = this.affectZ ? Random.Range(period.x, period.y) : 0f;

            this.amplitudeX = Random.Range(amplitude.x, amplitude.y);
            this.amplitudeY = Random.Range(amplitude.x, amplitude.y);
            this.amplitudeZ = Random.Range(amplitude.x, amplitude.y);
        }

        public Vector3 GetThrowPhase(Vector3 pointA, Vector3 pointB, float section, float t)
        {
            Vector3 direction = (pointB - pointA).normalized;
            Vector3 axisA;

            if (Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > float.Epsilon)
            {
                axisA = Vector3.Cross(direction, Vector3.up);
            }
            else
            {
                axisA = Vector3.Cross(direction, Vector3.right);
            }
            
            Vector3 axisB = Vector3.Cross(direction, axisA);
            Vector3 axisC = direction;

            Vector3 pointANew = (
                pointA +
                axisA * (Mathf.Sin(section * this.periodX) * this.amplitudeX * (1f - t)) +
                axisB * (Mathf.Sin(section * this.periodY) * this.amplitudeY * (1f - t)) +
                axisC * (Mathf.Sin(section * this.periodZ) * this.amplitudeZ * (1f - t))
            );
            
            pointANew = Vector3.Lerp(pointANew, pointA, t * 2f);
            return Vector3.Lerp(pointANew, pointB, section * t * 4f);
        }
        
        public Vector3 GetReelPhase(Vector3 pointA, Vector3 pointB, float section, float t)
        {
            Vector3 direction = (pointB - pointA).normalized;
            Vector3 axisA = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > float.Epsilon
                ? Vector3.Cross(direction, Vector3.up)
                : Vector3.Cross(direction, Vector3.right);

            Vector3 axisB = Vector3.Cross(direction, axisA);
            Vector3 axisC = direction;

            float cutout = section * (1f - t);

            Vector3 pointANew = (
                pointA +
                axisA * (Mathf.Sin(cutout * this.periodX) * this.amplitudeX) +
                axisB * (Mathf.Sin(cutout * this.periodY + Mathf.Cos(t * 5f)) * this.amplitudeY) +
                axisC * (Mathf.Sin(cutout * this.periodZ) * this.amplitudeZ)
            );

            pointANew = Vector3.Lerp(pointA, pointANew, t);
            return Vector3.Lerp(pointANew, pointB, section * (1f - t));
        }
    }   
}

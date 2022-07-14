namespace GameCreator.Traversal
{
    using System;
    using System.Collections;
    using UnityEngine;

    public class GrappleHook : Obstacle
    {
        public enum Anchor
        {
            Root = -1,
            LeftHand = HumanBodyBones.LeftHand,
            RightHand = HumanBodyBones.RightHand,
            LeftArm = HumanBodyBones.LeftLowerArm,
            RightArm = HumanBodyBones.RightLowerArm,
            Hips = HumanBodyBones.Hips,
            Head = HumanBodyBones.Head
        }

        public Anchor anchor = Anchor.LeftHand; 
        public Material grappleHookMaterial;
        public LineTextureMode grappleHookTextureMode = LineTextureMode.Tile;
        public float grappleHookWidth = 0.1f;
        public int grappleHookResolution = 20;
        
        public float grappleHookThrowDelay = 0.2f;
        public float grappleHookThrowDuration = 1f;
        public Vector2 grappleHookThrowPeriod = new Vector2(5f, 10f);
        public Vector2 grappleHookThrowAmplitude = new Vector2(0.25f, 1f);
        
        public float grappleHookReelDelay = 0.2f;
        public float grappleHookReelDuration = 1f;
        public Vector2 grappleHookReelPeriod = new Vector2(20f, 25f);
        public Vector2 grappleHookReelAmplitude = new Vector2(2f, 4f);

        private LineRenderer line;

        private RopeSimulator throwPhase;
        private RopeSimulator reelPhase;

        // INITIALIZERS: --------------------------------------------------------------------------
        
        private void Awake()
        {
            this.line = gameObject.AddComponent<LineRenderer>();
            this.line.material = this.grappleHookMaterial;
            this.line.useWorldSpace = true;
            this.line.textureMode = this.grappleHookTextureMode;
            this.line.startWidth = this.grappleHookWidth;
            this.line.endWidth = this.grappleHookWidth;
            this.line.positionCount = this.grappleHookResolution;
            this.line.enabled = false;

            this.throwPhase = new RopeSimulator(false);
            this.reelPhase = new RopeSimulator(true);
            
            this.EventStart += this.OnGrappleHookStart;
        }

        // CALLBACK METHODS: ----------------------------------------------------------------------
        
        private void OnGrappleHookStart(TraversalCharacter traverser, ObstacleStep[] points)
        {
            this.StartCoroutine(this.Animate(traverser, points));
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator Animate(TraversalCharacter traverser, ObstacleStep[] points)
        {
            this.throwPhase.Regenerate(this.grappleHookThrowPeriod, this.grappleHookThrowAmplitude);
            this.reelPhase.Regenerate(this.grappleHookReelPeriod, this.grappleHookReelAmplitude);
            
            Animator animator = traverser.Character.GetCharacterAnimator().animator;
            Transform origin = this.anchor == Anchor.Root
                ? traverser.transform
                : animator.GetBoneTransform((HumanBodyBones)this.anchor);
            
            if (this.grappleHookThrowDelay > 0f)
            {
                WaitForSeconds waitForThrowDelay = new WaitForSeconds(this.grappleHookThrowDelay);
                yield return waitForThrowDelay;
            }
            
            float throwStartTime = Time.time;
            this.line.enabled = true;
            
            WaitUntil waitThrow = new WaitUntil(() =>
            {
                float t = (Time.time - throwStartTime) / this.grappleHookThrowDuration;
                
                Vector3 startPosition = origin.position;
                Vector3 endPosition = points[points.Length - 1].position;
                
                for (int i = 0; i < this.line.positionCount; ++i)
                {
                    float section = (float) i / (this.line.positionCount - 1);

                    Vector3 point = this.throwPhase.GetThrowPhase(
                        startPosition, 
                        endPosition, 
                        section, t
                    );

                    this.line.SetPosition(i, point);
                }

                return t >= 1f;
            });

            yield return waitThrow;

            if (this.grappleHookReelDelay > 0f)
            {
                WaitForSeconds waitForReelDelay = new WaitForSeconds(this.grappleHookReelDelay);
                yield return waitForReelDelay;
            }
            
            float reelStartTime = Time.time;
            
            WaitUntil waitReel = new WaitUntil(() =>
            {
                float t = (Time.time - reelStartTime) / this.grappleHookReelDuration;
                
                Vector3 startPosition = origin.position;
                Vector3 endPosition = points[points.Length - 1].position;
                
                for (int i = 0; i < this.line.positionCount; ++i)
                {
                    float section = (float) i / (this.line.positionCount - 1);
                    Vector3 point = this.reelPhase.GetReelPhase(
                        startPosition, endPosition,
                        section, t
                    );

                    this.line.SetPosition(i, point);
                }

                return t >= 1f;
            });

            yield return waitReel;
            this.line.enabled = false;
        }
    }   
}

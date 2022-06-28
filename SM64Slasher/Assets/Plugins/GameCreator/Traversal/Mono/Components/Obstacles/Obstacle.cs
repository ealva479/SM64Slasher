using System.Collections.Generic;
using UnityEngine.Serialization;

namespace GameCreator.Traversal
{
    using System;
    using System.Collections;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class Obstacle : TraversableComponent
    {
        private static readonly Vector3 PLANE = new Vector3(1, 0, 1);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public float delayIn;
        public ObstaclePath obstaclePath = new ObstaclePath();

        public ObstacleClip clip;

        // EVENTS: --------------------------------------------------------------------------------

        public event Action<TraversalCharacter, ObstacleStep[]> EventStart;
        public event Action<TraversalCharacter, ObstacleStep[]> EventComplete;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override IEnumerator OnActivate(TraversalCharacter traverser, Climbable from)
        {
            if (this.clip == null) yield break;
            if (traverser == null) yield break;
            if (traverser.Character.characterLocomotion.isBusy) yield break;

            bool wasUsingFootIK = traverser.Animator.useFootIK;
            bool wasUsingHeadIK = traverser.Animator.useSmartHeadIK;
            
            traverser.Animator.useFootIK = false;
            traverser.Animator.useSmartHeadIK = false;
            
            traverser.Character.characterLocomotion.SetDirectionalDirection(
                traverser.transform.TransformDirection(Vector3.forward)
            );
        
            CharacterAnimator animator = traverser.Character.GetCharacterAnimator();
            CharacterLocomotion locomotion = traverser.Character.characterLocomotion;
            CharacterController controller = locomotion.characterController;
        
            bool wasControllable = locomotion.isControllable;
            this.Users.Add(traverser, wasControllable);
            
            locomotion.SetIsControllable(false);
            locomotion.isBusy = true;
            controller.detectCollisions = !this.clip.ignoreCollisions;

            ObstacleStep[] steps = this.obstaclePath.GetPathSteps(traverser, this);
            float totalDuration = this.obstaclePath.GetDuration(steps);

            switch (this.clip.animation)
            {
                case ObstacleClip.Animation.CharacterState:
                    if (this.clip.characterState != null)
                    {
                        animator.SetState(
                            this.clip.characterState, null, 
                            1f, this.clip.transitionIn, 1f, 
                            CharacterAnimation.Layer.Layer3
                        );   
                    }
                    break;
                
                case ObstacleClip.Animation.AnimationClip:
                    if (this.clip.animationClip != null)
                    {
                        float length = (
                            this.delayIn +
                            totalDuration + 
                            this.clip.transitionOut
                        );
                
                        animator.CrossFadeGesture(
                            this.clip.animationClip, 
                            this.clip.animationClip.length / length,
                            null,
                            this.clip.transitionIn,
                            this.clip.transitionOut
                        );
                    }
                    break;
                
                case ObstacleClip.Animation.RandomAnimationClip:
                    int randomIndex = UnityEngine.Random.Range(0, this.clip.randomAnimationClips.Length);
                    AnimationClip randomAnimationClip = this.clip.randomAnimationClips[randomIndex];
                    if (randomAnimationClip != null)
                    {
                        float length = (
                            this.delayIn +
                            totalDuration + 
                            this.clip.transitionOut
                        );
                
                        animator.CrossFadeGesture(
                            randomAnimationClip, 
                            randomAnimationClip.length / length,
                            null,
                            this.clip.transitionIn,
                            this.clip.transitionOut
                        );
                    }
                    break;
            }

            this.EventStart?.Invoke(traverser, steps);
            
            Quaternion startRotation = traverser.transform.rotation;
            Vector3 direction = this.clip.GetDirection(traverser, steps[steps.Length - 1]);
            
            float startTime = Time.time;
            
            this.StartCoroutine(new WaitUntil(() =>
            {
                if (direction == Vector3.zero) return true;
                
                float time = Time.time - startTime;
                float rotationT = time / Mathf.Min(totalDuration, this.clip.rotationDuration);

                traverser.transform.rotation = Quaternion.Slerp(
                    startRotation,
                    Quaternion.LookRotation(Vector3.Scale(direction, PLANE)),
                    rotationT
                );

                return rotationT >= 1f;
            }));

            if (this.delayIn > 0)
            {
                WaitUntil transitionIn = new WaitUntil(() =>
                    Time.time - startTime > this.delayIn
                );

                yield return transitionIn;
            }

            locomotion.UseGravity(false);
            steps[0].position = traverser.transform.position;
            
            for (int i = 1; i < steps.Length; ++i)
            {
                Vector3 pointA = steps[i - 1].position;
                Vector3 pointB = steps[i - 0].position;

                ObstacleStep currentPoint = steps[i];

                float duration = steps[i].transition.Get(pointA, pointB);
                float pathStartTime = Time.time;
                
                WaitUntil translation = new WaitUntil(() =>
                {
                    float elapsed = Time.time - pathStartTime;
                    float translationT = duration > 0f
                        ? Mathf.Clamp01(elapsed / duration)
                        : 1f;
                    
                    float translationTEase = Easing.GetEase(currentPoint.easing, 0f, 1f, translationT);
                    Vector3 position = Vector3.Lerp(pointA, pointB, translationTEase);

                    position += transform.TransformDirection(new Vector3(
                        this.clip.curveOffsetX.Evaluate(translationT),
                        this.clip.curveOffsetY.Evaluate(translationT),
                        this.clip.curveOffsetZ.Evaluate(translationT)
                    ));
                    
                    traverser.transform.position = position;

                    if (translationT >= 1f)
                    {
                        return elapsed >= duration + currentPoint.restDuration;
                    }

                    return false;
                });

                yield return translation;
            }

            if (direction != Vector3.zero && traverser.Character is PlayerCharacter player)
            {
                float exitSpeed = Vector3
                    .Project(this.clip.GetExitVelocity(traverser), direction)
                    .magnitude;
                
                Vector3 moveDirection = Quaternion.Euler(
                    0f, -HookCamera.Instance.transform.rotation.eulerAngles.y, 0f
                ) * direction;
                
                moveDirection.Scale(PLANE);
                moveDirection.Normalize();
                
                player.ForceDirection(moveDirection * exitSpeed);
                locomotion.SetDirectionalDirection(moveDirection);
            }

            traverser.Animator.useFootIK = wasUsingFootIK;
            traverser.Animator.useSmartHeadIK = wasUsingHeadIK;
            
            TraversableComponent commuter = steps[steps.Length - 1].commuteTo;
            if (commuter != null) yield return traverser.SetActive(commuter);
            else yield return traverser.SetActive(null);
        }

        public override void OnDeactivate(TraversalCharacter traverser)
        {
            bool wasControllable = !this.Users.ContainsKey(traverser) || this.Users[traverser];
            base.OnDeactivate(traverser);

            CharacterAnimator animator = traverser.Animator;
            CharacterLocomotion locomotion = traverser.Character.characterLocomotion;
            CharacterController controller = locomotion.characterController;
            
            controller.detectCollisions = true;
            locomotion.isBusy = false;
            locomotion.SetIsControllable(wasControllable);
            locomotion.UseGravity(true);
            
            if (this.clip.animation == ObstacleClip.Animation.CharacterState)
            {
                if (this.clip.characterState != null)
                {
                    animator.ResetState(
                        this.clip.transitionOut, 
                        CharacterAnimation.Layer.Layer3
                    );
                }
            }
            
            this.EventComplete?.Invoke(traverser, null);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector3 GetStartPoint(TraversalCharacter traverser, Climbable active)
        { 
            ObstacleStep[] points = this.obstaclePath.GetPathSteps(traverser, this);
            return points[1].position;
        }
        
        public override float GetReachDuration(TraversalCharacter traverser, Climbable active)
        {
            ObstacleStep[] points = this.obstaclePath.GetPathSteps(traverser, this);
            float duration = this.obstaclePath.GetDuration(points);

            return duration + this.delayIn;
        }
        
        // AUTO CONNECTIONS: ----------------------------------------------------------------------

        public void GenerateAutoConnections()
        { }

        public override List<TraversableComponent> GetAutoConnectionsElements()
        {
            return new List<TraversableComponent>();
        }

        // GIZMOS: --------------------------------------------------------------------------------
        
        private void OnDrawGizmos()
        {
            this.obstaclePath.OnDrawGizmos(this.transform);
        }
    }   
}

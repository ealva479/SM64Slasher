using System;
using System.Collections.Generic;
using GameCreator.Core.Hooks;

namespace GameCreator.Traversal
{
    using System.Collections;
    using UnityEngine;
    using GameCreator.Characters;

    public class Climbable : TraversableComponent
    {
        private const float TRANSITION_GRAB = 0.1f;
        
        private static readonly int ANIM_MOVE = Animator.StringToHash("Move");
        private static readonly int ANIM_MOVE_X = Animator.StringToHash("MoveX");
        private static readonly int ANIM_MOVE_Y = Animator.StringToHash("MoveY");

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public ClimbPath path = new ClimbPath();
        public ClimbClip clip;
        public bool canDrop = true;

        [Header("Commute Traversables")]
        public TraversableComponent commuteToA;
        public TraversableComponent commuteToB;

        [Header("Within Reach")] 
        public TraversableComponent[] reachables = new TraversableComponent[0];

        private Dictionary<int, bool> wasUsingFootIK = new Dictionary<int, bool>();
        private Dictionary<int, bool> wasUsingHeadIK = new Dictionary<int, bool>();
        
        // EVENTS: --------------------------------------------------------------------------------

        public event Action<TraversalCharacter> EventGrab;
        public event Action<TraversalCharacter> EventLeave;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void OnUpdate(TraversalCharacter traverser)
        {
            Vector3 position = traverser.transform.position;
            Vector3 handles = position - traverser.GetHandlesOffset(this);
            
            Vector3 direction = this.path.GetPathDirection(this);

            Vector3 pointA = this.path.GetPointA(this);
            Vector3 pointB = this.path.GetPointB(this);

            float magnitude = (pointA - pointB).magnitude;
            float magnitudeAtoChar = (handles - pointA).magnitude;
            float magnitudeBtoChar = (handles - pointB).magnitude;

            if (this.canDrop && Input.GetKey(this.clip.inputKeyDrop))
            {
                StartCoroutine(traverser.SetActive(null));
                return;
            }

            Vector3 localDirectionTraverse = Vector3.zero;
            switch (this.clip.orientation)
            {
                case ClimbClip.Orientation.Vertical:
                    localDirectionTraverse = new Vector3(
                        Input.GetAxisRaw("Horizontal"),
                        Input.GetAxisRaw("Vertical"),
                        0f
                    );
                    break;
                case ClimbClip.Orientation.Horizontal:
                    localDirectionTraverse = new Vector3(
                        Input.GetAxisRaw("Horizontal"),
                        0f,
                        Input.GetAxisRaw("Vertical")
                    );
                    break;
            }

            Vector3 cameraDirectionTraverse = HookCamera.Instance.transform.TransformDirection(
                localDirectionTraverse
            );

            TraversableComponent chosenReachable = null;
            float maxNormalDot = -1f;
                
            foreach (TraversableComponent reachable in this.reachables)
            {
                if (reachable == null) continue;
                if (reachable == this) continue;
                if (!reachable.isActiveAndEnabled) continue;
                
                Vector3 reachableClosestPoint = reachable.GetStartPoint(traverser, this);
                Vector3 reachableDirection = reachableClosestPoint - handles;

                float normalDot = Vector3.Dot(cameraDirectionTraverse, reachableDirection.normalized);
                if (normalDot < 0.4f || maxNormalDot > normalDot || 
                    reachableDirection.magnitude >= this.clip.maxTraverseDistance)
                {
                    Debug.DrawLine(handles, reachableClosestPoint, Color.red);
                    continue;
                }
                
                Debug.DrawLine(handles, reachableClosestPoint, Color.green);
                
                maxNormalDot = normalDot;
                chosenReachable = reachable;
            }

            if (Input.GetKey(this.clip.inputKeyTraverse))
            {
                if (chosenReachable != null)
                {
                    traverser.SetIsReaching();
                    this.StartCoroutine(this.JumpTo(traverser, chosenReachable));
                    return;
                }
            }

            float remainingDistance = 0f;
            float increment = 0f;
            float sign = 0f;

            float inputDotProduct = 0f;
            switch (this.clip.inputMovement)
            {
                case ClimbClip.InputMode.Directional:
                    inputDotProduct = Vector3.Dot(cameraDirectionTraverse, direction);
                    break;
                case ClimbClip.InputMode.AutomaticForward:
                    inputDotProduct = 1f;
                    break;
                case ClimbClip.InputMode.AutomaticBackward:
                    inputDotProduct = -1f;
                    break;
            }

            if (inputDotProduct > 0.5f)
            {
                increment = this.clip.moveForwardSpeed * Time.deltaTime;
                remainingDistance = magnitude - magnitudeAtoChar;

                if (increment > magnitude - magnitudeAtoChar && this.commuteToB != null)
                {
                    this.commuteToB.StartCoroutine(
                        traverser.SetActive(this.commuteToB)
                    );
                }

                increment = Mathf.Min(increment, magnitude - magnitudeAtoChar);
                sign = 1f;
            }
            
            if (inputDotProduct < -0.5f)
            {
                increment = this.clip.moveBackwardSpeed * Time.deltaTime;
                remainingDistance = magnitude - magnitudeBtoChar;
                
                if (increment > magnitude - magnitudeBtoChar && this.commuteToA != null)
                {
                    this.commuteToA.StartCoroutine(
                        traverser.SetActive(this.commuteToA)
                    );
                }
                
                increment = Mathf.Min(increment, magnitude - magnitudeBtoChar);
                sign = -1f;
            }
            
            handles += direction * (increment * sign);
            
            Quaternion rotation = this.path.GetRotation(handles, this);
            traverser.transform.rotation = rotation;

            traverser.transform.position = handles + traverser.GetHandlesOffset(this);
            
            if (remainingDistance < 0.05f)
            {
                traverser.AnimationMovementTarget = 0f;

                Vector3 traverserFwd = traverser.transform.TransformDirection(Vector3.forward);
                Vector3 traverserUwd = traverser.transform.TransformDirection(Vector3.up);
                Vector3 traverserSds = traverser.transform.TransformDirection(Vector3.right);
                
                switch (this.clip.orientation)
                {
                    case ClimbClip.Orientation.Vertical:
                        traverser.AnimationMovementDirectionTarget = new Vector2(
                            Vector3.Dot(traverserSds, cameraDirectionTraverse),
                            Vector3.Dot(traverserUwd, cameraDirectionTraverse)
                        );
                        break;
                    case ClimbClip.Orientation.Horizontal:
                        traverser.AnimationMovementDirectionTarget = new Vector2(
                            Vector3.Dot(traverserSds, cameraDirectionTraverse),
                            Vector3.Dot(traverserFwd, cameraDirectionTraverse)
                        );
                        break;
                }
            }
            else
            {
                traverser.AnimationMovementTarget = sign > 0f ? 1f : -1f;
                traverser.AnimationMovementDirectionTarget = Vector2.zero;
            }
            
            traverser.Animator.animator.SetFloat(ANIM_MOVE, traverser.AnimationMovement);
            traverser.Animator.animator.SetFloat(ANIM_MOVE_X, traverser.AnimationMovementDirection.x);
            traverser.Animator.animator.SetFloat(ANIM_MOVE_Y, traverser.AnimationMovementDirection.y);
        }

        public override IEnumerator OnActivate(TraversalCharacter traverser, Climbable from)
        {
            if (this.clip == null) yield break;
            if (traverser == null) yield break;
            
            if (traverser.Character.characterLocomotion.isBusy) yield break;
        
            this.wasUsingFootIK[traverser.GetHashCode()] = traverser.Animator.useFootIK;
            this.wasUsingHeadIK[traverser.GetHashCode()] = traverser.Animator.useSmartHeadIK;
            
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
            locomotion.UseGravity(false);
            locomotion.SetVerticalSpeed(0f);
            locomotion.isBusy = true;
            
            if (this.clip.ignoreCollisions) controller.detectCollisions = false;

            if (this.clip.animatorController != null)
            {
                animator.SetState(
                    this.clip.animatorController, null, 1f, 0.3f, 1f,
                    CharacterAnimation.Layer.Layer3
                );
            }

            Vector3 startPosition = traverser.transform.position;
            Quaternion startRotation = traverser.transform.rotation;
            
            Vector3 targetPosition = this.path.GetStartingPoint(traverser, from, this);
            Quaternion targetRotation = this.path.GetRotation(targetPosition, this);

            float transitionDuration = from != null
                ? this.clip.reachTransition.Get(startPosition, targetPosition)
                : this.clip.transition.Get(startPosition, targetPosition);

            float startTime = Time.time;
            
            Vector3 direction = targetPosition - startPosition;
            float characterHeight = locomotion.characterController.height;
            
            float reachMagnitude = Mathf.Clamp01(direction.magnitude / characterHeight);
            Vector2 reachDirection = direction.normalized;
            
            if (transitionDuration > 0.01f)
            {
                WaitUntil transition = new WaitUntil(() =>
                {
                    float t = (Time.time - startTime) / transitionDuration;
                    Vector3 handles = Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        t
                    );

                    traverser.transform.position = handles;
                    
                    traverser.transform.rotation = Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        t
                    );
                    
                    return t >= 1f;
                });

                yield return transition;
                
                animator.CrossFadeGesture(
                    this.clip.grabController, 1f, null, TRANSITION_GRAB, TRANSITION_GRAB, true,
                    new PlayableGesture.Parameter("X", reachDirection.x * reachMagnitude),
                    new PlayableGesture.Parameter("Y", reachDirection.y * reachMagnitude)
                );
                
                float grabDuration = 0f;
                foreach (AnimationClip grabClip in this.clip.grabController.animationClips)
                {
                    grabDuration = Mathf.Max(grabDuration, grabClip.length);
                }

                this.EventGrab?.Invoke(traverser);
                
                grabDuration = Mathf.Max(0f, grabDuration - TRANSITION_GRAB);
                WaitForSeconds waitGrab = new WaitForSeconds(grabDuration);
                yield return waitGrab;
            }
        }

        public override void OnDeactivate(TraversalCharacter traverser)
        {
            base.OnDeactivate(traverser);

            this.wasUsingFootIK.TryGetValue(traverser.GetHashCode(), out bool usingFootIK);
            this.wasUsingHeadIK.TryGetValue(traverser.GetHashCode(), out bool usingHeadIK);

            traverser.Animator.useFootIK = usingFootIK;
            traverser.Animator.useSmartHeadIK = usingHeadIK;
            
            CharacterAnimator animator = traverser.Character.GetCharacterAnimator();
            CharacterLocomotion locomotion = traverser.Character.characterLocomotion;
            CharacterController controller = locomotion.characterController;
            
            locomotion.SetIsControllable(true);
            locomotion.UseGravity(true);
            locomotion.isBusy = false;
            controller.detectCollisions = true;

            if (this.clip.animatorController != null)
            {
                animator.ResetState(0.25f, CharacterAnimation.Layer.Layer3);
            }
            
            this.EventLeave?.Invoke(traverser);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector3 GetStartPoint(TraversalCharacter traverser, Climbable active)
        {
            Vector3 startPoint = this.path.GetStartingPoint(traverser, active, this);
            if (traverser != null) startPoint -= traverser.GetHandlesOffset(this);

            return startPoint;
        }

        public override float GetReachDuration(TraversalCharacter traverser, Climbable active)
        {
            Vector3 destinationHandles = this.path.GetStartingPoint(traverser, active, this);
            Vector3 characterHandles = traverser.transform.position - (active != null
                ? traverser.GetHandlesOffset(active)
                : Vector3.zero
            );

            return this.clip.reachTransition.Get(characterHandles, destinationHandles);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator JumpTo(TraversalCharacter traverser, TraversableComponent reachable)
        {
            float duration = (
                reachable.GetReachDuration(traverser, this) + 
                this.clip.reachStartDelay + this.clip.reachEndDelay
            );

            Vector3 direction = (
                reachable.GetStartPoint(traverser, this) -
                traverser.transform.position
            );

            direction = traverser.transform.InverseTransformDirection(direction);
            direction.Normalize();

            PlayableGesture.Parameter parameterX = new PlayableGesture.Parameter("X", direction.x);
            PlayableGesture.Parameter parameterY = new PlayableGesture.Parameter("Y", direction.y);

            traverser.Animator.CrossFadeGesture(
                this.clip.reachController, 0.95f / duration, 
                null, 0.1f, 0.1f, true,
                parameterX, parameterY
            );

            WaitForSeconds waitDelay = new WaitForSeconds(this.clip.reachStartDelay);
            yield return waitDelay;

            reachable.StartCoroutine(traverser.SetActive(reachable));
        }
        
        // AUTO CONNECTIONS: ----------------------------------------------------------------------

        public void GenerateAutoConnections()
        {
            this.reachables = this.GetAutoConnectionsElements().ToArray();
        }

        public override List<TraversableComponent> GetAutoConnectionsElements()
        {
            List<TraversableComponent> newReachables = new List<TraversableComponent>();
            
            float maxDistance = this.clip != null ? this.clip.maxTraverseDistance : 0f;
            TraversableComponent[] candidates = FindObjectsOfType<TraversableComponent>();
            
            foreach (TraversableComponent candidate in candidates)
            {
                if (candidate == this) continue;
                if (candidate == this.commuteToA) continue;
                if (candidate == this.commuteToB) continue;
                
                Segment segment1 = new Segment
                {
                    pointA = this.path.GetPointA(this),
                    pointB = this.path.GetPointB(this),
                };

                bool closeEnough = false;

                if (candidate is Climbable climbableCandidate)
                {
                    Segment segment2 = new Segment
                    {
                        pointA = climbableCandidate.path.GetPointA(climbableCandidate),
                        pointB = climbableCandidate.path.GetPointB(climbableCandidate),
                    };

                    closeEnough = Segment.IsSegmentNearSegment(segment1, segment2, maxDistance);
                }
                else if (candidate is Obstacle obstacleCandidate)
                {
                    Vector3 pointObstacle = obstacleCandidate.transform.position; 
                    Vector3 pointClimbable = Segment.NearestSegmentPoint(
                        segment1.pointA,
                        segment1.pointB,
                        pointObstacle
                    );

                    closeEnough = Vector3.Distance(pointClimbable, pointObstacle) <= maxDistance;
                }

                if (!closeEnough) continue;
                newReachables.Add(candidate);
            }

            return newReachables;
        }

        // GIZMOS: --------------------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            this.path.OnDrawGizmos(this);
        }
    }
}

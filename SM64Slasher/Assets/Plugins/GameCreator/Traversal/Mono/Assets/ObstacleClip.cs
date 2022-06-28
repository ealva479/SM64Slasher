namespace GameCreator.Traversal
{
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Characters;

    [CreateAssetMenu(
        fileName = "Obstacle Clip", 
        menuName = "Game Creator/Traverse/Obstacle Clip"
    )]
    
    public class ObstacleClip : ScriptableObject
    {
        private enum RotationMode
        {
            DontRotate,
            TowardsDestination,
            TowardsEdgeDirection
        }
        
        private enum ExitSpeed
        {
            None,
            WalkSpeed,
            RunSpeed,
        }

        public enum Animation
        {
            AnimationClip,
            CharacterState,
            RandomAnimationClip
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------

        [SerializeField] 
        private RotationMode rotationMode = RotationMode.TowardsDestination;
        public float rotationDuration = 0.25f;
        
        public bool ignoreCollisions = true;
        public Animation animation = Animation.AnimationClip;
        public AnimationClip animationClip;
        public CharacterState characterState;
        public AnimationClip[] randomAnimationClips;
        
        public float handlesOffsetUpward = 0f;
        public float handlesOffsetForward = 0f;
        
        public AnimationCurve curveOffsetX = AnimationCurve.Constant(0f, 1f, 0f);
        public AnimationCurve curveOffsetY = AnimationCurve.Constant(0f, 1f, 0f);
        public AnimationCurve curveOffsetZ = AnimationCurve.Constant(0f, 1f, 0f);
        
        public float transitionIn = 0.1f;
        public float transitionOut = 0.2f;

        [SerializeField]
        private ExitSpeed exitSpeed = ExitSpeed.None;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector3 GetExitVelocity(TraversalCharacter traverser)
        {
            float runSpeed = traverser.Character.characterLocomotion.runSpeed;
            Vector3 localVelocity = Vector3.zero;
            
            switch (this.exitSpeed)
            {
                case ExitSpeed.None:
                    localVelocity = Vector3.zero;
                    break;
                    
                case ExitSpeed.WalkSpeed:
                    localVelocity = Vector3.forward * (runSpeed * 0.5f);
                    break;
                
                case ExitSpeed.RunSpeed:
                    localVelocity = Vector3.forward * runSpeed;
                    break;
            }

            return traverser.Character.transform.TransformDirection(localVelocity);
        }
        
        public Vector3 GetDirection(TraversalCharacter traverser, ObstacleStep edge)
        {
            switch (this.rotationMode)
            {
                case RotationMode.DontRotate:
                    return traverser.transform.TransformDirection(Vector3.forward);

                case RotationMode.TowardsEdgeDirection:
                    return Quaternion.Euler(0f, edge.angle, 0f) * Vector3.forward;
                    
                case RotationMode.TowardsDestination:
                    return (edge.position - traverser.transform.position).normalized;
            }

            return Vector3.zero;
        }
    }   
}

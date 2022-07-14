using UnityEngine.Serialization;

namespace GameCreator.Traversal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Characters;

    [CreateAssetMenu(
        fileName = "Climb Clip", 
        menuName = "Game Creator/Traverse/Climb Clip"
    )]
    
    public class ClimbClip : ScriptableObject
    {
        public enum Orientation
        {
            Vertical,
            Horizontal
        }
        
        public enum InputMode
        {
            Directional,
            AutomaticForward,
            AutomaticBackward
        }
        
        public float offsetUpward = 0f;
        public float offsetForward = 0.1f;
        public bool ignoreCollisions = false;
        public Orientation orientation = Orientation.Vertical;
        
        public Translation transition = Translation.BySpeed(6f);

        [Header("Animations")]
        public RuntimeAnimatorController animatorController;
        public RuntimeAnimatorController reachController;
        public RuntimeAnimatorController grabController;

        public float reachStartDelay = 0f;
        public Translation reachTransition = new Translation();
        public float reachEndDelay = 0f;
        
        public float moveForwardSpeed = 4f;
        public float moveBackwardSpeed = 4f;

        [Header("Player")] 
        public InputMode inputMovement = InputMode.Directional;

        public KeyCode inputKeyDrop = KeyCode.LeftShift;
        public KeyCode inputKeyTraverse = KeyCode.Space;
        public float maxTraverseDistance = 2f;
    }   
}

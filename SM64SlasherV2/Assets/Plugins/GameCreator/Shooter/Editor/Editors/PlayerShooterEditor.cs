namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
    using GameCreator.Core;

    [CustomEditor(typeof(PlayerShooter))]
	public class PlayerShooterEditor : CharacterShooterEditor
	{
        protected override void PaintID()
        {
            return;
        }
    }
}
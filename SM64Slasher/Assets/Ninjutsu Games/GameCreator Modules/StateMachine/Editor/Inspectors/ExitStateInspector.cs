using UnityEngine;
using UnityEditor;
using GameCreator.Core;

namespace NJG.Graph
{
    [CustomEditor(typeof(ExitState), true)]
    public class ExitStateInspector : ActionsStateInspector
    {
        protected override bool ShouldDrawCustomHeader()
        {
            return false;
        }
    }
}
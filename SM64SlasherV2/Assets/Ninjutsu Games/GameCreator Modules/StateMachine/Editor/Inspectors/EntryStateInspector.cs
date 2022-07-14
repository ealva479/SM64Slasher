using UnityEngine;
using UnityEditor;
using GameCreator.Core;

namespace NJG.Graph
{
    [CustomEditor(typeof(EntryState), true)]
    public class EntryStateInspector : ActionsStateInspector
    {
        protected override bool ShouldDrawCustomHeader()
        {
            return false;
        }
    }
}
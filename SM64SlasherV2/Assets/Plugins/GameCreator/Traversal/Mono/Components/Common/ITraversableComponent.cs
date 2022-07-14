namespace GameCreator.Traversal
{
    using System.Collections;

    public interface ITraversableComponent
    {
        void OnUpdate(TraversalCharacter traverser);
        
        IEnumerator OnActivate(TraversalCharacter traverser, Climbable from);
        
        void OnDeactivate(TraversalCharacter traverser);
    }   
}

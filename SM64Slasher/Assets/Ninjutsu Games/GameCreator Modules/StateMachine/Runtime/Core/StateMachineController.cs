using System.Collections.Generic;
using UnityEngine;
using GameCreator.Core;
using GameCreator.Variables;
#if PHOTON_MODULE
using NJG.PUN;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NJG.Graph
{   
    //[AddComponentMenu("NJG/State Machine Controller")]
    // [DefaultExecutionOrder(-1000)]
    public class StateMachineController : LocalVariables
    {
        public enum PlayMode
        {
            OnStart,
            OnEnable,
            Manual
        }
        private const string INVOKER = "invoker";

        public PlayMode playMode = PlayMode.OnStart;
        public GraphReactions reactions;
        public StateMachine stateMachine;
        public MBVariable invokerVariable;
        public bool overrideColliderValues = true;
        public Collider stateMachineCollider;
        public bool canDrawCollider;

        private GameObject reactionsInstance;
        
        public bool VariablesInitialized { get { return initalized; } }

        private Dictionary<string, Variable> registeredVariables = new Dictionary<string, Variable>();
        
        #if PHOTON_MODULE
        public ActionsStateNetwork ASNetwork
        {
            get
            {
                if(!asn) asn = GetComponent<ActionsStateNetwork>();
                if (!asn) asn = gameObject.AddComponent<ActionsStateNetwork>();
                return asn;
            }
        }

        private ActionsStateNetwork asn;
        #endif

        /*protected override void Awake()
        {
            base.Awake();
            if(Application.isPlaying && playMode == PlayMode.OnStart) Execute();
        }*/
        
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void SetSMExecutionOrder()
        {
            int executionOrder = -13000;
            GameObject go = new GameObject();
            StateMachineController smc = go.AddComponent<StateMachineController>();
            MonoScript monoScript = MonoScript.FromMonoBehaviour(smc);

            if (executionOrder != MonoImporter.GetExecutionOrder(monoScript))
            {
                MonoImporter.SetExecutionOrder(monoScript, executionOrder); // very early but allows other scripts to run even earlier...
            }

            DestroyImmediate(go); 
        }
#endif

        protected override void Start()
         {
             base.Start();

             if(Application.isPlaying && playMode == PlayMode.OnStart) Execute();
         }
        private void OnEnable()
        {
            if(Application.isPlaying && playMode == PlayMode.OnEnable) Execute();
        }

        public void Execute()
        {
            /*if (!Application.isPlaying)
            {
                Debug.LogWarning("[StateMachineController Error] Cannot Run StateMachine in editor mode.", gameObject);
                return;
            }*/
            if (!stateMachine)
            {
                Debug.LogWarning("[StateMachineController Error] StateMachine is null.", gameObject);
                return;
            }
            if (!stateMachine.sourceReactions)
            {
                Debug.LogWarning("[StateMachineController Error] StateMachine Reactions reference is null.", gameObject);
                return;
            }
            //Debug.LogWarningFormat(gameObject, "[StateMachineController]Execute: {0} reactions: {1}", gameObject, reactions);

            if (!reactions)
            {
                
                GameObject instance = Instantiate(
                    stateMachine.sourceReactions.gameObject,
                    transform.position,
                    transform.rotation,
                    transform
                );
                #if PHOTON_MODULE
                var net = instance.GetComponent<ActionsStateNetwork>();
                if (net)
                {
                    Destroy(net);
                }
                #endif
                instance.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                reactions = instance.GetComponent<GraphReactions>();
                reactions.Initialize();
                var list = GetComponent<ListVariables>();
                if (list)
                {
                    var newList = reactions.gameObject.AddComponent<ListVariables>();
                    newList.variables = list.variables;
                    newList.references = list.references;
                }
                //if(overrideColliderValues) 
                if (stateMachineCollider)
                {
                    var col = reactions.GetComponent<Collider>();
                    CopyCollider(stateMachineCollider, col);
                    col.enabled = true;
                    stateMachineCollider.enabled = false;
                }
                //controller.Initialize();
                UpdateVariableValues();
            }

            reactionsInstance = stateMachine.Init(this);
        }
        
        private void UpdateVariableValues()
        {
            for (int i = 0, imax = references.Length; i < imax; i++)
            {
                MBVariable controllerVar = references[i];
                if(!controllerVar) continue;
                for (int e = 0, emax = reactions.references.Length; e < emax; e++)
                {
                    MBVariable reactionVar = reactions.references[e];
                    if (reactionVar.variable.name == controllerVar.variable.name)
                    {
                        var cachedValue = controllerVar.variable.Get();

                        references[i].variable = reactionVar.variable;
                        if (cachedValue != null) reactionVar.variable.Update(cachedValue);
                        
                        if(!registeredVariables.ContainsKey(reactionVar.variable.name))
                        {
                            VariablesManager.events.SetOnChangeLocal(
                                OnReactionsVariable,
                                reactions.gameObject,
                                reactionVar.variable.name
                            );
                            VariablesManager.events.SetOnChangeLocal(
                                OnControllerVariable,
                                gameObject,
                                controllerVar.variable.name
                            );
                            registeredVariables.Add(reactionVar.variable.name, controllerVar.variable);
                            // Debug.LogWarningFormat("Variable id registered: {0}", reactionVar.variable.name);
                        }
					}
                }
            }
            reactions.Reset();
            RequireInit(true);
        }
        
        private void OnControllerVariable(string variableID)
        {
            if (registeredVariables.ContainsKey(variableID))
            {
                // Debug.LogWarningFormat("OnControllerVariable variableID: {0} value1: {1} value2: {2} equals: {3}", 
                //     variableID, Get(variableID).Get(), reactions.Get(variableID).Get(), reactions.Get(variableID).Get().Equals(Get(variableID).Get()));

                if(!reactions.Get(variableID).Get().Equals(Get(variableID).Get()))
                {
                    reactions.Get(variableID).Update(Get(variableID).Get());
                    VariablesManager.events.OnChangeLocal(reactions.gameObject, variableID);
                }
            }
        }

        private void OnReactionsVariable(string variableID)
        {
            if (registeredVariables.ContainsKey(variableID))
            {
                // Debug.LogWarningFormat("OnReactionsVariable variableID: {0} value1: {1} value2: {2} equals: {3}", 
                //     variableID, Get(variableID).Get(), reactions.Get(variableID).Get(), Get(variableID).Get().Equals(reactions.Get(variableID).Get()));

                if(!Get(variableID).Get().Equals(reactions.Get(variableID).Get()))
                {
                    Get(variableID).Update(reactions.Get(variableID).Get());
                    VariablesManager.events.OnChangeLocal(gameObject, variableID);
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (reactionsInstance) Destroy(reactionsInstance);
        }

        public IActionsList GetActions(string name)
        {
            ActionsState state = stateMachine.GetActionsState(name);

            if(state)
            {
                return state.GetActions(reactions);
            }
            else
            {
                Debug.LogWarningFormat("Couldn't find node with name {0} on state machine {1}", name, this, gameObject);
            }

            return null;
        }

        public void StopActions(string name)
        {
            IActionsList actions = GetActions(name);

            if (actions)
            {
                actions.Stop();
            }
            else
            {
                Debug.LogWarningFormat("Couldn't find node with name {0} on state machine {1}", name, this, gameObject);
            }
        }

        public void ExecuteNode(string name)
        {
            stateMachine.GetNode(name).OnEnter(reactions, gameObject);
        }
        
        private void CopyCollider(Collider source, Collider destiny)
        {
            if (source is BoxCollider)
            {
                var col1 = source as BoxCollider;
                var col2 = destiny as BoxCollider;
                col2.center = col1.center;
                col2.size = col1.size;
                col2.contactOffset = col1.contactOffset;
                col2.isTrigger = col1.isTrigger;
                col2.sharedMaterial = col1.sharedMaterial;
                col2.tag = col1.tag;
            }
            if (source is SphereCollider)
            {
                var col1 = source as SphereCollider;
                var col2 = destiny as SphereCollider;
                col2.center = col1.center;
                col2.radius = col1.radius;
                col2.contactOffset = col1.contactOffset;
                col2.isTrigger = col1.isTrigger;
                col2.sharedMaterial = col1.sharedMaterial;
                col2.tag = col1.tag;
            }
            if (source is CapsuleCollider)
            {
                var col1 = source as CapsuleCollider;
                var col2 = destiny as CapsuleCollider;
                col2.center = col1.center;
                col2.radius = col1.radius;
                col2.height = col1.height;
                col2.direction = col1.direction;
                col2.contactOffset = col1.contactOffset;
                col2.isTrigger = col1.isTrigger;
                col2.sharedMaterial = col1.sharedMaterial;
                col2.tag = col1.tag;
            }
            if (source is MeshCollider)
            {
                var col1 = source as MeshCollider;
                var col2 = destiny as MeshCollider;
                col2.convex = col1.convex;
                col2.cookingOptions = col1.cookingOptions;
                col2.sharedMesh = col1.sharedMesh;
                col2.contactOffset = col1.contactOffset;
                col2.isTrigger = col1.isTrigger;
                col2.sharedMaterial = col1.sharedMaterial;
                col2.tag = col1.tag;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (canDrawCollider && stateMachine)
            {
                // Debug.LogWarning("Drawing collider preview of: {0}", stateMachineCollider);
                if(stateMachineCollider)
                {
                    if(overrideColliderValues) SyncCollider(false);
                    else SyncCollider(true);
                    
                    Color c = Color.green;
                    c.a = 0.3f;
                    Gizmos.color = c;
                    if (stateMachineCollider is MeshCollider)
                    {
                        var col = stateMachineCollider as MeshCollider;
                        if(col) Gizmos.DrawMesh (col.sharedMesh, col.transform.position, col.transform.rotation, col.transform.localScale);
                    }
                    else if (stateMachineCollider is BoxCollider)
                    {
                        var col = stateMachineCollider as BoxCollider;
                        if(col) Gizmos.DrawCube(col.transform.position + col.center, Vector3.Scale(col.size, col.transform.localScale));
                    }
                    else if (stateMachineCollider is SphereCollider)
                    {
                        var col = stateMachineCollider as SphereCollider;
                        if(col) Gizmos.DrawSphere(col.transform.position + col.center, col.radius * GetHighestValue(col.transform.localScale));
                    }
                    else if (stateMachineCollider is CapsuleCollider)
                    {
                        c.a = 1f;
                        var col = stateMachineCollider as CapsuleCollider;
                        if(col) DrawWireCapsule(col.transform.position + col.center, col.transform.rotation, 
                            col.radius * GetHighestValueXZ(col.transform.localScale), col.height * col.transform.localScale.y, c);
                    }

                }
            }
        }
        
        public float GetHighestValueXZ(Vector3 vector)
        {
            float maxValue = float.MinValue;
            if (vector.x > maxValue) maxValue = vector.x;
            // if (vector.y > maxValue) maxValue = vector.y;
            if (vector.z > maxValue) maxValue = vector.z;
            return maxValue;
        }
        
        public float GetHighestValue(Vector3 vector)
        {
            float maxValue = float.MinValue;
            if (vector.x > maxValue) maxValue = vector.x;
            if (vector.y > maxValue) maxValue = vector.y;
            if (vector.z > maxValue) maxValue = vector.z;
            return maxValue;
        }
        
        public void SyncCollider(bool copyFromController)
        {
            if(!stateMachine) return;
            if(!stateMachine.sourceReactions) return;
            
            var sourceCollider = stateMachine.sourceReactions.GetComponent<Collider>();
            
            if ((!sourceCollider && stateMachineCollider) ||
                (sourceCollider && stateMachineCollider 
                                && sourceCollider.GetType() != stateMachineCollider.GetType()))
            {
                if(!(stateMachineCollider is CharacterController)) DestroyImmediate(stateMachineCollider, true);
            }
            
            if (sourceCollider && !stateMachineCollider && gameObject)
            {
                stateMachineCollider = gameObject.AddComponent(sourceCollider.GetType()) as Collider;
                EditorUtility.CopySerialized(sourceCollider, stateMachineCollider);
            }
            else if(sourceCollider && stateMachineCollider && sourceCollider.GetType() == stateMachineCollider.GetType())
            {
                if(copyFromController) EditorUtility.CopySerialized(stateMachineCollider, sourceCollider);
                else EditorUtility.CopySerialized(sourceCollider, stateMachineCollider);
            }
        }
        
        private void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
        {
            if (_color != default(Color))
                Handles.color = _color;
            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (_height - (_radius * 2)) / 2;
 
                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
                //draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
                Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);
 
            }
        }
#endif
    }
}
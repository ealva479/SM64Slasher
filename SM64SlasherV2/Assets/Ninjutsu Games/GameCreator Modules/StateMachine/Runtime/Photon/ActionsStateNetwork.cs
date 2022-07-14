#if PHOTON_MODULE
using System;
using System.Collections.Generic;
using GameCreator.Core;
using NJG.Graph;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace NJG.PUN
{
    [AddComponentMenu("")] //, RequireComponent(typeof(View))
    public class ActionsStateNetwork : MonoBehaviour, IMatchmakingCallbacks
    {
        [System.Serializable]
        public class ActionRPC
        {
            public enum TargetType
            {
                RpcTarget,
                PhotonPlayer
            }
            public ActionsState actions;
            public TargetType targetType = TargetType.RpcTarget;
            public RpcTarget targets = RpcTarget.Others;
            public TargetPhotonPlayer targetPlayer;
        }

        public List<ActionRPC> actions = new List<ActionRPC>();

        public PhotonView View
        {
            get
            {
                if (!view) view = GetComponentInParent<PhotonView>();
                return view;
            }
        }
        private bool initialized;
        private PhotonView view;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);

#if UNITY_EDITOR
            // HideStuff();
#endif
            if (PhotonNetwork.UseRpcMonoBehaviourCache)
            {
                View.RefreshRpcMonoBehaviourCache();
            }
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

#if UNITY_EDITOR
        /*private void HideStuff()
        {
            this.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        }

        private void OnValidate()
        {
            HideStuff();
        }*/
#endif

        private void Awake()
        {
            if (PhotonNetwork.InRoom)
            {
                if(!initialized) Initialize();
            }
        }

        private void Initialize()
        {
            if (initialized) return;

            initialized = true;
            
            if (View.IsMine || View.IsRoomView)
            {
                for (int i = 0, imax = actions.Count; i < imax; i++)
                {
                    int index = i;
                    ActionRPC rpc = actions[i];
                    if (rpc == null || !rpc.actions) continue;
                    rpc.actions.onExecute += reactions =>
                    {
                        var go = reactions.gameObject;
                        Debug.LogWarningFormat("[ActionsStateNetwork] OnExecute index: {0} name: {1} invoker: {2}",
                            index, rpc.actions.Root.sourceReactions.gameObject.name, go);

                        //rpc.lastInvoker = go;
                        if(go && View.IsRoomView && go.name.StartsWith("==>"))
                        {
                            go.name = $"{go.name.Replace("==>", string.Empty)}";
                            return;
                        }
                            
                        OnActionsExecute(index); 
                    };
                }
            }
        }

        private void OnActionsExecute(int index)
        {
            ActionRPC rpc = actions[index];
            if (rpc == null)
            {
                Debug.LogWarningFormat("Could not Sync Action reference is null.");
                return;
            }

            Debug.LogWarningFormat("[ActionsStateNetwork] OnActionsExecute index: {0} name: {4} IsRoomView: {1} isMine: {2}, isExecuting: {3}"
                , index, View.IsRoomView, View.IsMine, rpc.actions.GetActions(rpc.actions.Root.sourceReactions).isExecuting, rpc.actions.name);

            if (rpc.targetType == ActionRPC.TargetType.PhotonPlayer)
            {
                View.RPC(nameof(APRPC), rpc.targetPlayer.GetPhotonPlayer(gameObject), index);
            }
            else
            {
                View.RPC(nameof(APRPC), rpc.targets, index);
            }
        }

        [PunRPC]
        public virtual void APRPC(int index, PhotonMessageInfo info)
        {
            if(index >= actions.Count)
            {
#if UNITY_EDITOR
                Debug.LogWarning("The action you are trying to execute is out of range. Could be that a new action was created on the original client." + gameObject, gameObject);
#endif
                return;
            }

            /*if (info.Sender.IsLocal)
            {
                Debug.LogWarning("Prevent executing this since local player started it.");
                return;
            }*/       

            ActionRPC rpc = actions[index];

            Debug.LogWarningFormat("[ActionsStateNetwork] RPC index: {0} name: {1} isExecuting: {2} IsRoomView: {3} isMine: {4} sender: {5}", 
                index, rpc.actions.name, rpc.actions.GetActions(rpc.actions.Root.sourceReactions).isExecuting, View.IsRoomView, info.photonView.IsMine, info.Sender);

            if (info.photonView.IsRoomView)
            {
                var o = info.photonView.gameObject;
                o.name = $"==>{o.name}";
            }
            
            rpc.actions.OnEnter(rpc.actions.Root.sourceReactions, info.photonView.gameObject);
            // rpc.actions.GetActions(rpc.actions.Root.sourceReactions).Execute(info.photonView.gameObject, null);
        }


        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinedRoom()
        {
            Initialize();
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public void OnLeftRoom()
        {
        }
    }
}
#endif
using System;
using System.Collections.Generic;
using Dories.Logging;


namespace Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem
{
     internal sealed class FsmSystem<T>
    {
        internal T _fsmOwner;
        private FsmNodeEntity<T> _curNode;
        private Dictionary<Type, FsmNodeEntity<T>> _nodeDict;
        
        public FsmSystem(T fsmOwner)
        {
            _fsmOwner = fsmOwner;
            _nodeDict = new Dictionary<Type, FsmNodeEntity<T>>();
        }

        public FsmSystem<T> AddNode(FsmNodeEntity<T> node)
        {
            if (!_nodeDict.TryAdd(node.GetType(), node))
            {
                Log.Error("Patch", $"Node {node.GetType()} already exists");
            }
            node.Initialize(this);

            return this;
        }

        public void StartFsm<TK>()  where TK : FsmNodeEntity<T>
        {
            if (_curNode != null)
            {
                Log.Error("Patch", "there is already a fsm node running");
            }
            
            if (_nodeDict.TryGetValue(typeof(TK), out FsmNodeEntity<T> node))
            {
                _curNode = node;
                node.OnEnter();
            }
            else
            {
                Log.Error("Patch", $"Node {typeof(TK)} does not exist");
            }
        }

        public void ChangeState<TK>() where TK : FsmNodeEntity<T>
        {
            if (_curNode == null)
            {
                Log.Error("Patch", "there is not a fsm node running");
                return;
            }

            if (_nodeDict.TryGetValue(typeof(TK), out FsmNodeEntity<T> node))
            {
                _curNode.OnExit();
                _curNode = node;
                _curNode.OnEnter();
            }
        }
    }
}
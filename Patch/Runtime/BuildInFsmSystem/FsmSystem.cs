using System;
using System.Collections.Generic;
using Dories.YooAssetSystem.LogSystem;


namespace Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem
{
     internal sealed class FsmSystem<T>
    {
        internal T _fsmOwner;
        internal ILog _logger;
        private FsmNodeEntity<T> _curNode;
        private Dictionary<Type, FsmNodeEntity<T>> _nodeDict;
        
        public FsmSystem(T fsmOwner, ILog logger)
        {
            _fsmOwner = fsmOwner;
            _logger = logger;
            _nodeDict = new Dictionary<Type, FsmNodeEntity<T>>();
        }

        public FsmSystem<T> AddNode(FsmNodeEntity<T> node)
        {
            if (!_nodeDict.TryAdd(node.GetType(), node))
            {
                _logger.Error($"Node {node.GetType()} already exists");
            }
            node.Initialize(this);

            return this;
        }

        public void StartFsm<TK>()  where TK : FsmNodeEntity<T>
        {
            if (_curNode != null)
            {
                _logger.Error("there is already a fsm node running");
            }
            
            if (_nodeDict.TryGetValue(typeof(TK), out FsmNodeEntity<T> node))
            {
                _curNode = node;
                node.OnEnter();
            }
            else
            {
                _logger.Error($"Node {typeof(TK)} does not exist");
            }
        }

        public void ChangeState<TK>() where TK : FsmNodeEntity<T>
        {
            if (_curNode == null)
            {
                _logger.Error("there is not a fsm node running");
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
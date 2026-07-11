using Dories.YooAssetSystem.Runtime.LogSystem;

namespace Dories.YooAssetSystem.Runtime.Patch.BuildInFsmSystem
{
    public class FsmNodeEntity<T>
    {
        protected T _owner;
        protected ILog _logger;
        
        private FsmSystem<T> _fsm;

        internal void Initialize(FsmSystem<T> fsm)
        {
            _owner = fsm._fsmOwner;
            _logger  = fsm._logger;
            _fsm = fsm;
        }
        
        protected internal virtual void OnEnter()
        {
            
        }

        protected internal virtual void OnExit()
        {
            
        }

        protected void ChangeState<TK>() where TK : FsmNodeEntity<T>
        {
            _fsm.ChangeState<TK>();
        }
    }
}
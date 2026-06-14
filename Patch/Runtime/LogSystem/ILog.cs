namespace Dories.YooAssetSystem.Runtime.Patch.LogSystem
{
    public interface ILog
    {
        public void Debug(object message);
        
        public void Info(object message);
        
        public void Warn(object message);
        
        public void Error(object message);
    }
}
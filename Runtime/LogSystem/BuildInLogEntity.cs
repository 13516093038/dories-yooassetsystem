namespace Dories.YooAssetSystem.Runtime.LogSystem
{
    public class BuildInLogEntity : ILog
    {
        public void Debug(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Info(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Warn(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void Error(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
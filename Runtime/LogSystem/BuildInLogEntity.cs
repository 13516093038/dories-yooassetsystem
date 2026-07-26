namespace Dories.YooAssetSystem.Runtime.LogSystem
{
    public class BuildInLogEntity : ILog
    {
        private const string DebugLogColor = "#4ADE80";
        private const string InfoLogColor = "#93C5FD";
        private const string WarnLogColor = "#FACC15";
        private const string ErrorLogColor = "#F87171";
        private const string ExceptionLogColor = "#FB7185";

        public void Debug(object message)
        {
            UnityEngine.Debug.Log(Format(message, DebugLogColor));
        }

        public void Info(object message)
        {
            UnityEngine.Debug.Log(Format(message, InfoLogColor));
        }

        public void Warn(object message)
        {
            UnityEngine.Debug.LogWarning(Format(message, WarnLogColor));
        }

        public void Error(object message)
        {
            UnityEngine.Debug.LogError(Format(message, ErrorLogColor));
        }

        public void Exception(object message)
        {
            if (message is System.Exception ex)
            {
                UnityEngine.Debug.LogError(Format(ex.Message, ExceptionLogColor));
                UnityEngine.Debug.LogException(ex);
                return;
            }

            var text = message?.ToString() ?? string.Empty;
            UnityEngine.Debug.LogError(Format(text, ExceptionLogColor));
            UnityEngine.Debug.LogException(new System.Exception(text));
        }

        private static string Format(object message, string colorHex)
        {
            return $"<color={colorHex}>[DoriesYooAssetSystem] {message}</color>";
        }
    }
}

using System;
using Dories.Logging;

namespace Dories.YooAssetSystem.Runtime.LogSystem
{
    /// <summary>
    /// 把 YooAsset 内部日志转发到 Dories.Logging。
    /// </summary>
    internal sealed class YooAssetLogger : YooAsset.ILogger
    {
        private const string Tag = "YooAsset";

        public void Log(string message) => Dories.Logging.Log.Debug(Tag, message);

        public void LogWarning(string message) => Dories.Logging.Log.Warn(Tag, message);

        public void LogError(string message) => Dories.Logging.Log.Error(Tag, message);

        public void LogException(Exception exception) => Dories.Logging.Log.Exception(Tag, exception);
    }
}

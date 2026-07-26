using System;
using YooAsset;

namespace Dories.YooAssetSystem.Runtime.LogSystem
{
    public interface ILog : ILogger
    {
        public void Debug(object message);
        
        public void Info(object message);
        
        public void Warn(object message);
        
        public void Error(object message);

        public void Exception(object message);

        // 桥接到 ILogger，实现类不必再写
        void ILogger.Log(string message) => Debug(message);
        void ILogger.LogWarning(string message) => Warn(message);
        void ILogger.LogError(string message) => Error(message);
        void ILogger.LogException(Exception exception) => Exception(exception);
    }
}
namespace Dories.YooassetSystem.Runtime.Patch
{
    public enum PlayMode
    {
        /// <summary>
        /// 编辑器下的模拟模式
        /// </summary>
        EditorSimulateMode,

        /// <summary>
        /// 离线运行模式
        /// </summary>
        OfflinePlayMode,

        /// <summary>
        /// 联机运行模式
        /// </summary>
        HostPlayMode,
        
        /// <summary>
        /// 弱联网运行模式
        /// </summary>
        WeakOnlinePlayMode,
        
        /// <summary>
        /// 微信小游戏运行模式
        /// </summary>
        WeChatMiniGameMode,
        
        /// <summary>
        /// 抖音小游戏运行模式
        /// </summary>
        TTMiniGameMode,

        /// <summary>
        /// 自定义运行模式
        /// </summary>
        CustomPlayMode,
    }
}
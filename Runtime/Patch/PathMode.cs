namespace Dories.YooassetSystem.Runtime.Patch
{
    public enum PathMode
    {
        /// <summary>
        /// 编辑器模式
        /// </summary>
        EditorSimulateMode,

        /// <summary>
        /// 单机模式
        /// </summary>
        OfflinePlayMode,

        /// <summary>
        /// 热更新模式
        /// </summary>
        HostPlayMode,

        /// <summary>
        /// 弱联网模式
        /// </summary>
        WeakOnlinePlayMode,

        /// <summary>
        /// Web模式
        /// </summary>
        WebPlayMode,

        /// <summary>
        /// 微信小程序模式
        /// </summary>
        WEIXINMiNIGAMEPlayMode,

        /// <summary>
        /// 自定义模式
        /// </summary>
        CustomPlayMode,
    }
}
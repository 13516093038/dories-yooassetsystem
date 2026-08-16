using System;
#if DORIES_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using UnityEngine.SceneManagement;
using YooAsset;
// Unity 6 起 SceneManagement 也有 SceneHandle，与 YooAsset 重名，这里固定指向 YooAsset。
using SceneHandle = YooAsset.SceneHandle;

namespace Dories.YooassetSystem.Runtime.AssetLoader
{
    /// <summary>
    /// 场景加载结果封装，对业务隐藏 YooAsset 的 SceneHandle。
    /// </summary>
    public sealed class SceneLoadResult
    {
        private readonly SceneHandle _handle;

        internal SceneLoadResult(string location, SceneHandle handle)
        {
            Location = location;
            _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        /// <summary>
        /// 场景资源定位地址
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Unity 场景对象
        /// </summary>
        public Scene Scene => _handle.IsValid ? _handle.SceneObject : default;

        /// <summary>
        /// 加载进度（0~1）。allowSceneActivation=false 时会停在约 0.9
        /// </summary>
        public float Progress => _handle.IsValid ? _handle.Progress : 0f;

        /// <summary>
        /// 句柄是否仍有效
        /// </summary>
        public bool IsValid => _handle.IsValid;

        /// <summary>
        /// 场景是否已完成加载并激活
        /// </summary>
        public bool IsActivated =>
            _handle.IsValid && _handle.IsDone && _handle.Status == EOperationStatus.Succeeded;

        /// <summary>
        /// 允许场景激活（对应 allowSceneActivation=false 时的解除挂起）
        /// </summary>
        public void Activate()
        {
            if (!_handle.IsValid)
                throw new InvalidOperationException($"SceneLoadResult is invalid. Location: {Location}");

            if (IsActivated)
                return;

            _handle.AllowSceneActivation();
        }

        /// <summary>
        /// 允许激活并等待场景真正加载完成
        /// </summary>
#if DORIES_UNITASK_SUPPORT
        public async UniTask ActivateAsync()
#else
        public async Task ActivateAsync()
#endif
        {
            Activate();
            await WaitForActivationAsync();
        }

        /// <summary>
        /// 等待场景激活完成（需先调用 Activate，或加载时 allowSceneActivation=true）
        /// </summary>
#if DORIES_UNITASK_SUPPORT
        public async UniTask WaitForActivationAsync()
#else
        public async Task WaitForActivationAsync()
#endif
        {
            if (!_handle.IsValid)
                throw new InvalidOperationException($"SceneLoadResult is invalid. Location: {Location}");

            if (!_handle.IsDone)
                await _handle;

            if (_handle.Status != EOperationStatus.Succeeded)
            {
                var error = string.IsNullOrEmpty(_handle.Error)
                    ? $"Activate scene failed: {Location}"
                    : _handle.Error;
                throw new Exception(error);
            }
        }

        /// <summary>
        /// 将本场景设为当前激活场景（多场景并存时使用）
        /// </summary>
        public bool SetAsActiveScene()
        {
            if (!_handle.IsValid)
                return false;

            return _handle.ActivateScene();
        }
    }
}

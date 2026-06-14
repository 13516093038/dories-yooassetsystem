using System;
using YooAsset;

namespace Dories.YooAssetSystem.Patch.Runtime.Operations
{
    public enum ClearCacheOperationMode
    {
        ClearAllBundleFiles,

        /// <summary>
        /// 清理未在使用的资源包文件
        /// </summary>
        ClearUnusedBundleFiles,

        /// <summary>
        /// 清理指定地址的资源包文件
        /// </summary>
        ClearBundleFilesByLocations,

        /// <summary>
        /// 清理指定标签的资源包文件
        /// </summary>
        ClearBundleFilesByTags,

        /// <summary>
        /// 清理所有清单文件
        /// </summary>
        ClearAllManifestFiles,


        /// <summary>
        /// 清理未在使用的清单文件
        /// </summary>
        ClearUnusedManifestFiles,
    }

    internal static class ClearCacheOperationModeExtensions
    {
        public static string ToClearCacheMethods(this ClearCacheOperationMode mode)
        {
            return mode switch
            {
                ClearCacheOperationMode.ClearAllBundleFiles => ClearCacheMethods.ClearAllBundleFiles,
                ClearCacheOperationMode.ClearUnusedBundleFiles => ClearCacheMethods.ClearUnusedBundleFiles,
                ClearCacheOperationMode.ClearBundleFilesByLocations => ClearCacheMethods.ClearBundleFilesByLocations,
                ClearCacheOperationMode.ClearBundleFilesByTags => ClearCacheMethods.ClearBundleFilesByTags,
                ClearCacheOperationMode.ClearAllManifestFiles => ClearCacheMethods.ClearAllManifestFiles,
                ClearCacheOperationMode.ClearUnusedManifestFiles => ClearCacheMethods.ClearUnusedManifestFiles,
                _ => throw new ArgumentException($"Invalid clear cache operation mode: {mode}"),
            };
        }
    }
}
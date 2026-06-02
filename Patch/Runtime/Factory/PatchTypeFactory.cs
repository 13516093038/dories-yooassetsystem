using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dories.YooassetSystem.Runtime.Patch
{
    /// <summary>
    /// 根据 Inspector 配置的类名创建 Patch 相关实例
    /// </summary>
    internal static class PatchTypeFactory
    {
        private static readonly Dictionary<string, Type> TypeMap = BuildTypeMap();

        public static T CreateInstance<T>(string typeName) where T : class
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            if (!TypeMap.TryGetValue(typeName, out var type) || !typeof(T).IsAssignableFrom(type))
            {
                Debug.LogError($"找不到 {typeof(T).Name} 的实现类: {typeName}");
                return null;
            }

            try
            {
                return (T)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Debug.LogError($"创建 {typeName} 实例失败: {e.Message}");
                return null;
            }
        }

        private static Dictionary<string, Type> BuildTypeMap()
        {
            var map = new Dictionary<string, Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract || type.IsGenericType)
                        continue;

                    RegisterType(map, type.Name, type);
                    RegisterType(map, type.FullName, type);
                }
            }

            return map;
        }

        private static void RegisterType(Dictionary<string, Type> map, string key, Type type)
        {
            if (string.IsNullOrEmpty(key) || map.ContainsKey(key))
                return;

            map.Add(key, type);
        }
    }

    internal static class RemoteServiceFactory
    {
        public static YooAsset.IRemoteService CreateRemoteService(string typeName) =>
            PatchTypeFactory.CreateInstance<YooAsset.IRemoteService>(typeName);
    }

    internal static class BundleDecryptorFactory
    {
        public static YooAsset.IBundleDecryptor CreateBundleDecryptor(string typeName) =>
            PatchTypeFactory.CreateInstance<YooAsset.IBundleDecryptor>(typeName);
    }

    internal static class ManifestDecryptorFactory
    {
        public static YooAsset.IManifestDecryptor CreateManifestDecryptor(string typeName) =>
            PatchTypeFactory.CreateInstance<YooAsset.IManifestDecryptor>(typeName);
    }
}

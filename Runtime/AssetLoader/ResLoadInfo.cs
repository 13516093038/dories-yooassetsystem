#if UNITY_EDITOR
using System.Collections.Generic;

namespace Dories.YooassetSystem.Runtime.AssetLoader
{
    public enum ResLoadResourceType
    {
        Asset,
        Scene,
        RawFile
    }

    public struct PackageResLoadViewInfo
    {
        public string PackageName;
        public Dictionary<string, ResLoadInfo> ResLoadInfos;
        public Dictionary<string, ResLoadInfo> SceneLoadInfos;
        public Dictionary<string, ResLoadInfo> RawFileLoadInfos;

        public PackageResLoadViewInfo(string packageName)
        {
            PackageName = packageName;
            ResLoadInfos = new Dictionary<string, ResLoadInfo>();
            SceneLoadInfos = new Dictionary<string, ResLoadInfo>();
            RawFileLoadInfos = new Dictionary<string, ResLoadInfo>();
        }

        public bool HasAnyEntry()
        {
            return ResLoadInfos.Count > 0 || SceneLoadInfos.Count > 0 || RawFileLoadInfos.Count > 0;
        }

        public int TotalEntryCount()
        {
            return ResLoadInfos.Count + SceneLoadInfos.Count + RawFileLoadInfos.Count;
        }
    }

    public struct ResLoadInfo
    {
        public float LoadTime;
        public int RefCount;
    }
}
#endif

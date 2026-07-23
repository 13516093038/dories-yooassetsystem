using System.Collections.Generic;
using Dories.yooassetsystem.Editor.Utilities;
using Dories.YooassetSystem.Runtime.AssetLoader;
using Dories.YooAssetSystem.Runtime.LogSystem;
using UnityEditor;
using UnityEngine;

namespace Dories.YooAssetSystem.Editor.Inspector
{
    [CustomEditor(typeof(AssetLoaderEntity))]
    public class AssetLoaderEntityInspector : UnityEditor.Editor
    {
        private SerializedProperty _logNameProp;

        private readonly List<PackageResLoadViewInfo> _cachedAssetsInfo = new();
        private Vector2 _scrollPosition;
        private bool _cacheFoldout = true;

        private void OnEnable()
        {
            _logNameProp = serializedObject.FindProperty("ILog");
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TypeSelectorUtility.DrawTypePopup(
                _logNameProp,
                typeof(ILog),
                "ILog",
                false,
                true,
                "You must select a Logger"
            );

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);
            DrawCacheMonitor();
        }

        private void DrawCacheMonitor()
        {
            _cacheFoldout = EditorGUILayout.Foldout(_cacheFoldout, "Cached Resources", true);
            if (!_cacheFoldout)
                return;

            EditorGUI.indentLevel++;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("进入 Play Mode 后显示缓存。", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            var entity = (AssetLoaderEntity)target;
            entity.GetAssetGroupCacheLoadInfo(_cachedAssetsInfo);

            var totalCount = CountCachedResources(_cachedAssetsInfo);
            EditorGUILayout.LabelField("Package 数量", _cachedAssetsInfo.Count.ToString());
            EditorGUILayout.LabelField("缓存资源数量", totalCount.ToString());
            EditorGUILayout.Space(4);

            if (totalCount == 0)
            {
                EditorGUILayout.LabelField("(无缓存资源)", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
                return;
            }

            DrawCacheTableHeader();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(280f));
            for (var i = 0; i < _cachedAssetsInfo.Count; i++)
            {
                var packageInfo = _cachedAssetsInfo[i];
                DrawResourceRows(packageInfo.PackageName, "Asset", packageInfo.ResLoadInfos, true);
                DrawResourceRows(packageInfo.PackageName, "Scene", packageInfo.SceneLoadInfos, false);
                DrawResourceRows(packageInfo.PackageName, "RawFile", packageInfo.RawFileLoadInfos, true);
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
        }

        private static void DrawResourceRows(
            string packageName,
            string typeLabel,
            Dictionary<string, ResLoadInfo> loadInfos,
            bool showRefCount)
        {
            if (loadInfos == null || loadInfos.Count == 0)
                return;

            foreach (var pair in loadInfos)
            {
                var resInfo = pair.Value;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(typeLabel, GUILayout.Width(56f));
                GUILayout.Label(packageName, GUILayout.Width(100f));
                GUILayout.Label(pair.Key);
                GUILayout.Label(showRefCount ? resInfo.RefCount.ToString() : "-", GUILayout.Width(40f));
                GUILayout.Label($"{resInfo.LoadTime:F1}ms", GUILayout.Width(64f));
                EditorGUILayout.EndHorizontal();
            }
        }

        private static int CountCachedResources(List<PackageResLoadViewInfo> packageInfos)
        {
            var count = 0;
            for (var i = 0; i < packageInfos.Count; i++)
                count += packageInfos[i].TotalEntryCount();

            return count;
        }

        private static void DrawCacheTableHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Type", EditorStyles.miniLabel, GUILayout.Width(56f));
            EditorGUILayout.LabelField("Package", EditorStyles.miniLabel, GUILayout.Width(100f));
            EditorGUILayout.LabelField("Location", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Ref", EditorStyles.miniLabel, GUILayout.Width(40f));
            EditorGUILayout.LabelField("Load", EditorStyles.miniLabel, GUILayout.Width(64f));
            EditorGUILayout.EndHorizontal();
        }
    }
}

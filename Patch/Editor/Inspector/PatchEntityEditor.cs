
using Dories.YooassetSystem.Patch.Editor.Inspector;
using Dories.YooAssetSystem.Patch.Runtime.Operations;
 using Dories.YooAssetSystem.Runtime.Patch;
 using Dories.YooAssetSystem.Runtime.Patch.LogSystem;
using UnityEditor;
using UnityEngine;
using YooAsset;
 using YooAsset.Editor;
 using PlayMode = Dories.YooAssetSystem.Runtime.Patch.PlayMode;

 namespace Dories.YooAssetSystem.Patch.Editor.Inspector
 {
     [CustomEditor(typeof(PatchEntity))]
     public class PatchEntityEditor : UnityEditor.Editor
     {
         // 缓存 SerializedProperty，避免每帧 FindProperty
         private SerializedProperty _logNameProp;
         private SerializedProperty _isReleaseModeProp;
         private SerializedProperty _playModeProp;
         private SerializedProperty _packagesInfoListProp;
         private SerializedProperty _isAutoDownloadProp;

         private const int DefaultTimeout = 60;
         private const int DefaultDownloadingMaxNum = 10;
         private const int DefaultFailedTryAgainTimes = 3;

         private BundleCollectorSetting GetCollectorSetting()
         {
             return BundleCollectorSettingData.Setting;
         }

         private void OnEnable()
         {
             _logNameProp = serializedObject.FindProperty("iLog");
             _isReleaseModeProp = serializedObject.FindProperty("isReleaseMode");
             _playModeProp = serializedObject.FindProperty("playMode");
             _packagesInfoListProp = serializedObject.FindProperty("packagesInfoList");
             _isAutoDownloadProp = serializedObject.FindProperty("isAutoDownload");
         }

         public override void OnInspectorGUI()
         {
             serializedObject.Update();
             DrawPatchSettings();
             EditorGUILayout.Space(8);
             DrawPackagesList();
             serializedObject.ApplyModifiedProperties();
         }

         private void DrawPatchSettings()
         {
             EditorGUILayout.PropertyField(_isReleaseModeProp);
             EditorGUILayout.PropertyField(_playModeProp);

             if ((PlayMode)_playModeProp.enumValueIndex is PlayMode.HostPlayMode or PlayMode.WeChatMiniGameMode
                 or PlayMode.TTMiniGameMode or PlayMode.WebPlayMode)
             {
                 EditorGUILayout.PropertyField(_isAutoDownloadProp,
                     new GUIContent("Auto Download", "检测到有资源需要更新时，是否自动开始下载；关闭则需由 NeedUpdateListener 手动确认。"));
             }

             TypeSelectorUtility.DrawTypePopup(
                 _logNameProp,
                 typeof(ILog),
                 "ILog",
                 false,
                 true,
                 "You must select a Logger"
             );
         }

         private void DrawPackagesList()
         {
             var playMode = (PlayMode)_playModeProp.enumValueIndex;
             EditorGUILayout.LabelField("App Packages Info", EditorStyles.boldLabel);

             var bundleCollectorSetting = BundleCollectorSettingData.Setting;

             var oldSize = _packagesInfoListProp.arraySize;
             _packagesInfoListProp.arraySize = bundleCollectorSetting.Packages.Count;

             // 列表扩容时 Unity 不会应用 C# 字段初始值，需手动补默认 timeout
             for (int i = oldSize; i < _packagesInfoListProp.arraySize; i++)
             {
                 var newElement = _packagesInfoListProp.GetArrayElementAtIndex(i);
                 newElement.FindPropertyRelative("timeout").intValue = DefaultTimeout;
                 newElement.FindPropertyRelative("downloadingMaxNum").intValue = DefaultDownloadingMaxNum;
                 newElement.FindPropertyRelative("failedTryAgainTimes").intValue = DefaultFailedTryAgainTimes;
             }

             for (int i = 0; i < _packagesInfoListProp.arraySize; i++)
             {
                 SerializedProperty element = _packagesInfoListProp.GetArrayElementAtIndex(i);
                 EditorGUILayout.BeginVertical("box");
                 element.isExpanded = EditorGUILayout.Foldout(
                     element.isExpanded,
                     $"Package {bundleCollectorSetting.Packages[i].PackageName}",
                     true
                 );
                 if (element.isExpanded)
                 {
                     EditorGUI.indentLevel++;
                     var packageNameProp = element.FindPropertyRelative("packageName");
                     var packageName = bundleCollectorSetting.Packages[i].PackageName;
                     packageNameProp.stringValue = packageName;

                     EditorGUI.BeginDisabledGroup(true);
                     EditorGUILayout.TextField("Package Name", packageName);
                     EditorGUI.EndDisabledGroup();
                     // 只在 HostPlayMode 下显示
                     if (playMode is PlayMode.HostPlayMode)
                     {
                         var weakOnlineProp = element.FindPropertyRelative("isSupportWeakOnline");
                         EditorGUILayout.PropertyField(weakOnlineProp);
                     }

                     EditorGUILayout.Space(4);

                     if (playMode is PlayMode.HostPlayMode or PlayMode.WeChatMiniGameMode or PlayMode.TTMiniGameMode
                         or PlayMode.WebPlayMode)
                     {
                         EditorGUILayout.LabelField("Load Manifest Timeout:", EditorStyles.boldLabel);
                         var timeoutProp = element.FindPropertyRelative("timeout");
                         if (timeoutProp.intValue <= 0)
                             timeoutProp.intValue = DefaultTimeout;
                         EditorGUILayout.PropertyField(timeoutProp);
                         EditorGUILayout.Space(4);
                     }

                     if (playMode is PlayMode.HostPlayMode or PlayMode.WeChatMiniGameMode or PlayMode.TTMiniGameMode
                         or PlayMode.WebPlayMode)
                     {
                         var remoteServiceProp = element.FindPropertyRelative("remoteService");
                         TypeSelectorUtility.DrawTypePopup(
                             remoteServiceProp,
                             typeof(IRemoteService),
                             "Remote Service",
                             true,
                             true,
                             "You must select a remote service"
                         );
                         EditorGUILayout.Space(4);
                     }

                     var decryptorProp = element.FindPropertyRelative("bundleDecryptor");
                     TypeSelectorUtility.DrawTypePopup(
                         decryptorProp,
                         typeof(IBundleDecryptor),
                         "Bundle Decryptor"
                     );

                     EditorGUILayout.Space(4);

                     DrawClearCacheBundleInfo(element, packageName);

                     if (playMode is PlayMode.HostPlayMode or PlayMode.WeChatMiniGameMode or PlayMode.TTMiniGameMode)
                     {
                         DrawDownloadTags(element, packageName);
                     }

                     EditorGUI.indentLevel--;
                 }

                 EditorGUILayout.EndVertical();
                 EditorGUILayout.Space(4);
             }
         }

         private void DrawClearCacheBundleInfo(SerializedProperty fatherProperty, string packageName)
         {
             EditorGUILayout.BeginVertical("box");
             EditorGUILayout.LabelField("Package Clear Cache Setting", EditorStyles.boldLabel);
             EditorGUILayout.Space(2);
             var clearCacheBundleInfoProperty = fatherProperty.FindPropertyRelative("clearCacheBundleInfo");

             var modeProp = clearCacheBundleInfoProperty.FindPropertyRelative("mode");
             EditorGUILayout.PropertyField(modeProp);

             var mode = (ClearCacheOperationMode)modeProp.enumValueIndex;
             if (mode == ClearCacheOperationMode.ClearBundleFilesByLocations)
             {
                 var locationsProp = clearCacheBundleInfoProperty.FindPropertyRelative("locations");
                 EditorGUILayout.PropertyField(locationsProp);
             }
             else if (mode == ClearCacheOperationMode.ClearBundleFilesByTags)
             {
                 var tagsProp = clearCacheBundleInfoProperty.FindPropertyRelative("tags");
                 var availableTags = BundleCollectorSettingData.Setting.GetPackageAllTags(packageName);
                 TagListDrawer.Draw(tagsProp, availableTags, "Tags");
             }

             EditorGUILayout.EndVertical();
         }

         private void DrawDownloadTags(SerializedProperty element, string packageName)
         {
             EditorGUILayout.BeginVertical("box");

             var downloadingMaxNumProp = element.FindPropertyRelative("downloadingMaxNum");
             if (downloadingMaxNumProp.intValue <= 0)
                 downloadingMaxNumProp.intValue = DefaultDownloadingMaxNum;
             EditorGUILayout.PropertyField(downloadingMaxNumProp);

             var failedTryAgainTimesProp = element.FindPropertyRelative("failedTryAgainTimes");
             if (failedTryAgainTimesProp.intValue <= 0)
                 failedTryAgainTimesProp.intValue = DefaultFailedTryAgainTimes;
             EditorGUILayout.PropertyField(failedTryAgainTimesProp);

             var isCombineDownloaderProp = element.FindPropertyRelative("isCombineDownloader");
             EditorGUILayout.PropertyField(isCombineDownloaderProp);

             var tagsProp = element.FindPropertyRelative("downloadTags");
             var availableTags = BundleCollectorSettingData.Setting.GetPackageAllTags(packageName);
             TagListDrawer.Draw(
                 tagsProp,
                 availableTags,
                 "Download Tags");
             EditorGUILayout.EndVertical();
         }
     }
 }
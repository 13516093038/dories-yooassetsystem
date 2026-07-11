using Dories.yooassetsystem.Editor.Utilities;
using Dories.YooassetSystem.Runtime.AssetLoader;
using Dories.YooAssetSystem.Runtime.LogSystem;
using UnityEditor;

namespace Dories.YooAssetSystem.Editor.Inspector
{
    [CustomEditor(typeof(AssetLoaderEntity))]
    public class AssetLoaderEntityInspector : UnityEditor.Editor
    {
        private SerializedProperty _logNameProp;
        

        private void OnEnable()
        {
            _logNameProp = serializedObject.FindProperty("ILog");
        }

        public override void OnInspectorGUI()
        {
            TypeSelectorUtility.DrawTypePopup(
                _logNameProp,
                typeof(ILog),
                "ILog",
                false,
                true,
                "You must select a Logger"
            );
        }
    }
}
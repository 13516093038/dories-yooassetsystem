using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dories.YooassetSystem.Patch.Editor.Inspector
{
    /// <summary>
    /// 从 YooAsset 收集器 Tag 列表中选择，样式接近 Unity 默认 Array，且不允许重复。
    /// </summary>
    public static class TagListDrawer
    {
        private static readonly GUIContent PlusIcon = EditorGUIUtility.IconContent("Toolbar Plus");
        private static readonly GUIContent MinusIcon = EditorGUIUtility.IconContent("Toolbar Minus");

        public static void Draw(
            SerializedProperty tagsProp,
            IList<string> availableTags,
            string label,
            string helpMessage = null)
        {
            tagsProp.isExpanded = EditorGUILayout.Foldout(
                tagsProp.isExpanded,
                $"{label} ({tagsProp.arraySize})",
                true);

            if (!tagsProp.isExpanded)
                return;

            EditorGUI.indentLevel++;

            if (!string.IsNullOrEmpty(helpMessage))
                EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

            if (availableTags == null || availableTags.Count == 0)
            {
                EditorGUILayout.HelpBox("该 Package 在收集器中未配置任何 Tag", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            if (tagsProp.arraySize == 0)
            {
                 EditorGUILayout.HelpBox("未指定Tag，将下载整包资源", MessageType.Info);
            }

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                var tagProp = tagsProp.GetArrayElementAtIndex(i);
                var currentTag = tagProp.stringValue;
                var rowOptions = GetTagOptionsForRow(availableTags, tagsProp, i);

                if (rowOptions.Count == 0)
                {
                    EditorGUILayout.HelpBox($"Element {i}: 没有可选 Tag", MessageType.Warning);
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                if (!string.IsNullOrEmpty(currentTag) && !availableTags.Contains(currentTag))
                {
                    EditorGUILayout.LabelField($"Element {i}", currentTag, EditorStyles.helpBox);
                }
                else
                {
                    int currentIndex = string.IsNullOrEmpty(currentTag)
                        ? 0
                        : Mathf.Max(0, rowOptions.IndexOf(currentTag));
                    int newIndex = EditorGUILayout.Popup($"Element {i}", currentIndex, rowOptions.ToArray());
                    tagProp.stringValue = rowOptions[newIndex];
                }

                if (GUILayout.Button(MinusIcon, GUILayout.Width(24)))
                {
                    tagsProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            var unusedTags = GetUnusedTags(availableTags, tagsProp);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(unusedTags.Count == 0))
            {
                if (GUILayout.Button(PlusIcon, GUILayout.Width(24)))
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = unusedTags[0];
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private static List<string> GetUnusedTags(IList<string> availableTags, SerializedProperty tagsProp)
        {
            var selected = new HashSet<string>();
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                var value = tagsProp.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrEmpty(value))
                    selected.Add(value);
            }

            return availableTags.Where(tag => !selected.Contains(tag)).ToList();
        }

        private static List<string> GetTagOptionsForRow(
            IList<string> availableTags,
            SerializedProperty tagsProp,
            int rowIndex)
        {
            var currentTag = tagsProp.GetArrayElementAtIndex(rowIndex).stringValue;
            var others = new HashSet<string>();
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (i == rowIndex)
                    continue;

                var value = tagsProp.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrEmpty(value))
                    others.Add(value);
            }

            var options = new List<string>();
            foreach (var tag in availableTags)
            {
                if (tag == currentTag || !others.Contains(tag))
                    options.Add(tag);
            }

            if (!string.IsNullOrEmpty(currentTag)
                && !availableTags.Contains(currentTag)
                && !options.Contains(currentTag))
            {
                options.Insert(0, currentTag);
            }

            return options;
        }
    }
}

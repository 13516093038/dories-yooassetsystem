using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dories.yooassetsystem.Editor.Utilities
{
    public static class TypeSelectorUtility
    {
        /// <summary>
        /// 获取某接口/基类的所有非抽象实现类
        /// </summary>
        public static List<Type> GetImplementations(Type baseType)
        {
            return TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .OrderBy(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// 绘制类型下拉框，选中的类名写入 stringProp
        /// </summary>
        public static void DrawTypePopup(
            SerializedProperty stringProp,
            Type interfaceType,
            string label,
            bool allowNone = true,
            bool required = false,
            string requiredMessage = "This field is required")
        {
            var types = GetImplementations(interfaceType);

            // 下拉显示名
            var options = new List<string>();
            if (allowNone) options.Add("[None]");

            foreach (var t in types)
                options.Add(t.FullName); // 也可改成 t.FullName

            // 当前选中项
            int currentIndex = 0;
            if (!string.IsNullOrEmpty(stringProp.stringValue))
            {
                int idx = types.FindIndex(t => t.FullName == stringProp.stringValue);
                currentIndex = allowNone ? (idx >= 0 ? idx + 1 : 0) : Mathf.Max(idx, 0);
            }

            int newIndex = EditorGUILayout.Popup(label, currentIndex, options.ToArray());

            if (allowNone)
                stringProp.stringValue = newIndex <= 0 ? string.Empty : types[newIndex - 1].FullName;
            else if (types.Count > 0)
                stringProp.stringValue = types[newIndex].FullName;

            // 未选择且必填 → 红色错误提示框
            bool isMissing = required && string.IsNullOrEmpty(stringProp.stringValue);
            if (isMissing)
            {
                EditorGUILayout.HelpBox(requiredMessage, MessageType.Error);
            }
        }
    }
}
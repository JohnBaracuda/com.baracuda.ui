using System;
using System.Linq;
using UnityEngine;

namespace Baracuda.UI.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(UIGroup))]
    public class UIGroupReferenceDrawer : UnityEditor.PropertyDrawer
    {
        private string[] _options;

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            _options ??= UIGroup.Registry.Keys.Values.ToArray();
            var valueProperty = property.FindPropertyRelative("value");

            var selectedIndex = KeyToIndex(valueProperty.intValue);

            var lastIndex = selectedIndex;
            selectedIndex = UnityEditor.EditorGUI.Popup(position, selectedIndex, _options);

            if (lastIndex != selectedIndex)
            {
                valueProperty.intValue = IndexToKey(selectedIndex);
                valueProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private int KeyToIndex(int keyValue)
        {
            foreach (var (key, name) in UIGroup.Registry.Keys)
            {
                if (keyValue == key)
                {
                    return Array.IndexOf(_options, name);
                }
            }

            return -1;
        }

        private int IndexToKey(int index)
        {
            var displayName = _options[index];

            foreach (var (key, name) in UIGroup.Registry.Keys)
            {
                if (displayName == name)
                {
                    return key;
                }
            }

            return -1;
        }
    }
}
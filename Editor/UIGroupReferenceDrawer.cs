using System;
using System.Linq;
using UnityEngine;

namespace Baracuda.UI.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(UIGroupReference))]
    public class UIGroupReferenceDrawer : UnityEditor.PropertyDrawer
    {
        private string[] _options;
        private int _selectedIndex = -1;

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            _options ??= UIGroupReference.Registry.Keys.Values.ToArray();
            var valueProperty = property.FindPropertyRelative("value");

            if (_selectedIndex == -1)
            {
                _selectedIndex = KeyToIndex(valueProperty.intValue);
            }

            var lastIndex = _selectedIndex;
            _selectedIndex = UnityEditor.EditorGUI.Popup(position, _selectedIndex, _options);

            if (lastIndex != _selectedIndex)
            {
                valueProperty.intValue = IndexToKey(_selectedIndex);
                valueProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private int KeyToIndex(int keyValue)
        {
            foreach (var (key, name) in UIGroupReference.Registry.Keys)
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

            foreach (var (key, name) in UIGroupReference.Registry.Keys)
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
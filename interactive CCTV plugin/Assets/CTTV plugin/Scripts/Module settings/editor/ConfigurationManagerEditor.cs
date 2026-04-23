#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Surveillance.Settings
{[CustomEditor(typeof(ConfigurationManager))]
    public class ConfigurationManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ConfigurationManager manager = (ConfigurationManager)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Применить настройки ко всем модулям", GUILayout.Height(30)))
            {
                manager.ApplyConfiguration();
            }
        }
    }
}
#endif
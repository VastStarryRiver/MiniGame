using Invariable;
using UnityEditor;
using UnityEngine;



namespace MyTools
{
    [CustomEditor(typeof(UIButton))]
    public class UIButtonEditor : Editor
    {
        private const string DefaultAudioPath = "Assets/GameAssets/Audios/Sfx/defaultBtn.mp3";
        private const string UserModifiedKeyPrefix = "UIButton_AudioClip_UserModified_";



        private void OnEnable()
        {
            SerializedProperty audioClip = serializedObject.FindProperty("m_audioClip");

            if (audioClip == null)
            {
                return;
            }

            if (EditorPrefs.GetBool(GetUserModifiedKey(), false))
            {
                return;
            }

            if (audioClip.objectReferenceValue == null)
            {
                AudioClip defaultClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultAudioPath);

                if (defaultClip != null)
                {
                    audioClip.objectReferenceValue = defaultClip;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }



        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty isNotChangeScale = serializedObject.FindProperty("m_isNotChangeScale");
            EditorGUILayout.PropertyField(isNotChangeScale);

            if (!isNotChangeScale.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_scaleType"));
            }

            SerializedProperty audioClip = serializedObject.FindProperty("m_audioClip");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(audioClip);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(GetUserModifiedKey(), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_clickEvent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_doubleClickEvent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_downEvent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_upEvent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_longPressEvent"));

            serializedObject.ApplyModifiedProperties();
        }



        /// <summary>
        /// 按目标对象生成 EditorPrefs key，重启后仍识别是否手动改过音频
        /// </summary>
        private string GetUserModifiedKey()
        {
            GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(target);

            return UserModifiedKeyPrefix + globalId.ToString();
        }
    }
}
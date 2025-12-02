using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioController))]
public class AudioControllerEditor : Editor
{
    private SerializedProperty soundboardData;

    // Temporär AudioSource för preview med volym
    private AudioSource previewSource;

    private void OnEnable()
    {
        soundboardData = serializedObject.FindProperty("soundboardData");
    }

    private void OnDisable()
    {
        StopPreviewSource();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header + global Stop-knapp
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Soundboard Data", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Stop All", GUILayout.Width(80)))
            StopPreviewSource();
        EditorGUILayout.EndHorizontal();

        // Loopa igenom elementen
        for (int i = 0; i < soundboardData.arraySize; i++)
        {
            SerializedProperty element = soundboardData.GetArrayElementAtIndex(i);

            // Header med foldout + remove
            EditorGUILayout.BeginHorizontal();
            element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, "Sound ID " + i, true);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("-", GUILayout.Width(22)))
            {
                RemoveElementAt(i);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUILayout.EndHorizontal();

            // Innehållet
            if (element.isExpanded)
            {
                
                var clipProp = element.FindPropertyRelative("sound");
                var audioSourceProp = element.FindPropertyRelative("audioSource");
                var volumeProp = element.FindPropertyRelative("volume");
                var descProp = element.FindPropertyRelative("description");

                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(audioSourceProp, new GUIContent("AudioSource"));
                EditorGUILayout.PropertyField(clipProp, new GUIContent("Sound"));

                // Play / Stop
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                using (new EditorGUI.DisabledScope(clipProp.objectReferenceValue == null))
                {
                    if (GUILayout.Button("▶", GUILayout.Width(40)))
                    {
                        var clip = clipProp.objectReferenceValue as AudioClip;
                        PlayClipWithVolume(clip, volumeProp.floatValue);
                    }
                    if (GUILayout.Button("■", GUILayout.Width(40)))
                    {
                        StopPreviewSource();
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Volume slider 0-1
                volumeProp.floatValue = EditorGUILayout.Slider("Volume", volumeProp.floatValue, 0f, 1f);

                EditorGUILayout.PropertyField(descProp, new GUIContent("Description"));

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        // Add-knapp
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Element", GUILayout.Width(120)))
        {
            AddElement();
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void AddElement()
    {
        int insertIndex = soundboardData.arraySize;
        soundboardData.InsertArrayElementAtIndex(insertIndex);
        var newElem = soundboardData.GetArrayElementAtIndex(insertIndex);

        newElem.FindPropertyRelative("sound").objectReferenceValue = null;
        newElem.FindPropertyRelative("volume").floatValue = 1f; // default 1
        newElem.FindPropertyRelative("description").stringValue = string.Empty;
    }

    private void RemoveElementAt(int index)
    {
        var element = soundboardData.GetArrayElementAtIndex(index);
        soundboardData.DeleteArrayElementAtIndex(index);
        if (element.objectReferenceValue != null)
            soundboardData.DeleteArrayElementAtIndex(index);
    }

    // ---------- Audio preview med volym ----------
    private void PlayClipWithVolume(AudioClip clip, float volume)
    {
        if (clip == null) return;

        StopPreviewSource();

        var go = new GameObject("EditorAudioPreview");
        go.hideFlags = HideFlags.HideAndDontSave;
        previewSource = go.AddComponent<AudioSource>();
        previewSource.clip = clip;
        previewSource.volume = Mathf.Clamp01(volume);
        previewSource.playOnAwake = false;
        previewSource.Play();

        // Starta editor update-loop för att kolla när klippet är klart
        EditorApplication.update += CheckClipFinished;
    }

    private void CheckClipFinished()
    {
        if (previewSource == null || !previewSource.isPlaying)
        {
            StopPreviewSource();
            EditorApplication.update -= CheckClipFinished;
        }
    }

    private void StopPreviewSource()
    {
        if (previewSource != null)
        {
            previewSource.Stop();
            DestroyImmediate(previewSource.gameObject);
            previewSource = null;
        }
    }
}

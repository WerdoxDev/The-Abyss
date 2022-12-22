using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(WaterGenerator))]
public class WaterGeneratorEditor : Editor
{
    private SerializedProperty _waterMeshes;
    private SerializedProperty _waterMaterial;
    private SerializedProperty _size;
    private SerializedProperty _gridSize;

    private void OnEnable()
    {
        _waterMeshes = serializedObject.FindProperty("waterMeshes");
        _waterMaterial = serializedObject.FindProperty("waterMaterial");
        _size = serializedObject.FindProperty("size");
        _gridSize = serializedObject.FindProperty("gridSize");
    }

    public override void OnInspectorGUI()
    {
        WaterGenerator generator = (WaterGenerator)target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(_waterMeshes);
        EditorGUILayout.PropertyField(_waterMaterial);
        EditorGUILayout.PropertyField(_size);
        EditorGUILayout.PropertyField(_gridSize);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Generate")) generator.GenerateWaters();
    }
}
#endif
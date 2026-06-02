#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BSPMapManager))]
public class BSPMapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();

        BSPMapManager manager = (BSPMapManager)target;

        GUILayout.Space(15);

        
        GUI.backgroundColor = Color.cyan;

        
        if (GUILayout.Button("Regenerar Mapa", GUILayout.Height(40)))
        {
            manager.MapGeneration();
        }
    }
}
#endif

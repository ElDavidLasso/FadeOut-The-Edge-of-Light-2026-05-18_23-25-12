#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BSPMapManager))]
public class BSPMapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Dibuja las variables normales del script (Dimensiones, prefabs, etc.)
        DrawDefaultInspector();

        BSPMapManager manager = (BSPMapManager)target;

        GUILayout.Space(15); // Espacio estético en la interfaz

        // Cambiamos el color del botón para que resalte en tu espacio de trabajo
        GUI.backgroundColor = Color.cyan;

        // Si se hace clic en el botón físico del Inspector
        if (GUILayout.Button("Regenerar Mapa", GUILayout.Height(40)))
        {
            manager.MapGeneration();
        }
    }
}
#endif

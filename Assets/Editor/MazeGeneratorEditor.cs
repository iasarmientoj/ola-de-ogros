using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields
        DrawDefaultInspector();

        MazeGenerator generator = (MazeGenerator)target;

        GUILayout.Space(20);
        
        // Add styling for buttons
        GUIStyle genStyle = new GUIStyle(GUI.skin.button);
        genStyle.fontSize = 14;
        genStyle.fontStyle = FontStyle.Bold;
        genStyle.normal.textColor = Color.white;

        GUIStyle clearStyle = new GUIStyle(GUI.skin.button);
        clearStyle.fontSize = 12;
        clearStyle.normal.textColor = Color.white;

        // Save original background color and set button colors
        Color originalBg = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.12f, 0.58f, 0.25f); // Soft premium green
        if (GUILayout.Button("⚡ Generar Laberinto", genStyle, GUILayout.Height(40)))
        {
            // Register undo for editor operations
            Undo.RegisterCompleteObjectUndo(generator, "Generate Labyrinth");
            generator.GenerateLabyrinth();
            
            // Mark the scene as dirty so it prompts for saving changes
            EditorUtility.SetDirty(generator);
        }

        GUILayout.Space(5);

        GUI.backgroundColor = new Color(0.7f, 0.15f, 0.15f); // Muted red
        if (GUILayout.Button("❌ Borrar Laberinto", clearStyle, GUILayout.Height(30)))
        {
            Undo.RegisterCompleteObjectUndo(generator, "Clear Labyrinth");
            generator.ClearLabyrinth();
            EditorUtility.SetDirty(generator);
        }

        // Restore original background color
        GUI.backgroundColor = originalBg;

        GUILayout.Space(10);
    }
}

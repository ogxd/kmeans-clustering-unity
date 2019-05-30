using UnityEditor;
using UnityEngine;

public enum CusteringTarget {
    Points2D,
    Points3D,
    Bounds,
}

public class CusteringWindow : EditorWindow {

    void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI() {
        //Generate GUI and set running state if button is pressed
    }

    void OnSceneGUI(SceneView sceneview) {
        //Handles calls like these
        Handles.color = Color.cyan;
        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 7, endLeft, endRight);
        Handles.Label(position, size, style);
    }
}
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public static class ARSceneBuilder
{
    const string ScenePath = "Assets/Scenes/ARScene.unity";
    const string PlanePrefabPath = "Assets/Prefabs/ARPlaneVisualizer.prefab";
    const string PlaceholderPrefabPath = "Assets/Prefabs/PlaceholderModel.prefab";

    [MenuItem("AR Tools/Build Initial AR Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Selection.activeGameObject = null;
        EditorApplication.ExecuteMenuItem("GameObject/XR/AR Session");

        Selection.activeGameObject = null;
        EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Mobile AR)");
        var xrOrigin = Selection.activeGameObject;
        if (xrOrigin == null)
        {
            Debug.LogError("Failed to create XR Origin via menu item.");
            return;
        }

        var planeManager = xrOrigin.AddComponent<ARPlaneManager>();
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        planeManager.planePrefab = CreatePlanePrefab();

        var placer = xrOrigin.AddComponent<AutoPlaceOnPlane>();
        var so = new SerializedObject(placer);
        so.FindProperty("modelPrefab").objectReferenceValue = CreatePlaceholderPrefab();
        so.ApplyModifiedProperties();

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log("AR scene build complete: " + ScenePath);
    }

    static GameObject CreatePlanePrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlanePrefabPath);
        if (existing != null)
        {
            return existing;
        }

        Directory.CreateDirectory("Assets/Prefabs");
        Selection.activeGameObject = null;
        EditorApplication.ExecuteMenuItem("GameObject/XR/AR Default Plane");
        var go = Selection.activeGameObject;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PlanePrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreatePlaceholderPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlaceholderPrefabPath);
        if (existing != null)
        {
            return existing;
        }

        Directory.CreateDirectory("Assets/Prefabs");
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "PlaceholderModel";
        cube.transform.localScale = Vector3.one * 0.2f;
        var prefab = PrefabUtility.SaveAsPrefabAsset(cube, PlaceholderPrefabPath);
        Object.DestroyImmediate(cube);
        return prefab;
    }

    static void AddSceneToBuildSettings(string path)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == path)
            {
                return;
            }
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(path, true);
        EditorBuildSettings.scenes = newScenes;
    }
}

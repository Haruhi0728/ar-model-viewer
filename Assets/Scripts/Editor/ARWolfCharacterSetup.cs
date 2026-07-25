using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ARWolfCharacterSetup
{
    const string FbxPath = "Assets/Models/Wolf/WOLF_DEMO.fbx";
    const string TexturePath = "Assets/Models/Wolf/Textures/T_Wolf.png";
    const string ControllerPath = "Assets/Prefabs/WolfAnimatorController.controller";
    const string CharacterPrefabPath = "Assets/Prefabs/WolfCharacter.prefab";
    const string ScenePath = "Assets/Scenes/ARScene.unity";

    [MenuItem("AR Tools/Build Wolf Character")]
    public static void Build()
    {
        FixMaterial();
        var avatar = ConfigureGeneric();

        var walkClip = FindAsset<AnimationClip>(FbxPath, "Walk");
        if (walkClip == null)
        {
            Debug.LogError("Could not find Walk clip in " + FbxPath);
            return;
        }

        var controller = BuildAnimatorController(walkClip);
        var prefab = BuildCharacterPrefab(avatar, controller);

        AssignToScene(prefab);

        Debug.Log("Wolf character setup complete: " + CharacterPrefabPath);
    }

    static void FixMaterial()
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);

        var texDir = "Assets/Models/Wolf/ExtractedTextures";
        Directory.CreateDirectory(texDir);
        importer.ExtractTextures(texDir);

        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.SaveAndReimport();

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(FbxPath))
        {
            if (asset is Material mat && mat.HasProperty("_MainTex") && mat.mainTexture == null && texture != null)
            {
                mat.mainTexture = texture;
                EditorUtility.SetDirty(mat);
            }
        }

        var matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Models/Wolf" });
        foreach (var guid in matGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.HasProperty("_MainTex") && mat.mainTexture == null && texture != null)
            {
                mat.mainTexture = texture;
                EditorUtility.SetDirty(mat);
            }
        }

        AssetDatabase.SaveAssets();
    }

    static Avatar ConfigureGeneric()
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        var clips = importer.defaultClipAnimations;
        var walkIndex = -1;
        for (var i = 0; i < clips.Length; i++)
        {
            if (clips[i].name.ToLowerInvariant().Contains("walk"))
            {
                walkIndex = i;
                break;
            }
        }

        if (walkIndex < 0)
        {
            Debug.LogError("Could not find a clip with 'walk' in its name. Found: " + string.Join(", ", System.Array.ConvertAll(clips, c => c.name)));
        }
        else
        {
            clips[walkIndex].name = "Walk";
            clips[walkIndex].loopTime = true;
            importer.clipAnimations = new[] { clips[walkIndex] };
        }

        importer.SaveAndReimport();

        return FindAsset<Avatar>(FbxPath, null);
    }

    static T FindAsset<T>(string path, string name) where T : Object
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is T typed && (name == null || typed.name == name))
            {
                return typed;
            }
        }
        return null;
    }

    static AnimatorController BuildAnimatorController(AnimationClip walkClip)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
        }

        Directory.CreateDirectory("Assets/Prefabs");
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);

        var stateMachine = controller.layers[0].stateMachine;
        var walkState = stateMachine.AddState("Walk");
        walkState.motion = walkClip;
        stateMachine.defaultState = walkState;

        return controller;
    }

    static GameObject BuildCharacterPrefab(Avatar avatar, AnimatorController controller)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);

        var animator = instance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }
        animator.avatar = avatar;
        animator.runtimeAnimatorController = controller;

        var sensorController = instance.GetComponent<SensorAvatarController>();
        if (sensorController == null)
        {
            sensorController = instance.AddComponent<SensorAvatarController>();
        }

        var so = new SerializedObject(sensorController);
        so.FindProperty("idleAnimatorSpeed").floatValue = 0f;
        so.ApplyModifiedProperties();

        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, CharacterPrefabPath);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    static void AssignToScene(GameObject characterPrefab)
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found in ARScene.");
            return;
        }

        var placer = xrOrigin.GetComponent<AutoPlaceOnPlane>();
        if (placer != null)
        {
            var so = new SerializedObject(placer);
            so.FindProperty("modelPrefab").objectReferenceValue = characterPrefab;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}

using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ARCharacterSetup
{
    const string WalkFbxPath = "Assets/Models/Character/WALK5.fbx";
    const string IdleFbxPath = "Assets/Models/Character/idle5.fbx";
    const string ControllerPath = "Assets/Prefabs/CharacterAnimatorController.controller";
    const string CharacterPrefabPath = "Assets/Prefabs/WalkingCharacter.prefab";
    const string ScenePath = "Assets/Scenes/ARScene.unity";

    [MenuItem("AR Tools/Build Walking Character")]
    public static void Build()
    {
        var walkAvatar = ConfigureHumanoid(WalkFbxPath, "Walk", null);
        ConfigureHumanoid(IdleFbxPath, "Idle", walkAvatar);

        var walkClip = FindAsset<AnimationClip>(WalkFbxPath, "Walk");
        var idleClip = FindAsset<AnimationClip>(IdleFbxPath, "Idle");

        if (walkClip == null || idleClip == null)
        {
            Debug.LogError($"Could not find clips. walkClip={walkClip}, idleClip={idleClip}");
            return;
        }

        var controller = BuildAnimatorController(idleClip, walkClip);
        var prefab = BuildCharacterPrefab(walkAvatar, controller);

        AssignToScene(prefab);

        Debug.Log("Walking character setup complete: " + CharacterPrefabPath);
    }

    static Avatar ConfigureHumanoid(string fbxPath, string clipName, Avatar sharedAvatar)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(fbxPath);
        importer.animationType = ModelImporterAnimationType.Generic;

        if (sharedAvatar == null)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        }
        else
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = sharedAvatar;
        }

        var clips = importer.defaultClipAnimations;
        if (clips.Length > 0)
        {
            clips[0].name = clipName;
            clips[0].loopTime = true;
            importer.clipAnimations = clips;
        }

        importer.SaveAndReimport();

        return FindAsset<Avatar>(fbxPath, null);
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

    static AnimatorController BuildAnimatorController(AnimationClip idleClip, AnimationClip walkClip)
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
        var idleState = stateMachine.AddState("Idle");
        idleState.motion = idleClip;
        var walkState = stateMachine.AddState("Walk");
        walkState.motion = walkClip;

        stateMachine.defaultState = idleState;

        var toWalk = idleState.AddTransition(walkState);
        toWalk.hasExitTime = false;
        toWalk.duration = 0.15f;
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");

        var toIdle = walkState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration = 0.15f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");

        return controller;
    }

    static GameObject BuildCharacterPrefab(Avatar avatar, AnimatorController controller)
    {
        var walkModel = AssetDatabase.LoadAssetAtPath<GameObject>(WalkFbxPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(walkModel);

        var animator = instance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }
        animator.avatar = avatar;
        animator.runtimeAnimatorController = controller;

        if (instance.GetComponent<SensorAvatarController>() == null)
        {
            instance.AddComponent<SensorAvatarController>();
        }

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

        var bridgeGo = GameObject.Find("SensorBridge");
        if (bridgeGo == null)
        {
            bridgeGo = new GameObject("SensorBridge", typeof(BendSensorClient));
        }
        else if (bridgeGo.GetComponent<BendSensorClient>() == null)
        {
            bridgeGo.AddComponent<BendSensorClient>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}

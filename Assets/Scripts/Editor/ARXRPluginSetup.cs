using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

public static class ARXRPluginSetup
{
    const string SettingsKey = "com.unity.xr.management.loader_settings";

    [MenuItem("AR Tools/Configure XR Plug-in Management")]
    public static void Configure()
    {
        var settingsAsset = GetOrCreateSettingsAsset();

        ConfigureBuildTarget(settingsAsset, BuildTargetGroup.iOS, "UnityEngine.XR.ARKit.ARKitLoader");
        ConfigureBuildTarget(settingsAsset, BuildTargetGroup.Android, "UnityEngine.XR.ARCore.ARCoreLoader");

        PlayerSettings.iOS.cameraUsageDescription = "3Dモデルを配置するためにカメラを使用します。";
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        AssetDatabase.SaveAssets();
        Debug.Log("XR Plug-in Management configuration complete.");
    }

    static XRGeneralSettingsPerBuildTarget GetOrCreateSettingsAsset()
    {
        EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(SettingsKey, out var settingsAsset);
        if (settingsAsset != null)
        {
            return settingsAsset;
        }

        var assets = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
        if (assets.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(assets[0]);
            settingsAsset = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
            if (settingsAsset != null)
            {
                return settingsAsset;
            }
        }

        settingsAsset = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
        System.IO.Directory.CreateDirectory("Assets/XR/Settings");
        AssetDatabase.CreateAsset(settingsAsset, "Assets/XR/Settings/XRGeneralSettingsPerBuildTarget.asset");
        AssetDatabase.SaveAssets();
        EditorBuildSettings.AddConfigObject(SettingsKey, settingsAsset, true);
        return settingsAsset;
    }

    static void ConfigureBuildTarget(XRGeneralSettingsPerBuildTarget settingsAsset, BuildTargetGroup group, string loaderTypeName)
    {
        if (!settingsAsset.HasSettingsForBuildTarget(group))
        {
            settingsAsset.CreateDefaultSettingsForBuildTarget(group);
        }

        if (!settingsAsset.HasManagerSettingsForBuildTarget(group))
        {
            settingsAsset.CreateDefaultManagerSettingsForBuildTarget(group);
        }

        var managerSettings = settingsAsset.ManagerSettingsForBuildTarget(group);
        var didAssign = XRPackageMetadataStore.AssignLoader(managerSettings, loaderTypeName, group);
        if (!didAssign)
        {
            Debug.LogError($"Failed to assign {loaderTypeName} for {group}.");
        }
    }
}

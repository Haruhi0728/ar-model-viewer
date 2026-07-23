using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ARGraphicsApiFix
{
    [MenuItem("AR Tools/Fix Android Graphics API for ARCore")]
    public static void Fix()
    {
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

        AssetDatabase.SaveAssets();
        Debug.Log("Android Graphics API set to OpenGLES3 only (Vulkan removed for ARCore compatibility).");
    }
}

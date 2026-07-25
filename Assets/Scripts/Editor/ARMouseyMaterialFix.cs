using System.IO;
using UnityEditor;
using UnityEngine;

public static class ARMouseyMaterialFix
{
    [MenuItem("AR Tools/Fix Mousey Materials")]
    public static void Fix()
    {
        FixOne("Assets/Models/Mousey/MouseyWalk.fbx");
        FixOne("Assets/Models/Mousey/MouseyIdle.fbx");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Mousey materials/textures extracted.");
    }

    static void FixOne(string fbxPath)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(fbxPath);

        var texDir = "Assets/Models/Mousey/Textures";
        Directory.CreateDirectory(texDir);
        importer.ExtractTextures(texDir);

        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.SaveAndReimport();
    }
}

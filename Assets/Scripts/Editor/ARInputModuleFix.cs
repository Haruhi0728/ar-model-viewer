using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class ARInputModuleFix
{
    [MenuItem("AR Tools/Fix EventSystem Input Module")]
    public static void Fix()
    {
        var es = Object.FindObjectOfType<EventSystem>();
        if (es == null)
        {
            Debug.LogError("No EventSystem found in the scene.");
            return;
        }

        var standalone = es.GetComponent<StandaloneInputModule>();
        if (standalone != null)
        {
            Object.DestroyImmediate(standalone);
        }

        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            es.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("EventSystem input module fixed for new Input System.");
    }
}

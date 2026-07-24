using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ARResetButtonSetup
{
    [MenuItem("AR Tools/Add Reset Button To Scene")]
    public static void AddResetButton()
    {
        var xrOriginGo = GameObject.Find("XR Origin");
        if (xrOriginGo == null)
        {
            Debug.LogError("XR Origin not found. Open ARScene.unity first.");
            return;
        }

        var placer = xrOriginGo.GetComponent<AutoPlaceOnPlane>();
        if (placer == null)
        {
            Debug.LogError("AutoPlaceOnPlane component not found on XR Origin.");
            return;
        }

        if (GameObject.Find("ResetButtonCanvas") != null)
        {
            Debug.Log("Reset button already exists in the scene.");
            return;
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
        }

        var canvasGo = new GameObject("ResetButtonCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Reset Canvas");

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        var buttonGo = new GameObject("ResetButton", typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(canvasGo.transform, false);

        var buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0, 120);
        buttonRect.sizeDelta = new Vector2(320, 130);

        var image = buttonGo.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.85f);

        var button = buttonGo.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, placer.ResetPlacement);

        var textGo = new GameObject("Text", typeof(Text));
        textGo.transform.SetParent(buttonGo.transform, false);

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.GetComponent<Text>();
        text.text = "リセット";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = 48;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Reset button added. Save the scene (Ctrl+S) to keep it.");
    }
}

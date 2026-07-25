using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class ARUIFeaturesSetup
{
    const string ScenePath = "Assets/Scenes/ARScene.unity";

    [MenuItem("AR Tools/Add Pinch Zoom And Hamburger Menu")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var xrOrigin = GameObject.Find("XR Origin");
        var placer = xrOrigin != null ? xrOrigin.GetComponent<AutoPlaceOnPlane>() : null;
        if (placer == null)
        {
            Debug.LogError("AutoPlaceOnPlane not found on XR Origin. Open ARScene.unity first.");
            return;
        }

        if (xrOrigin.GetComponent<PinchToScaleController>() == null)
        {
            xrOrigin.AddComponent<PinchToScaleController>();
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        if (GameObject.Find("MenuCanvas") == null)
        {
            BuildMenuCanvas(placer);
        }
        else
        {
            Debug.Log("MenuCanvas already exists, skipping UI creation.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Pinch zoom and hamburger menu setup complete.");
    }

    static void BuildMenuCanvas(AutoPlaceOnPlane placer)
    {
        var canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        var hamburgerGo = CreateButton("HamburgerButton", canvasGo.transform, "≡", 56);
        var hamburgerRect = hamburgerGo.GetComponent<RectTransform>();
        hamburgerRect.anchorMin = new Vector2(0, 1);
        hamburgerRect.anchorMax = new Vector2(0, 1);
        hamburgerRect.pivot = new Vector2(0, 1);
        hamburgerRect.anchoredPosition = new Vector2(30, -60);
        hamburgerRect.sizeDelta = new Vector2(110, 110);

        var panelGo = new GameObject("ModelMenuPanel", typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panelGo.transform.SetParent(canvasGo.transform, false);

        var panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(30, -190);
        panelRect.sizeDelta = new Vector2(320, 0);

        var panelImage = panelGo.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        var vlg = panelGo.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 16, 16);
        vlg.spacing = 12;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;

        var fitter = panelGo.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        panelGo.SetActive(false);

        var mousey = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MouseyCharacter.prefab");
        var wolf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WolfCharacter.prefab");
        var hand = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WalkingCharacter.prefab");

        var menuControllerGo = new GameObject("ModelMenuController", typeof(ModelMenuController));
        var controller = menuControllerGo.GetComponent<ModelMenuController>();

        var so = new SerializedObject(controller);
        so.FindProperty("placer").objectReferenceValue = placer;
        so.FindProperty("menuPanel").objectReferenceValue = panelGo;
        var arrProp = so.FindProperty("modelPrefabs");
        arrProp.arraySize = 3;
        arrProp.GetArrayElementAtIndex(0).objectReferenceValue = mousey;
        arrProp.GetArrayElementAtIndex(1).objectReferenceValue = wolf;
        arrProp.GetArrayElementAtIndex(2).objectReferenceValue = hand;
        so.ApplyModifiedProperties();

        CreateMenuItemButton(panelGo.transform, "Mousey", controller, 0);
        CreateMenuItemButton(panelGo.transform, "Wolf", controller, 1);
        CreateMenuItemButton(panelGo.transform, "Hand", controller, 2);

        var hamburgerButton = hamburgerGo.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(hamburgerButton.onClick, controller.ToggleMenu);
    }

    static GameObject CreateButton(string name, Transform parent, string label, int fontSize)
    {
        var go = new GameObject(name, typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.85f);

        var textGo = new GameObject("Text", typeof(Text));
        textGo.transform.SetParent(go.transform, false);

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = fontSize;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return go;
    }

    static void CreateMenuItemButton(Transform parent, string label, ModelMenuController controller, int index)
    {
        var go = CreateButton(label + "Button", parent, label, 36);

        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 90;

        var button = go.GetComponent<Button>();
        UnityEventTools.AddIntPersistentListener(button.onClick, controller.SelectModel, index);
    }
}

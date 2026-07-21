using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MenuSceneCreator
{
    [MenuItem("Tools/Crear Escena de Menú Principal")]
    public static void CreateMenuScene()
    {
        // 1. Crear nueva escena limpia
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 2. Crear Canvas principal
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. Crear EventSystem si no existe
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 4. Fondo blanco (reemplazable fácilmente con sprite en el Inspector)
        GameObject bgObj = new GameObject("Background (Fondo)", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.color = new Color(0.95f, 0.95f, 0.95f, 1f); // Blanco neutro

        // 5. Título Principal
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(canvasObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.82f);
        titleRect.anchorMax = new Vector2(0.5f, 0.82f);
        titleRect.sizeDelta = new Vector2(900, 120);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "SURVIVAL: CLASE 20";
        titleText.fontSize = 64;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // 6. Panel de Botones Principales (Jugar, Controles, Opciones, Salir)
        GameObject mainPanel = new GameObject("MainButtonsPanel", typeof(RectTransform));
        mainPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform mainPanelRect = mainPanel.GetComponent<RectTransform>();
        mainPanelRect.anchorMin = new Vector2(0.5f, 0.42f);
        mainPanelRect.anchorMax = new Vector2(0.5f, 0.42f);
        mainPanelRect.sizeDelta = new Vector2(420, 420);

        VerticalLayoutGroup layout = mainPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button btnPlay = CreateMenuButton(mainPanel.transform, "Btn_Play", "JUGAR");
        Button btnControls = CreateMenuButton(mainPanel.transform, "Btn_Controls", "CONTROLES");
        Button btnOptions = CreateMenuButton(mainPanel.transform, "Btn_Options", "OPCIONES");
        Button btnQuit = CreateMenuButton(mainPanel.transform, "Btn_Quit", "SALIR");

        // 7. Panel de Controles del Juego
        GameObject controlsPanel = CreateSubPanel(canvasObj.transform, "ControlsPanel", "CONTROLES DEL JUEGO");
        GameObject controlsContent = new GameObject("ControlsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        controlsContent.transform.SetParent(controlsPanel.transform, false);
        RectTransform ccRect = controlsContent.GetComponent<RectTransform>();
        ccRect.anchorMin = new Vector2(0.1f, 0.25f);
        ccRect.anchorMax = new Vector2(0.9f, 0.78f);
        ccRect.offsetMin = Vector2.zero;
        ccRect.offsetMax = Vector2.zero;
        TextMeshProUGUI ccText = controlsContent.GetComponent<TextMeshProUGUI>();
        ccText.text = "<b>WASD</b> - Moverse por el mapa\n\n" +
                      "<b>Ratón</b> - Apuntar y Mirar a los lados\n\n" +
                      "<b>Clic Izquierdo</b> - Disparar\n\n" +
                      "<b>Espacio</b> - Saltar\n\n" +
                      "<b>Shift Izquierdo</b> - Correr\n\n" +
                      "<b>Tecla F</b> - Linterna (Encender / Apagar)";
        ccText.fontSize = 26;
        ccText.alignment = TextAlignmentOptions.Center;
        ccText.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        Button btnCloseControls = CreateMenuButton(controlsPanel.transform, "Btn_CloseControls", "VOLVER");
        RectTransform closeCtrlRect = btnCloseControls.GetComponent<RectTransform>();
        closeCtrlRect.anchorMin = new Vector2(0.5f, 0.12f);
        closeCtrlRect.anchorMax = new Vector2(0.5f, 0.12f);
        closeCtrlRect.sizeDelta = new Vector2(250, 60);

        // 8. Panel de Opciones (Sensibilidad y Volumen)
        GameObject optionsPanel = CreateSubPanel(canvasObj.transform, "OptionsPanel", "OPCIONES DE JUEGO");

        Slider sensSlider = CreateOptionSlider(optionsPanel.transform, "Sensibilidad Ratón", 10, 300, 100, new Vector3(0, 50, 0));
        Slider volSlider = CreateOptionSlider(optionsPanel.transform, "Volumen General", 0, 1, 1, new Vector3(0, -50, 0));

        Button btnCloseOptions = CreateMenuButton(optionsPanel.transform, "Btn_CloseOptions", "VOLVER");
        RectTransform closeOptRect = btnCloseOptions.GetComponent<RectTransform>();
        closeOptRect.anchorMin = new Vector2(0.5f, 0.12f);
        closeOptRect.anchorMax = new Vector2(0.5f, 0.12f);
        closeOptRect.sizeDelta = new Vector2(250, 60);

        // Ocultar subpaneles por defecto
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);

        // 9. GameObject MainMenuManager
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenu menuScript = managerObj.AddComponent<MainMenu>();
        menuScript.mainButtonsPanel = mainPanel;
        menuScript.controlsPanel = controlsPanel;
        menuScript.optionsPanel = optionsPanel;
        menuScript.sensitivitySlider = sensSlider;
        menuScript.volumeSlider = volSlider;
        menuScript.gameSceneName = "Juego 1";

        // Vincular los eventosOnClick
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlay.onClick, menuScript.PlayGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnControls.onClick, menuScript.OpenControls);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnCloseControls.onClick, menuScript.CloseControls);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnOptions.onClick, menuScript.OpenOptions);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnCloseOptions.onClick, menuScript.CloseOptions);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnQuit.onClick, menuScript.QuitGame);

        // 10. Guardar Escena
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
        string scenePath = "Assets/Scenes/Menu.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        // 11. Ajustar Build Settings (Menu = 0, Juego 1 = 1)
        EditorBuildSettingsScene[] newBuildScenes = new EditorBuildSettingsScene[2];
        newBuildScenes[0] = new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true);
        newBuildScenes[1] = new EditorBuildSettingsScene("Assets/Scenes/Juego 1.unity", true);
        EditorBuildSettings.scenes = newBuildScenes;

        Debug.Log("¡Escena de Menú generada con éxito en Assets/Scenes/Menu.unity y configurada en el Build Settings como Escena 0!");
    }

    private static Button CreateMenuButton(Transform parent, string name, string labelText)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.2f, 0.25f, 0.35f, 1f);

        LayoutElement layout = btnObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 70;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = labelText;
        text.fontSize = 28;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return btnObj.GetComponent<Button>();
    }

    private static GameObject CreateSubPanel(Transform parent, string name, string title)
    {
        GameObject panelObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(parent, false);
        RectTransform pRect = panelObj.GetComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.2f, 0.15f);
        pRect.anchorMax = new Vector2(0.8f, 0.85f);
        pRect.offsetMin = Vector2.zero;
        pRect.offsetMax = Vector2.zero;

        Image img = panelObj.GetComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.95f, 0.98f);

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform tRect = titleObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 0.88f);
        tRect.anchorMax = new Vector2(0.5f, 0.88f);
        tRect.sizeDelta = new Vector2(600, 60);

        TextMeshProUGUI tText = titleObj.GetComponent<TextMeshProUGUI>();
        tText.text = title;
        tText.fontSize = 36;
        tText.fontStyle = FontStyles.Bold;
        tText.alignment = TextAlignmentOptions.Center;
        tText.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        return panelObj;
    }

    private static Slider CreateOptionSlider(Transform parent, string label, float min, float max, float defaultVal, Vector3 localPos)
    {
        GameObject container = new GameObject(label + "_Container", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform cRect = container.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.anchoredPosition = localPos;
        cRect.sizeDelta = new Vector2(650, 60);

        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(container.transform, false);
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0, 0.5f);
        tRect.anchorMax = new Vector2(0.4f, 0.5f);
        tRect.sizeDelta = new Vector2(0, 50);

        TextMeshProUGUI tText = textObj.GetComponent<TextMeshProUGUI>();
        tText.text = label;
        tText.fontSize = 24;
        tText.alignment = TextAlignmentOptions.Left;
        tText.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(container.transform, false);
        RectTransform sRect = sliderObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.45f, 0.5f);
        sRect.anchorMax = new Vector2(1f, 0.5f);
        sRect.sizeDelta = new Vector2(0, 30);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultVal;

        GameObject sliderBg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        sliderBg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = sliderBg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        sliderBg.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 1f);

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform haRect = handleArea.GetComponent<RectTransform>();
        haRect.anchorMin = Vector2.zero;
        haRect.anchorMax = Vector2.one;
        haRect.offsetMin = Vector2.zero;
        haRect.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform hRect = handle.GetComponent<RectTransform>();
        hRect.sizeDelta = new Vector2(30, 40);
        handle.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);

        slider.targetGraphic = handle.GetComponent<Image>();
        slider.handleRect = hRect;

        return slider;
    }
}

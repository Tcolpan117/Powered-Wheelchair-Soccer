using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class VRMainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Game";

    [Header("XR Camera")]
    [SerializeField] private Camera headsetCamera;

    [Header("World Menu")]
    [SerializeField, Min(0.5f)] private float distanceFromHead = 2f;
    [SerializeField, Range(0.0005f, 0.004f)]
    private float menuScale = 0.0018f;

    [Header("Theme")]
    [SerializeField]
    private Color cyan =
        new Color32(40, 225, 245, 255);
    [SerializeField]
    private Color orange =
        new Color32(255, 165, 45, 255);

    private readonly Color navy = new Color32(12, 23, 42, 255);
    private readonly Color white = new Color32(240, 250, 255, 255);
    private readonly Color muted = new Color32(160, 190, 205, 255);

    private GameObject home;
    private GameObject settings;
    private GameObject exit;
    private GameObject generatedMenu;

    private TMP_Text status;
    private TMP_Text volumeLabel;
    private Button startButton;
    private RectTransform ball;
    private Vector2 ballOrigin;

    private bool loading;

    private IEnumerator Start()
    {
        if (headsetCamera == null)
            headsetCamera = Camera.main;

        if (headsetCamera == null)
        {
            Debug.LogError(
                "Assign the Main Camera from your XR Origin.", this);
            yield break;
        }

        if (TMP_Settings.defaultFontAsset == null)
        {
            Debug.LogError(
                "Import TextMeshPro Essential Resources first.", this);
            yield break;
        }

        GameSettings.Apply();
        ConfigureEventSystem();

        // Let the XR camera update before placing the world menu.
        yield return new WaitForEndOfFrame();

        BuildMenu();
    }

    private void Update()
    {
        if (ball == null)
            return;

        float time = Time.unscaledTime;
        ball.anchoredPosition =
            ballOrigin + new Vector2(0, Mathf.Sin(time * 2f) * 12f);

        ball.localRotation = Quaternion.Euler(
            0, 0, Mathf.Sin(time) * 12f);
    }

    private void ConfigureEventSystem()
    {
        EventSystem system = FindFirstObjectByType<EventSystem>();

        if (system == null)
        {
            GameObject obj = new GameObject("XR EventSystem");
            system = obj.AddComponent<EventSystem>();
        }

        foreach (BaseInputModule module
                 in system.GetComponents<BaseInputModule>())
        {
            if (!(module is XRUIInputModule))
                module.enabled = false;
        }

        XRUIInputModule xrModule =
            system.GetComponent<XRUIInputModule>();

        if (xrModule == null)
            xrModule = system.gameObject.AddComponent<XRUIInputModule>();

        xrModule.enabled = true;
        xrModule.enableXRInput = true;
    }

    private void BuildMenu()
    {
        generatedMenu = new GameObject(
            "Powerchair Soccer Menu",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(TrackedDeviceGraphicRaycaster)
        );

        RectTransform root =
            generatedMenu.GetComponent<RectTransform>();

        // No camera parent: the menu stays stationary in the world.
        root.SetParent(null);
        root.position =
            headsetCamera.transform.position
            + headsetCamera.transform.forward * distanceFromHead;

        root.rotation = headsetCamera.transform.rotation;
        root.localScale = Vector3.one * menuScale;
        root.sizeDelta = new Vector2(1400, 1000);

        Canvas canvas = generatedMenu.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = headsetCamera;
        canvas.sortingOrder = 100;

        generatedMenu.GetComponent<CanvasScaler>()
            .dynamicPixelsPerUnit = 10f;

        TrackedDeviceGraphicRaycaster xrRaycaster =
            generatedMenu.GetComponent<TrackedDeviceGraphicRaycaster>();

        // Scene colliders won't block controller UI raycasts.
        xrRaycaster.checkFor2DOcclusion = false;
        xrRaycaster.checkFor3DOcclusion = false;

        BuildPitch(root);

        // Opaque center panel for readable text.
        Box(root, "Center Panel", Vector2.zero,
            new Vector2(920, 960), navy);

        Box(root, "Top Accent", new Vector2(0, 470),
            new Vector2(920, 8), cyan);

        Box(root, "Bottom Accent", new Vector2(0, -470),
            new Vector2(920, 8), orange);

        Text(root, "POWERCHAIR", new Vector2(0, 355),
            new Vector2(880, 90), 67, white);

        Text(root, "SOCCER VR", new Vector2(0, 275),
            new Vector2(880, 95), 78, cyan);

        Text(root, "BIG PLAYS. FULL THROTTLE.",
            new Vector2(0, 195),
            new Vector2(840, 45), 25, orange);

        Box(root, "Divider", new Vector2(0, 145),
            new Vector2(760, 2), muted);

        BuildBall(root);

        home = Panel(root, "Home");
        settings = Panel(root, "Settings");
        exit = Panel(root, "Exit Confirmation");

        startButton = MenuButton(
            home.transform, "START GAME", new Vector2(0, 65),
            new Vector2(720, 105), cyan, StartGame);

        MenuButton(
            home.transform, "SETTINGS", new Vector2(0, -75),
            new Vector2(720, 105), orange,
            () => Show(settings));

        MenuButton(
            home.transform, "EXIT GAME", new Vector2(0, -215),
            new Vector2(720, 105), cyan,
            () => Show(exit));

        status = Text(
            home.transform, "Point your controller • Pull the trigger",
            new Vector2(0, -330),
            new Vector2(830, 85), 24, muted);

        BuildSettings();
        BuildExit();

        Text(root, "YOUR CHAIR. YOUR TEAM. YOUR GAME.",
            new Vector2(0, -430),
            new Vector2(850, 35), 21, orange);

        Show(home);
    }

    private void BuildPitch(Transform parent)
    {
        // Large opaque backdrop fills the view when facing the menu.
        Box(parent, "Stadium Background", Vector2.zero,
            new Vector2(8000, 6000),
            new Color32(5, 31, 30, 255));

        for (int i = -7; i <= 7; i++)
        {
            Box(parent, "Grass Stripe",
                new Vector2(i * 240, 0),
                new Vector2(240, 2500),
                i % 2 == 0
                    ? new Color32(10, 66, 51, 255)
                    : new Color32(12, 79, 57, 255));
        }

        Color line = new Color(0.7f, 1f, 0.88f, 0.45f);

        Box(parent, "Halfway Line", Vector2.zero,
            new Vector2(5, 1800), line);

        Box(parent, "Pitch Top", new Vector2(0, 850),
            new Vector2(2900, 5), line);

        Box(parent, "Pitch Bottom", new Vector2(0, -850),
            new Vector2(2900, 5), line);

        Ring(parent, Vector2.zero, 440, 5, line);

        Text(parent, "HOME", new Vector2(-650, 240),
            new Vector2(300, 60), 42, cyan);

        Text(parent, "AWAY", new Vector2(650, 240),
            new Vector2(300, 60), 42, orange);

        Text(parent, "00", new Vector2(-650, 150),
            new Vector2(300, 100), 86, white);

        Text(parent, "00", new Vector2(650, 150),
            new Vector2(300, 100), 86, white);

        Text(parent, "LET'S PLAY!", new Vector2(-650, -180),
            new Vector2(310, 100), 30, orange);

        Text(parent, "ROLL TO VICTORY", new Vector2(650, -180),
            new Vector2(310, 120), 30, cyan);
    }

    private void BuildBall(Transform parent)
    {
        ball = Rect(parent, "Animated Soccer Ball",
            new Vector2(0, 425), new Vector2(58, 58));

        ballOrigin = ball.anchoredPosition;

        // Draw a filled circle from horizontal strips.
        for (int y = -28; y <= 28; y += 2)
        {
            float width = 2f * Mathf.Sqrt(28f * 28f - y * y);
            Box(ball, "Ball Strip", new Vector2(0, y),
                new Vector2(Mathf.Max(width, 1f), 2f), white);
        }

        RectTransform patch = Box(
            ball, "Ball Patch", Vector2.zero,
            new Vector2(22, 22), navy);

        patch.localRotation = Quaternion.Euler(0, 0, 45);
    }

    private void BuildSettings()
    {
        Text(settings.transform, "MAKE IT YOUR GAME",
            new Vector2(0, 75),
            new Vector2(800, 60), 35, white);

        volumeLabel = Text(settings.transform, "",
            new Vector2(0, -5),
            new Vector2(800, 55), 30, cyan);

        RefreshVolume();

        MenuButton(settings.transform, "VOLUME -",
            new Vector2(-190, -115),
            new Vector2(340, 95), cyan,
            () => ChangeVolume(-0.1f));

        MenuButton(settings.transform, "VOLUME +",
            new Vector2(190, -115),
            new Vector2(340, 95), orange,
            () => ChangeVolume(0.1f));

        MenuButton(settings.transform, "BACK TO MENU",
            new Vector2(0, -270),
            new Vector2(720, 105), cyan,
            () => Show(home));
    }

    private void ChangeVolume(float amount)
    {
        GameSettings.MasterVolume += amount;
        GameSettings.Save();
        RefreshVolume();
    }

    private void RefreshVolume()
    {
        volumeLabel.text =
            $"MASTER VOLUME: " +
            $"{Mathf.RoundToInt(GameSettings.MasterVolume * 100f)}%";
    }

    private void BuildExit()
    {
        Text(exit.transform, "LEAVING THE PITCH?",
            new Vector2(0, 45),
            new Vector2(830, 70), 40, white);

        Text(exit.transform, "Your next match will be waiting.",
            new Vector2(0, -35),
            new Vector2(830, 55), 26, muted);

        MenuButton(exit.transform, "YES, EXIT GAME",
            new Vector2(0, -160),
            new Vector2(720, 105), orange, ExitGame);

        MenuButton(exit.transform, "KEEP PLAYING",
            new Vector2(0, -300),
            new Vector2(720, 105), cyan,
            () => Show(home));
    }

    private void Show(GameObject selected)
    {
        home.SetActive(selected == home);
        settings.SetActive(selected == settings);
        exit.SetActive(selected == exit);
    }

    private void StartGame()
    {
        if (loading)
            return;

        if (string.IsNullOrWhiteSpace(gameplaySceneName) ||
            !Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            status.text =
                "Scene missing: check Gameplay Scene Name\n" +
                "and add it to the build scene list.";
            return;
        }

        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        loading = true;
        startButton.interactable = false;
        GameSettings.Save();
        status.text = "GETTING THE PITCH READY...";

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(gameplaySceneName);

        if (operation == null)
        {
            loading = false;
            startButton.interactable = true;
            status.text = "Could not load the gameplay scene.";
            yield break;
        }

        while (!operation.isDone)
        {
            int progress = Mathf.RoundToInt(
                Mathf.Clamp01(operation.progress / 0.9f) * 100f);

            status.text = $"GETTING THE PITCH READY... {progress}%";
            yield return null;
        }
    }

    private void ExitGame()
    {
        GameSettings.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private GameObject Panel(Transform parent, string name)
    {
        return Rect(parent, name, Vector2.zero,
            new Vector2(920, 960)).gameObject;
    }

    private RectTransform Rect(
        Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return rect;
    }

    private RectTransform Box(
        Transform parent, string name,
        Vector2 position, Vector2 size, Color color)
    {
        RectTransform rect = Rect(parent, name, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;

        // Decorations must not intercept button input.
        image.raycastTarget = false;

        return rect;
    }

    private TMP_Text Text(
        Transform parent, string value,
        Vector2 position, Vector2 size, float fontSize, Color color)
    {
        RectTransform rect = Rect(parent, "Text", position, size);
        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return text;
    }

    private Button MenuButton(
        Transform parent, string label,
        Vector2 position, Vector2 size,
        Color accent, UnityAction action)
    {
        RectTransform rect = Rect(parent, label, position, size);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = true;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.interactable = true;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color32(27, 48, 68, 255);
        colors.highlightedColor = accent;
        colors.selectedColor = colors.normalColor;
        colors.pressedColor = Color.Lerp(accent, Color.white, 0.35f);
        colors.disabledColor = new Color32(45, 50, 60, 255);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        button.onClick.AddListener(action);

        Box(rect, "Accent",
            new Vector2(-size.x * 0.5f + 5f, 0),
            new Vector2(10, size.y), accent);

        // Dark label remains readable over bright hover colors.
        Text(rect, label, new Vector2(0, -2),
            size - new Vector2(35, 10), 30, Color.black);

        // Light text shadow offset gives contrast on the dark idle button.
        // Use a brighter normal background for consistent readability.
        colors = button.colors;
        colors.normalColor = Color.Lerp(accent, Color.white, 0.35f);
        colors.selectedColor = colors.normalColor;
        button.colors = colors;

        return button;
    }

    private void Ring(
        Transform parent, Vector2 center,
        float radius, float thickness, Color color)
    {
        const int segments = 64;
        float length = 2f * Mathf.PI * radius / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * 360f / segments;
            float radians = angle * Mathf.Deg2Rad;

            RectTransform segment = Box(
                parent, "Center Circle",
                center + new Vector2(
                    Mathf.Cos(radians), Mathf.Sin(radians)) * radius,
                new Vector2(length + 1f, thickness), color);

            segment.localRotation =
                Quaternion.Euler(0, 0, angle + 90f);
        }
    }

    private void OnDestroy()
    {
        if (generatedMenu != null)
            Destroy(generatedMenu);
    }
}
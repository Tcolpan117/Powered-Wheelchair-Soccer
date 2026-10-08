using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public class VRMainMenu : MonoBehaviour
{
    private const float BallBobSpeed = 2f;
    private const float BallBobHeight = 12f;
    private const float BallRockSpeed = 1f;
    private const float BallRockDegrees = 12f;

    private enum MenuScreen { Home, GameSelect, Settings }

    [System.Serializable]
    private struct ScreenEntry
    {
        public MenuScreen screen;
        public GameObject root;
    }

#if UNITY_EDITOR
    [Header("Games (drag scenes here)")]
    [SerializeField, FormerlySerializedAs("games")]
    private List<SceneAsset> gameScenes = new List<SceneAsset>();
#endif
    [SerializeField, HideInInspector] private List<string> scenePaths = new List<string>();

    [Header("Placement")]
    [SerializeField] private float distanceFromHead = 2f;
    [SerializeField] private Color32 skyColor = new Color32(5, 12, 25, 255);

    [Header("References (set by builder)")]
    [SerializeField] private Camera headsetCamera;
    [SerializeField] private Transform menuCanvas;
    [SerializeField] private Transform environment;
    [SerializeField] private RectTransform animatedBall;

    [SerializeField] private List<ScreenEntry> screens;

    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text volumeLabel;

    [SerializeField] private Transform gameButtonContainer;
    [SerializeField] private Button gameButtonTemplate;

    private Vector2 ballRestPosition;

    private IEnumerator Start()
    {
        ballRestPosition = animatedBall.anchoredPosition;
        ApplySkyColor();

        yield return new WaitForEndOfFrame();

        PlaceMenuInFrontOfPlayer();
        CreateGameButtons();
        RefreshVolumeLabel();
    }

    private void Update() => AnimateBall();

    private void ApplySkyColor()
    {
        headsetCamera.clearFlags = CameraClearFlags.SolidColor;
        headsetCamera.backgroundColor = skyColor;
    }

    private void PlaceMenuInFrontOfPlayer()
    {
        Vector3 headPosition = headsetCamera.transform.position;
        Quaternion headYaw = Quaternion.Euler(0f, headsetCamera.transform.eulerAngles.y, 0f);

        Vector3 stadiumCenter = new Vector3(headPosition.x, environment.position.y, headPosition.z);
        Vector3 menuPosition = headPosition + headYaw * Vector3.forward * distanceFromHead;

        environment.SetPositionAndRotation(stadiumCenter, headYaw);
        menuCanvas.SetPositionAndRotation(menuPosition, headYaw);
    }

    private void AnimateBall()
    {
        float time = Time.unscaledTime;
        Vector2 bob = new Vector2(0f, Mathf.Sin(time * BallBobSpeed) * BallBobHeight);
        float rock = Mathf.Sin(time * BallRockSpeed) * BallRockDegrees;

        animatedBall.anchoredPosition = ballRestPosition + bob;
        animatedBall.localRotation = Quaternion.Euler(0f, 0f, rock);
    }

    public void ShowHome() => ShowScreen(MenuScreen.Home);
    public void ShowGameSelect() => ShowScreen(MenuScreen.GameSelect);
    public void ShowSettings() => ShowScreen(MenuScreen.Settings);

    private void ShowScreen(MenuScreen target)
    {
        foreach (ScreenEntry entry in screens)
            entry.root.SetActive(entry.screen == target);
    }

    private void CreateGameButtons()
    {
        foreach (string scenePath in scenePaths)
        {
            Button gameButton = Instantiate(gameButtonTemplate, gameButtonContainer);
            gameButton.GetComponentInChildren<TMP_Text>().text = GetDisplayName(scenePath);
            gameButton.onClick.AddListener(() => LoadGameScene(scenePath));
            gameButton.gameObject.SetActive(true);
        }
    }

    private static string GetDisplayName(string scenePath) =>
        System.IO.Path.GetFileNameWithoutExtension(scenePath).ToUpperInvariant();

    private void LoadGameScene(string scenePath)
    {
        statusLabel.text = "GETTING THE PITCH READY...";
        ScreenFader.LoadScene(scenePath);
    }

    public void VolumeUp() => AdjustVolume(GameSettings.VolumeStep);
    public void VolumeDown() => AdjustVolume(-GameSettings.VolumeStep);

    private void AdjustVolume(float amount)
    {
        GameSettings.AdjustMasterVolume(amount);
        RefreshVolumeLabel();
    }

    private void RefreshVolumeLabel() =>
        volumeLabel.text = $"MASTER VOLUME: {GameSettings.MasterVolumePercent}%";

    public void ExitGame()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        scenePaths = gameScenes.Select(scene => AssetDatabase.GetAssetPath(scene)).ToList();
    }

    [ContextMenu("Add Games To Build Settings")]
    private void AddGamesToBuildSettings()
    {
        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();

        foreach (string scenePath in scenePaths)
            if (!buildScenes.Any(buildScene => buildScene.path == scenePath))
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));

        EditorBuildSettings.scenes = buildScenes.ToArray();
    }
#endif
}
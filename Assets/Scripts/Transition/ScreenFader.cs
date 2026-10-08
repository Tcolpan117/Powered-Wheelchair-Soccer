using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Creates its own persistent object, so there is nothing to set up in scenes.
public class ScreenFader : MonoBehaviour
{
    private const float Duration = 0.5f;
    private const float Distance = 0.5f;

    private static ScreenFader instance;

    private Image image;
    private Camera followCamera;
    private bool busy;

    public static void LoadScene(string scenePath)
    {
        if (instance == null)
            instance = new GameObject("ScreenFader").AddComponent<ScreenFader>();

        if (!instance.busy)
            instance.StartCoroutine(instance.Run(scenePath));
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 100 x 100 units at 0.1 scale = 10 m wide black panel in front of the camera
        transform.localScale = Vector3.one * 0.1f;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);

        GameObject panel = new GameObject("Fade");
        panel.transform.SetParent(transform, false);
        image = panel.AddComponent<Image>();
        image.rectTransform.sizeDelta = new Vector2(100f, 100f);
        image.color = Color.clear;
        image.enabled = false;
    }

    private void LateUpdate()
    {
        if (followCamera == null || !followCamera.isActiveAndEnabled)
            followCamera = Camera.main != null ? Camera.main
                : Camera.allCamerasCount > 0 ? Camera.allCameras[0] : null;

        if (followCamera == null) return;

        transform.SetPositionAndRotation(
            followCamera.transform.position + followCamera.transform.forward * Distance,
            followCamera.transform.rotation);
    }

    private IEnumerator Run(string scenePath)
    {
        busy = true;

        yield return Fade(1f);
        yield return SceneManager.LoadSceneAsync(scenePath);
        yield return null; // let the new scene's first frames (and any hitch) happen while black
        yield return null;
        yield return Fade(0f);

        busy = false;
    }

    private IEnumerator Fade(float target)
    {
        image.enabled = true;
        float start = image.color.a;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Duration)
        {
            image.color = new Color(0f, 0f, 0f, Mathf.Lerp(start, target, t));
            yield return null;
        }

        image.color = new Color(0f, 0f, 0f, target);
        image.enabled = target > 0f;
    }
}

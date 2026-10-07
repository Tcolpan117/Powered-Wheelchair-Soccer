using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class MainMenuPlayMode
{
    static MainMenuPlayMode()
    {
        string[] sceneIds = AssetDatabase.FindAssets("t:Scene");

        foreach (string id in sceneIds)
        {
            string path = AssetDatabase.GUIDToAssetPath(id);

            if (System.IO.Path.GetFileNameWithoutExtension(path)
                != "MainMenu")
            {
                continue;
            }

            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

            return;
        }

        Debug.LogWarning(
            "MainMenu scene not found. Save your menu scene as MainMenu."
        );
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Load a scene with a loading widget.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider slider;
    
    [Tooltip("The title text of the widget.")]
    [SerializeField] private TextMeshProUGUI titleText;
    
    [Tooltip("The progress text (in percentage).")]
    [SerializeField] private TextMeshProUGUI progressText;

    /// <summary>
    /// Load a specific scene
    /// </summary>
    /// <param name="sceneIndex">The index of the scene to load.
    /// See File->Build Settings to find the reference of a scene.</param>
    public void LoadScene(int sceneIndex)
    {
        if (!CheckIfSceneExist(sceneIndex))
        { 
            print("Scene doesn't exist.");
            return;
        }
        
        SetLoadingScreen(sceneIndex);
        StartCoroutine(LoadSceneAsynchronously(sceneIndex));
    }

    /// <summary>
    /// Load a specific scene
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);

        if (!CheckIfSceneExist(sceneIndex))
        {
            print("Scene doesn't exist.");
            return;
        }
        
        SetLoadingScreen(sceneIndex);
        StartCoroutine(LoadSceneAsynchronously(sceneIndex));
    }
    
    /// <summary>
    /// Load the scene in the background so the game doesn't freeze while loading.
    /// Useful in case of a big scene to load.
    /// </summary>
    /// <param name="sceneIndex">The index of the scene to load.
    /// See File->Build Settings to find the reference of a scene.</param>
    /// <returns></returns>
    IEnumerator LoadSceneAsynchronously(int sceneIndex)
    {
        // Start the operation asynchronously
        AsyncOperation sceneOperation = SceneManager.LoadSceneAsync(sceneIndex);

        // While the scene load, update the progress bar
        while (!sceneOperation.isDone)
        {
            float operationProgress = Mathf.Clamp01(sceneOperation.progress / 0.9f);
            slider.value = operationProgress;
            progressText.text = operationProgress + "%";
            
            yield return null;
        }
    }

    /// <summary>
    /// Display the loading widget with the name of the scene that is loading.
    /// </summary>
    /// <param name="sceneIndex">The index of the scene to load.
    /// See File->Build Settings to find the reference of a scene.</param>
    private void SetLoadingScreen(int sceneIndex)
    {
        // Display the scene loader widget
        gameObject.SetActive(true);
        
        // Change the title to add the name of the scene to load
        titleText.text = "Loading scene:\n" + 
                    System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneIndex));
    }

    /// <summary>
    /// Verify if the scene exist before loading the scene.
    /// </summary>
    /// <param name="sceneIndex">The scene index from Build Settings</param>
    /// <returns>True if the scene exist in Build Settings.</returns>
    private bool CheckIfSceneExist(int sceneIndex)
    {
        var sceneCount = SceneManager.sceneCountInBuildSettings;

        return sceneIndex >= 0 && sceneIndex < sceneCount;
    }

}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour
{
    [Header("Progress Settings")]
    public Slider progressSlider;
    public int totalTreasures = 3;

    [Header("Scene Transition")]
    public string nextSceneName;
    public float delayBeforeLoad = 1.5f;

    private int progressAmount;
    private bool levelComplete;

    void Start()
    {
        if (progressSlider == null)
        {
            progressSlider = FindObjectOfType<Slider>();
        }

        progressAmount = 0;
        levelComplete = false;

        if (progressSlider != null)
        {
            progressSlider.maxValue = totalTreasures;
            progressSlider.value = 0;
        }

        Gem.OnGemCollect += IncreaseProgressAmount;
    }

    void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;

        if (progressSlider != null)
        {
            progressSlider.value = progressAmount;
        }

        if (progressAmount >= totalTreasures && !levelComplete)
        {
            levelComplete = true;
            Debug.Log("Level Complete!");
            StartCoroutine(LoadNextScene());
        }
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("No next scene! This was the last level.");
            }
        }
    }

    void OnDestroy()
    {
        Gem.OnGemCollect -= IncreaseProgressAmount;
    }
}

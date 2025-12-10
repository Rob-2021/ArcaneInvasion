using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Level : MonoBehaviour
{

    public static Level instance;

    int numDestructables = 0;
    bool startNextLevel = false;
    float nextLevelTimer = 3.0f;

    string[] levels = { "Level1", "Level2", "Level3" };
    int currentLevel = 1;

    int score = 0;
    Text scoreText;
    TMP_Text scoreTextTMP;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // Try to find the ScoreText in the current scene (may not exist yet)
            var go = GameObject.Find("ScoreText");
            if (go != null)
            {
                scoreText = go.GetComponent<Text>();
                scoreTextTMP = go.GetComponent<TMP_Text>();
            }
            // Ensure we update the reference when a new scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid potential memory leaks or null callbacks
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Level: OnSceneLoaded called for scene '{scene.name}' (currentLevel={currentLevel})");
        // After a scene loads, try to find the ScoreText in the newly loaded scene
        var go = GameObject.Find("ScoreText");
        if (go != null)
        {
            scoreText = go.GetComponent<Text>();
            scoreTextTMP = go.GetComponent<TMP_Text>();
            // Update the UI with the current score when available
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
            else if (scoreTextTMP != null)
            {
                scoreTextTMP.text = score.ToString();
            }
        }
        else
        {
            scoreText = null;
            scoreTextTMP = null;
        }

        // Fix potential duplicate EventSystem issue: keep only one active EventSystem
        var eventSystems = GameObject.FindObjectsOfType<EventSystem>();
        if (eventSystems != null && eventSystems.Length > 1)
        {
            Debug.LogWarning($"There are {eventSystems.Length} EventSystem instances — removing duplicates.");
            // Keep the first one, destroy the rest
            for (int i = 1; i < eventSystems.Length; i++)
            {
                // Only destroy if not null and not the same object
                if (eventSystems[i] != null)
                {
                    Destroy(eventSystems[i].gameObject);
                }
            }
        }
        Debug.Log($"Level: OnSceneLoaded finished. numDestructables={numDestructables}");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (startNextLevel)
        {
            if (nextLevelTimer <= 0)
            {
                currentLevel++;
                Debug.Log($"Level: Advancing to level {currentLevel}");
                if (currentLevel <= levels.Length)
                {
                    string sceneName = levels[currentLevel - 1];
                    Debug.Log($"Level: Loading scene '{sceneName}' (index {currentLevel - 1})");
                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    Debug.Log("GAME OVER!!!");
                }

                nextLevelTimer = 3.0f;
                startNextLevel = false;
            }
            else
            {
                nextLevelTimer -= Time.deltaTime;
            }
        }
    }

    public void ResetLevel()
    {
        foreach(Bullet b in GameObject.FindObjectsOfType<Bullet>()){
            Destroy(b.gameObject);
        }
        Debug.Log("Level: ResetLevel called — clearing bullets, score, and resetting level state.");
        numDestructables = 0;
        score = 0;
        // Ensure UI shows zero immediately
        if (scoreText != null) scoreText.text = "0";
        if (scoreTextTMP != null) scoreTextTMP.text = "0";
        // Cancel any pending level advance
        startNextLevel = false;
        nextLevelTimer = 3.0f;
        string sceneName = levels[Mathf.Clamp(currentLevel - 1, 0, levels.Length - 1)];
        Debug.Log($"Level: Reloading scene '{sceneName}' (currentLevel={currentLevel})");
        SceneManager.LoadScene(sceneName);
    }

    public void AddScore(int amountToAdd)
    {
        score += amountToAdd;
        // If the UI text reference is missing, try to find it now (scene may have changed)
        if (scoreText == null)
        {
            var go = GameObject.Find("ScoreText");
            if (go != null)
            {
                scoreText = go.GetComponent<Text>();
                scoreTextTMP = go.GetComponent<TMP_Text>();
            }
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        else if (scoreTextTMP != null)
        {
            scoreTextTMP.text = score.ToString();
        }
    }

    public void AddDestructable()
    {
        numDestructables++;
        Debug.Log($"Level: AddDestructable -> count = {numDestructables}");
    }

    public void RemoveDestructable()
    {
        if (numDestructables > 0)
        {
            numDestructables--;
        }
        else
        {
            Debug.LogWarning("Level: RemoveDestructable called but count is already 0");
        }

        Debug.Log($"Level: RemoveDestructable -> count = {numDestructables}");

        if (numDestructables <= 0)
        {
            numDestructables = 0;
            startNextLevel = true;
            Debug.Log("Level: All destructables cleared. Starting next level timer.");
        }
    }

}

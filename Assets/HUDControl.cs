﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class HUDControl : MonoBehaviour
{
    private int rewinds;
    private TextMeshProUGUI rewindsDisplay;

    private float timer;
    private bool timerStopped = false;
    private TextMeshProUGUI timerDisplay;
    private AudioManager audioManager;
    private GameObject restartPrompt;
    private GameObject instructions;
    private GameObject pauseMenu;
    private GameObject clearedPopup;
    private TextMeshProUGUI clearedStats;
    private TextMeshProUGUI clearedHighscore;
    private GameObject levelSelectPopup;
    private String sceneName;
    private StarThresholds starThresholds;
    public bool paused = false;
    public Sprite emptyStar;

    private List<string> levelOrder = new List<string>() { "E1", "E2", "E3", "E4", "E5", "M1", "M2", "M3", "M4", "M5", "H1", "H2", "H3", "H4", "H5" };
    private GameObject[] ministars;
    private int currentLevel;
    private TextMeshProUGUI currentLevelDisplay;

    void Start()
    {
        ministars = GameObject.FindGameObjectsWithTag("Ministars");
        rewindsDisplay = transform.Find("Rewinds").GetComponent<TextMeshProUGUI>();
        timerDisplay = transform.Find("Timer").GetComponent<TextMeshProUGUI>();
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        restartPrompt = transform.Find("RestartPrompt").gameObject;
        restartPrompt.SetActive(false);
        instructions = transform.Find("Instructions").gameObject;
        instructions.SetActive(false);
        pauseMenu = transform.Find("Pause Menu").gameObject;
        pauseMenu.SetActive(false);
        clearedPopup = transform.Find("Cleared Popup").gameObject;
        clearedStats = clearedPopup.transform.Find("Stats").GetComponent<TextMeshProUGUI>();
        clearedHighscore = clearedPopup.transform.Find("Highscore").GetComponent<TextMeshProUGUI>();
        clearedPopup.SetActive(false);
        levelSelectPopup = transform.Find("Level Select").gameObject;
        levelSelectPopup.SetActive(false);
        sceneName = SceneManager.GetActiveScene().name;
        starThresholds = GameObject.Find("StarThresholds").GetComponent<StarThresholds>();

        currentLevel = levelOrder.IndexOf(SceneManager.GetActiveScene().name);
        currentLevelDisplay = transform.Find("Current Level").GetComponent<TextMeshProUGUI>();

        if (currentLevel <= 4)
        {
            currentLevelDisplay.text = "Level: Easy " + (currentLevel + 1);
        }
        else if (currentLevel <= 9)
        {
            currentLevelDisplay.text = "Level: Medium " + (currentLevel % 5 + 1);
        }
        else
        {
            currentLevelDisplay.text = "Level: Hard " + (currentLevel % 5 + 1);
        }
        UpdateAllLevelMinistars();
    }

    public void IncreaseRewinds()
    {
        rewinds++;
        rewindsDisplay.text = "Rewinds: " + rewinds;
    }

    void UpdateTimer()
    {
        if (timerStopped == false)
        {
            timer += Time.deltaTime;
            if (timer >= 20f && sceneName != "Main Menu") PromptRestart();
            timerDisplay.text = "TIME: " + Math.Round(timer, 3);
        }
    }
    public void StopTimer() { timerStopped = true; }
    public float GetTimer() { return timer; }
    void CheckInput()
    {
        // Listen for restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            Unpause();
            audioManager.Play("RestartLevel");
            Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
        }

        // Listen for pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (paused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
    }
    public void PromptRestart() { restartPrompt.SetActive(true); }
    public void Pause()
    {
        Time.timeScale = 0;
        paused = true;
        pauseMenu.SetActive(true);
    }
    public void LevelCleared()
    {
        string levelKey = "Level" + SceneManager.GetActiveScene().name;
        float bestTime = PlayerPrefs.GetFloat(levelKey + "BestTime", 19.999f);
        int bestRewinds = PlayerPrefs.GetInt(levelKey + "BestRewinds", 14);
        float highscore = PlayerPrefs.GetFloat(levelKey + "Highscore", 0);
        float score = (19.999f - timer) * (14 - rewinds) * 10;
        if (PlayerPrefs.GetInt(levelKey + "Stars", 0) == 0) PlayerPrefs.SetInt("LevelsBeaten", PlayerPrefs.GetInt("LevelsBeaten", 0) + 1);
        int stars = starThresholds.HowManyStars(score);
        for (int i = 2; i > stars - 1; i--)
        {
            clearedPopup.transform.Find("Stars").GetChild(i).GetComponent<Image>().sprite = emptyStar;
        }
        if (highscore < score)
        {
            PlayerPrefs.SetFloat(levelKey + "Highscore", score);
            highscore = score;
            PlayerPrefs.SetFloat(levelKey + "BestTime", timer);
            bestTime = timer;
            PlayerPrefs.SetInt(levelKey + "BestRewinds", rewinds);
            bestRewinds = rewinds;
            PlayerPrefs.SetInt(levelKey + "Stars", stars);
            clearedStats.text = "<size=60><color=#ffa726>new highscore!</color></size>";
            clearedHighscore.text = "<color=#ffa726>Best clear</color>: <color=#91a7ff>" + Math.Round(bestTime, 3) + " seconds</color> using <color=#42bd41>" + bestRewinds + " rewinds</color>\nHighscore: " + Math.Round(highscore, 0);
        }
        else
        {
            clearedStats.text = "Cleared in <color=#91a7ff>" + Math.Round(timer, 3) + " seconds</color> using <color=#42bd41>" + rewinds + " rewinds</color>\nTotal score: " + Math.Round(score, 0);
            clearedHighscore.text = "<color=#ffa726>Best clear</color>: <color=#91a7ff>" + Math.Round(bestTime, 3) + " seconds</color> using <color=#42bd41>" + bestRewinds + " rewinds</color>\nHighscore: " + Math.Round(highscore, 0);
        }
        clearedPopup.SetActive(true);
    }
    public void GoToNextLevel()
    {
        Unpause();
        if (currentLevel == levelOrder.Count - 1)
            SceneManager.LoadScene("Main Menu");
        else
            SceneManager.LoadScene(levelOrder[currentLevel + 1]);
        PlayerPrefs.SetString("Loaded", "false");

    }
    public void Unpause()
    {
        Time.timeScale = 1;
        paused = false;
        pauseMenu.SetActive(false);
        DisableInstructions();
    }
    public void GoToHomeMenu()
    {
        Unpause();
        SceneManager.LoadScene("Main Menu");
        PlayerPrefs.SetString("Loaded", "false");

    }
    public void EnableInstructions()
    {
        instructions.SetActive(true);
    }
    public void DisableInstructions() { instructions.SetActive(false); }
    public void EnableLevelSelect()
    {
        UpdateAllLevelMinistars();
        levelSelectPopup.SetActive(true);
    }
    public void DisableLevelSelect() { levelSelectPopup.SetActive(false); }
    public void SelectLevel(string sceneName)
    {
        Unpause();
        SceneManager.LoadScene(sceneName);
        PlayerPrefs.SetString("Loaded", "false");
        StartCoroutine(GameObject.Find("LevelLoader").GetComponent<LevelLoader>().fade(sceneName));

    }
    public void UpdateAllLevelMinistars()
    {
        int levelsBeaten = 0;
        foreach (GameObject m in ministars)
        {
            m.GetComponent<MinistarsControl>().UpdateMinistars();
            if (m.GetComponent<MinistarsControl>().stars > 0) 
                levelsBeaten ++;
        }
        PlayerPrefs.SetInt("LevelsBeaten", levelsBeaten);
        
    }
    void Update()
    {
        UpdateTimer();
        CheckInput();
    }
}

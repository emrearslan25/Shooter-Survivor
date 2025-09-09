using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Player UI")]
    public Slider healthBar;
    public Text healthText;
    public Text levelText;
    public Text timerText;

    [Header("Game UI")]
    public Text enemyCountText;
    public Text scoreText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject upgradePanel;

    // References
    private PlayerController player;
    private GameManager gameManager;
    private ExperienceSystem experienceSystem;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        experienceSystem = FindObjectOfType<ExperienceSystem>();

        // Hide panels initially
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    void Update()
    {
        UpdatePlayerUI();
        UpdateGameUI();
    }

    void UpdatePlayerUI()
    {
        if (player != null)
        {
            // Health
            if (healthBar != null)
                healthBar.value = player.GetHealthPercentage();

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(player.currentHealth)} / {player.maxHealth}";
        }

        // Level
        if (experienceSystem != null && levelText != null)
            levelText.text = $"Level {experienceSystem.GetCurrentLevel()}";

        // Timer
        if (gameManager != null && timerText != null)
        {
            float time = gameManager.GetGameTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void UpdateGameUI()
    {
        if (gameManager != null)
        {
            // Enemy count
            if (enemyCountText != null)
                enemyCountText.text = $"Enemies: {gameManager.GetEnemies().Count}";

            // Score (based on time survived + enemies killed)
            if (scoreText != null)
            {
                int score = Mathf.FloorToInt(gameManager.GetGameTime() * 10);
                scoreText.text = $"Score: {score}";
            }
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void OnGUI()
    {
        // Simple pause menu toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f)
                ShowPauseMenu();
            else
                HidePauseMenu();
        }
    }
}

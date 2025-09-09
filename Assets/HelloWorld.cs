using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameAreaWidth = 20f;
    public float gameAreaHeight = 20f;
    public int maxEnemies = 50;
    public float enemySpawnRate = 2f;

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public Vector2 playerStartPosition = Vector2.zero;

    [Header("Prefabs")]
    public GameObject[] enemyPrefabs;
    public GameObject[] weaponPrefabs;

    [Header("UI")]
    public GameObject gameUI;
    public GameObject upgradePanel;

    // Game State
    private PlayerController player;
    private List<Enemy> enemies = new List<Enemy>();
    private WeaponSystem weaponSystem;
    private ExperienceSystem experienceSystem;
    private UpgradeSystem upgradeSystem;

    private float spawnTimer = 0f;
    private bool isGameRunning = false;
    private float gameTime = 0f;

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (!isGameRunning) return;

        gameTime += Time.deltaTime;
        UpdateSpawning();
        UpdateGameState();
    }

    void InitializeGame()
    {
        // Create player
        if (playerPrefab != null)
        {
            GameObject playerObj = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
            player = playerObj.GetComponent<PlayerController>();
        }

        // Initialize systems
        weaponSystem = GetComponent<WeaponSystem>();
        experienceSystem = GetComponent<ExperienceSystem>();
        upgradeSystem = GetComponent<UpgradeSystem>();

        // Start game
        isGameRunning = true;

        if (gameUI != null)
            gameUI.SetActive(true);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    void UpdateSpawning()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= enemySpawnRate && enemies.Count < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = 0f;

            // Increase spawn rate over time
            enemySpawnRate = Mathf.Max(0.5f, enemySpawnRate - 0.01f);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        // Random enemy type
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        // Random position around the player (2D)
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(player.transform);
            enemies.Add(enemy);
        }
    }

    Vector2 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(8f, 12f);

        Vector2 offset = new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance
        );

        return (Vector2)player.transform.position + offset;
    }

    void UpdateGameState()
    {
        // Remove dead enemies
        enemies.RemoveAll(e => e == null || e.IsDead());

        // Check for level up
        if (experienceSystem != null && experienceSystem.ShouldLevelUp())
        {
            PauseGameForUpgrade();
        }
    }

    void PauseGameForUpgrade()
    {
        isGameRunning = false;
        Time.timeScale = 0f;

        if (upgradePanel != null)
            upgradePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isGameRunning = true;
        Time.timeScale = 1f;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    public PlayerController GetPlayer() => player;
    public List<Enemy> GetEnemies() => enemies;
    public float GetGameTime() => gameTime;
    public bool IsGameRunning() => isGameRunning;

    public void GameOver()
    {
        isGameRunning = false;
        Debug.Log("Game Over!");
        // Show game over screen
    }
}

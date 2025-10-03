using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameAreaWidth = 20f;
    public float gameAreaHeight = 20f;
    public int maxEnemies = 50;
    public float enemySpawnRate = 2f;

    [Header("Score System")]
    private static string currentPlayerName = "Oyuncu";
    private float gameStartTime;
    private int currentScore = 0;
    private bool gameEnded = false;

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
        gameStartTime = Time.time;
        
        // Create player (prefab or runtime)
        if (playerPrefab != null)
        {
            GameObject playerObj = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
            player = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            player = CreateRuntimePlayer(playerStartPosition);
        }

    // Initialize systems (be robust even if they are on different GameObjects)
    weaponSystem = GetComponent<WeaponSystem>();
    if (weaponSystem == null) weaponSystem = FindObjectOfType<WeaponSystem>();

    experienceSystem = GetComponent<ExperienceSystem>();
    if (experienceSystem == null) experienceSystem = FindObjectOfType<ExperienceSystem>();

    upgradeSystem = GetComponent<UpgradeSystem>();
    if (upgradeSystem == null) upgradeSystem = FindObjectOfType<UpgradeSystem>();

        // Start game
        isGameRunning = true;

        if (gameUI != null)
            gameUI.SetActive(true);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        // Ensure camera follows player
        var cam = Camera.main;
        if (cam != null)
        {
            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = player != null ? player.transform : null;
        }
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
        // Random position around the player (2D)
        Vector2 spawnPos = GetRandomSpawnPosition();

        // Prefab kullanımı öncelikli
        if (enemyPrefabs != null && enemyPrefabs.Length > 0 && enemyPrefabs[0] != null)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemyPrefab = enemyPrefabs[randomIndex];
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Initialize(player.transform);
                enemies.Add(enemy);
            }
        }
        else
        {
            // Runtime creation (şu anki sistem)
            Enemy enemy = CreateRuntimeEnemy(spawnPos);
            if (enemy != null)
            {
                enemy.Initialize(player.transform);
                enemies.Add(enemy);
            }
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
    // Re-resolve systems if missing
    if (experienceSystem == null) experienceSystem = FindObjectOfType<ExperienceSystem>();
    if (upgradeSystem == null) upgradeSystem = FindObjectOfType<UpgradeSystem>();
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
        // Stop game loop & pause time
        isGameRunning = false;
        Time.timeScale = 0f;

        // Delegate showing randomized options to UpgradeSystem
        if (upgradeSystem == null)
            upgradeSystem = GetComponent<UpgradeSystem>();
        if (upgradeSystem != null)
        {
            upgradeSystem.OnLevelUp();
        }
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
        if (gameEnded) return;
        gameEnded = true;
        
        isGameRunning = false;
        
        // Calculate final score and survival time
        float survivalTime = Time.time - gameStartTime;
        CalculateFinalScore(survivalTime);
        
        // Save high score
        int playerLevel = experienceSystem != null ? experienceSystem.GetCurrentLevel() : 1;
        ScoreManager.SaveScore(currentPlayerName, currentScore, survivalTime, playerLevel);
        
        Debug.Log($"Game Over! Score: {currentScore}, Time: {survivalTime:F1}s");
        
        // Show game over screen
        ShowGameOverScreen();
    }

    // ---------- Runtime Creation Helpers ----------
    PlayerController CreateRuntimePlayer(Vector2 position)
    {
        GameObject go = new GameObject("Player");
        go.transform.position = position;
        
        // PLAYER BOYUT - STANDART (1.0 scale)
        go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        
        var rb2d = go.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0f;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Player collider - visual ile tam eşleşsin
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f; // 1.0 scale için yarısı

        var pc = go.AddComponent<PlayerController>();
        pc.manualControl = true;
        pc.moveSpeed = 6f;
        pc.useSimpleShooting = true;

        // Visual - 1.0 scale boyutunda
        var visual = CreateCircleVisual("PlayerVisual", go.transform, new Color(0.1f, 0.9f, 0.9f, 1f), 0.5f, 28, 0.06f);
        var vr = visual.GetComponent<Renderer>(); if (vr != null) vr.sortingOrder = 50;
        pc.model = visual;

        // FirePoint
        var firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(go.transform, false);
        firePoint.transform.localPosition = new Vector3(0.6f, 0f, 0f);
        pc.firePoint = firePoint.transform;

        return pc;
    }

    Enemy CreateRuntimeEnemy(Vector2 position)
    {
        // ENEMY TİPLERİNE GÖRE AĞIRLIKLI SPAWN
        EnemyType[] types = { EnemyType.Circle, EnemyType.Triangle, EnemyType.Square };
        EnemyType selectedType = types[Random.Range(0, types.Length)];
        
        // Yeni spawn oranları: %45 Circle, %45 Triangle, %10 Square
        // (Mavi düşman spawn oranı düşürüldü)
        float rand = Random.Range(0f, 1f);
        if (rand < 0.45f) selectedType = EnemyType.Circle;        // Kırmızı çember
        else if (rand < 0.9f) selectedType = EnemyType.Triangle; // Sarı üçgen  
        else selectedType = EnemyType.Square;                    // Büyük mavi kare

        GameObject go = new GameObject($"Enemy_{selectedType}");
        go.transform.position = position;
        
        var rb2d = go.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0f;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Başlangıç collider - enemy type setup'ında düzeltilecek
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f; // Geçici, her tip kendi ayarlayacak

        var enemy = go.AddComponent<Enemy>();
        enemy.enemyType = selectedType;

        // Enemy kendi boyutunu ve collider'ını ayarlayacak
        enemy.Initialize(player.transform);
        
        return enemy;
    }

    GameObject CreateCircleVisual(string name, Transform parent, Color color, float radius, int segments, float width)
    {
        GameObject v = new GameObject(name);
        v.transform.SetParent(parent, false);
        var lr = v.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = Mathf.Max(segments, 3);
        lr.widthMultiplier = width;
        lr.startColor = color; lr.endColor = color;
    var m = new Material(Shader.Find("Sprites/Default"));
    m.renderQueue = 3000; // Transparent
    lr.material = m;
        for (int i = 0; i < lr.positionCount; i++)
        {
            float a = (i / (float)lr.positionCount) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }
        return v;
    }

    // Score and player name management
    public static void SetCurrentPlayerName(string name)
    {
        currentPlayerName = name;
    }

    public static string GetCurrentPlayerName()
    {
        return currentPlayerName;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    void CalculateFinalScore(float survivalTime)
    {
        // Base score from survival time
        int timeScore = Mathf.FloorToInt(survivalTime * 10f);
        
        // Bonus from level
        int levelBonus = experienceSystem != null ? experienceSystem.GetCurrentLevel() * 100 : 0;
        
        // Bonus from enemies killed (approximate)
        int enemyBonus = Mathf.FloorToInt(survivalTime * enemySpawnRate * 50f);
        
        currentScore = timeScore + levelBonus + enemyBonus;
    }

    void ShowGameOverScreen()
    {
        // Try to show game over panel via UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
        
        // Alternative: Load main menu after delay
        Invoke("ReturnToMainMenu", 3f);
    }

    void ReturnToMainMenu()
    {
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

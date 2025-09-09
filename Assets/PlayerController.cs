using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Control Settings")]
    public bool manualControl = true; // WASD + mouse aim
    [Header("Simple Shooting")]
    public bool useSimpleShooting = true; // if true, ignore WeaponSystem and shoot basic projectile on LMB
    public Transform firePoint; // optional; if null, use player position
    public GameObject projectilePrefab; // optional; if null, create runtime projectile
    public float simpleFireRate = 6f;
    public float simpleProjectileSpeed = 16f;
    public float simpleDamage = 10f;
    private float _lastSimpleShotTime = 0f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float pickupRange = 2f;

    [Header("Combat Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageCooldown = 1f;

    [Header("Visual")]
    public GameObject model;
    public ParticleSystem damageEffect;

    // Components
    private Rigidbody2D rb;
    private WeaponSystem weaponSystem;
    private ExperienceSystem experienceSystem;

    // State
    private Vector2 moveDirection;
    private float lastDamageTime = 0f;
    private bool isAlive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        weaponSystem = FindObjectOfType<WeaponSystem>();
        experienceSystem = FindObjectOfType<ExperienceSystem>();

        currentHealth = maxHealth;

        // Auto-pickup system
        InvokeRepeating("AutoPickupItems", 0f, 0.1f);

        // Disable WeaponSystem if using simple shooting
        if (useSimpleShooting && weaponSystem != null)
        {
            weaponSystem.enabled = false;
        }
    }

    void Update()
    {
        if (!isAlive) return;
        if (manualControl)
        {
            HandleInputMovement();
            HandleMouseAimAndFire();
        }
        else
        {
            HandleMovement();
            HandleAutoAim();
        }
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        // Apply movement (2D)
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    // Manual Input Movement using WASD/Arrow keys
    void HandleInputMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        Vector2 input = new Vector2(h, v);
        moveDirection = input.sqrMagnitude > 1f ? input.normalized : input;
    }

    // Mouse aim and left-click fire
    void HandleMouseAimAndFire()
    {
        // Rotate to mouse
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = Camera.main != null ? Camera.main.ScreenToWorldPoint(mouseScreen) : Vector3.zero;
        mouseWorld.z = 0f;

        Vector2 toMouse = (mouseWorld - transform.position);
        if (toMouse.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Fire while holding LMB
        if (Input.GetMouseButton(0))
        {
            if (useSimpleShooting)
            {
                TrySimpleFire(mouseWorld);
            }
            else if (weaponSystem != null && weaponSystem.isActiveAndEnabled)
            {
                weaponSystem.ManualFireAt(mouseWorld);
            }
        }
    }

    void TrySimpleFire(Vector3 targetWorld)
    {
        if (Time.time - _lastSimpleShotTime < 1f / simpleFireRate) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 dir = ((Vector2)targetWorld - (Vector2)origin).normalized;

        GameObject proj = projectilePrefab != null ? Instantiate(projectilePrefab, origin, Quaternion.identity)
                                                   : CreateRuntimeProjectile(origin);
        if (proj == null) return;

        var comp = proj.GetComponent<Projectile>();
        if (comp != null)
        {
            comp.Initialize(dir, simpleProjectileSpeed, simpleDamage);
        }
        else
        {
            var rb2d = proj.GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.velocity = dir * simpleProjectileSpeed;
        }

        _lastSimpleShotTime = Time.time;
    }

    GameObject CreateRuntimeProjectile(Vector3 spawnPos)
    {
        var go = new GameObject("SimpleProjectile");
        go.transform.position = spawnPos;
        var sr = go.AddComponent<SpriteRenderer>();
        // Create a tiny white square sprite at runtime so it's always visible
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        var cols = new Color32[8 * 8];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(cols);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.color = Color.white; // head color
        // Ensure visible in 2D sorting
        sr.sortingOrder = 10;

        // Add a neon trail for visibility
        var tr = go.AddComponent<TrailRenderer>();
        tr.time = 0.18f;
        tr.minVertexDistance = 0.01f;
        tr.startWidth = 0.10f;
        tr.endWidth = 0.02f;
        var neon = new Gradient();
        neon.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0f, 1f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.4f, 1f, 1f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        tr.colorGradient = neon;
    var mat = new Material(Shader.Find("Sprites/Default"));
    mat.renderQueue = 3000; // Transparent
    tr.material = mat;
    sr.material = mat;
        var circle = go.AddComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = 0.12f;
        var rb2d = go.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0f;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        go.AddComponent<Projectile>();
        return go;
    }

    void HandleMovement()
    {
        // Auto-move towards nearest enemy or random direction
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            List<Enemy> enemies = gameManager.GetEnemies();

            if (enemies.Count > 0)
            {
                // Move towards center of enemies (2D)
                Vector2 centerOfEnemies = Vector2.zero;
                foreach (Enemy enemy in enemies)
                {
                    if (enemy != null)
                        centerOfEnemies += (Vector2)enemy.transform.position;
                }
                centerOfEnemies /= enemies.Count;

                moveDirection = (centerOfEnemies - (Vector2)transform.position).normalized;
            }
            else
            {
                // Random movement when no enemies
                float time = Time.time;
                moveDirection = new Vector2(
                    Mathf.Sin(time * 0.5f),
                    Mathf.Cos(time * 0.5f)
                ).normalized;
            }
        }

        // Rotate to face movement direction (2D)
    if (moveDirection != Vector2.zero && !manualControl)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void HandleAutoAim()
    {
        // Find nearest enemy for auto-aim
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            List<Enemy> enemies = gameManager.GetEnemies();
            Enemy nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead())
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }

            // Aim at nearest enemy
            if (nearestEnemy != null)
            {
                Vector2 aimDirection = ((Vector2)nearestEnemy.transform.position - (Vector2)transform.position).normalized;
                // Weapon system will handle the actual firing
            }
        }
    }

    void AutoPickupItems()
    {
        // Find nearby experience orbs and pick them up (2D)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRange);

        foreach (Collider2D collider in colliders)
        {
            ExperienceOrb orb = collider.GetComponent<ExperienceOrb>();
            if (orb != null)
            {
                orb.Pickup(this);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        // Damage effect
        if (damageEffect != null)
        {
            damageEffect.Play();
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.FlashDamage();
        }

        // Screen shake effect (2D)
        Camera.main.transform.position += (Vector3)Random.insideUnitCircle * 0.1f;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isAlive = false;

        // Death animation
        if (model != null)
        {
            model.SetActive(false);
        }

        // Game over
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public bool IsAlive() => isAlive;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public Vector2 GetMoveDirection() => moveDirection;
    public bool IsSimpleShootingEnabled() => useSimpleShooting;
}
